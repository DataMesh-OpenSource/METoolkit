using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using MEHoloClient.Entities;
using MEHoloClient.Sync;
using MEHoloClient.Interface.Sync;
using MEHoloClient.Utils;
using MEHoloClient.UDP;
using DataMesh.AR.Anchor;
using DataMesh.AR.Utility;
using System;

#if  UNITY_METRO && !UNITY_EDITOR
using System.Threading.Tasks;
using System.Threading;
#endif

namespace DataMesh.AR.SpectatorView
{
    public class LiveHololens : DataMesh.AR.MEHoloModuleSingleton<LiveHololens>
    {
        /// <summary>
        /// 识别版本号，只用于和Live通讯时使用 
        /// 连接上Live后会发这个版本号过去，Live会和本地的版本号对比 
        /// </summary>
        public static short version = 4;

        /// <summary>
        /// PC端的IP
        /// </summary>
        public string liveIp;

        /// <summary>
        /// PC端的端口
        /// </summary>
        public int livePort;


        private SyncClient syncClient;


#if  UNITY_METRO && !UNITY_EDITOR

        private string socketUrl;

        private Transform mainCameraTransform;
        private SceneAnchorController anchorController;

        private bool synchronizing = false;
        private int syncIndex = 0;

        private bool useUDP = true;
        private int udpPort;
        private UdpSender sender;

        private Log.LogManager logManager;
        private string currentLogName;

        private int count = 0;
        private string logStr = "";


        /// <summary>
        /// 初始化
        /// </summary>
        protected override void _Init()
        {
            anchorController = SceneAnchorController.Instance;
            mainCameraTransform = Camera.main.transform;

            logManager = Log.LogManager.Instance;

        }

        /// <summary>
        /// 开启服务
        /// </summary>
        protected override void _TurnOn()
        {
            // 加载配置文件
            Debug.Log("config source=" + AppConfig.Instance.configFileSourcePath);
            Debug.Log("config target=" + AppConfig.Instance.configFilePath);
            Debug.Log("Load config file " + MEHoloConstant.LiveAgentConfigFile);
            AppConfig.Instance.LoadConfig(MEHoloConstant.LiveAgentConfigFile);
            liveIp = AppConfig.Instance.GetConfigByFileName(MEHoloConstant.LiveAgentConfigFile, "Live_Ip");
            livePort = int.Parse(AppConfig.Instance.GetConfigByFileName(MEHoloConstant.LiveAgentConfigFile, "Live_Port"));

            // 连接WorkStation
            socketUrl = "ws://" + liveIp + ":" + livePort + MEHoloConstant.LiveServerHandlerName;

            syncClient = new SyncClient(socketUrl, true, 100);
            syncClient.StartClient();

            Debug.Log("start connect: " + socketUrl);


            // 关闭其他服务，以节省效能 
            DataMesh.AR.Interactive.MultiInputManager.Instance.StopCapture();
        }

        protected override void _TurnOff()
        {
            
        }

        private LiveMessageSynchronizeAll CreateSyncMessage()
        {
            LiveMessageSynchronizeAll msg = new LiveMessageSynchronizeAll();
            msg.seq = syncIndex;
            syncIndex++;
            msg.position = mainCameraTransform.transform.position;
            msg.rotation = mainCameraTransform.transform.rotation;

            msg.anchorCount = anchorController.anchorObjectList.Count;
            msg.anchorPositionList = new Vector3[msg.anchorCount];
            msg.anchorRotationList = new Quaternion[msg.anchorCount];
            msg.anchorIsLocated = new bool[msg.anchorCount];
            for (int i = 0; i < anchorController.anchorObjectList.Count; i++)
            {
                AnchorObjectInfo info = anchorController.anchorObjectList[i];
                msg.anchorPositionList[i] = info.rootTrans.position;
                msg.anchorRotationList[i] = info.rootTrans.rotation;
                if (info.anchor != null)
                    msg.anchorIsLocated[i] = info.anchor.isLocated;
                else
                    msg.anchorIsLocated[i] = false;
            }

            return msg;
        }
         
        // Update is called once per frame
        void Update()
        {

            if (syncClient != null)
            {
                if (syncClient.Running)
                {
                    while (syncClient.SyncQueue.GetCount() > 0)
                    {
                        byte[] messageBytes = syncClient.SyncQueue.Dequeue();

                        // 处理消息 
                        LiveMessage msg = LiveMessageManager.ParseMessage(messageBytes);
                        //Debug.Log("msg type=" + msg.type);
                        switch (msg.type)
                        {
                            case LiveMessageConstant.BEV_MESSAGE_TYPE_START:
                                synchronizing = true;
                                break;
                            case LiveMessageConstant.BEV_MESSAGE_TYPE_STOP:
                                synchronizing = false;
                                break;
                            case LiveMessageConstant.BEV_MESSAGE_TYPE_SET_ANCHOR:
                                LiveMessageSetAnchor msgSetAnchor = msg as LiveMessageSetAnchor;
                                // 这里也不能再同步了
                                synchronizing = false;
                                SetAnchors(msgSetAnchor);

                                // 重置同步消息序号 
                                syncIndex = 0;

                                break;
                            case LiveMessageConstant.BEV_MESSAGE_TYPE_SAVE_ANCHOR:
                                LiveMessageSaveAnchor msgSaveAnchor = msg as LiveMessageSaveAnchor;
                                // 这里也不能再同步了
                                synchronizing = false;
                                SaveAnchors(msgSaveAnchor);
                                break;
                            case LiveMessageConstant.BEV_MESSAGE_TYPE_DOWNLOAD_ANCHOR:
                                DownloadAnchor();
                                break;
                            case LiveMessageConstant.BEV_MESSAGE_TYPE_REQUEST_SPATIAL_MAPPING:
                                SendSpatialMapping();
                                break;
                        }
                    }


                }
            }

        }

        async void LateUpdate()
        {
            // 如果需要同步，则发送摄影机和所有Anchor位置 
            if (synchronizing)
            {
                if (!useUDP)
                {
                    if (syncClient != null)
                    {
                        if (syncClient.Running)
                        {
                            LiveMessageSynchronizeAll msg = CreateSyncMessage();
                            byte[] msgData = msg.Serialize();

                            // 记录Log 
                            logManager.Log(currentLogName, msg.FormatLogString());

                            await syncClient.SendMessage(msgData);
                        }
                    }
                }
                else
                {
                    if (sender != null && sender.Running)
                    {
                        LiveMessageSynchronizeAll msg = CreateSyncMessage();
                        byte[] msgData = msg.Serialize();

                        // 记录Log 
                        logManager.Log(currentLogName, msg.FormatLogString());

                        try
                        {
                            //Debug.Log("msgData length = " + msgData.Length);
                            await sender.Send(msgData);
                        }
                        catch (Exception e)
                        {
                            Debug.Log("---->" + e);
                        }

                    }
                }
            }
        }

        /// <summary>
        /// 初始化anchor
        /// </summary>
        /// <param name="msgSetAnchor"></param>
        private void SetAnchors(LiveMessageSetAnchor msgSetAnchor)
        {
            //Debug.Log("Init Anchor!");
            anchorController.ClearAllAnchorInfo(true);

            anchorController.serverHost = msgSetAnchor.anchorData.serverHost;
            anchorController.serverPort = msgSetAnchor.anchorData.serverPort;
            anchorController.appId = msgSetAnchor.anchorData.appId;
            anchorController.roomId = msgSetAnchor.anchorData.roomId;

            this.useUDP = msgSetAnchor.anchorData.useUDP;
            this.udpPort = msgSetAnchor.anchorData.serverPortUDP;


            // 开始记录日志 
            if (currentLogName != null)
            {
                Debug.Log("Has Old Log! " + currentLogName);
                logManager.StopLog(currentLogName);
            }
            currentLogName = "SendSync_" + msgSetAnchor.anchorData.logIndex;

            for (int i = 0; i < msgSetAnchor.anchorData.anchorNameList.Count; i++)
            {
                string anchorName = msgSetAnchor.anchorData.anchorNameList[i];
                Vector3 pos = msgSetAnchor.anchorData.anchorPosition[i].ToVector3();
                Vector3 forward = msgSetAnchor.anchorData.anchorForward[i].ToVector3();

                // 创建新anchor 
                GameObject obj = new GameObject(anchorName);
                obj.transform.position = pos;
                if (msgSetAnchor.anchorData.sendRotation)
                    obj.transform.eulerAngles = forward;
                else
                    obj.transform.forward = forward;

                //Debug.Log("Add Anchor[" + anchorName + "] at " + pos + " | " + forward);

                anchorController.AddAnchorObject(anchorName, obj);
            }

            anchorController.ShowAllMark(false);

            if (useUDP)
            {
                if (sender != null)
                    sender.Dispose();

                sender = new UdpSender(udpPort, UdpMode.Unicast, liveIp, null);
                sender.Init();

            }

            // 设置完毕之后，回传结果给PC 
            SendSetAnchorResult();
        }

        private void SendSetAnchorResult()
        {
            if (syncClient != null)
            {
                if (syncClient.Running)
                {
                    LiveMessageSetAnchorFinish msg = new LiveMessageSetAnchorFinish();
                    msg.version = version;

                    //Debug.Log("Send Anchor sync info!");
                    syncClient.SendMessage(msg.Serialize());
                }
            }

        }


        /// <summary>
        /// 存储anchor
        /// </summary>
        /// <param name="msgSetAnchor"></param>
        private void SaveAnchors(LiveMessageSaveAnchor msgSetAnchor)
        {
            Debug.Log("Save Anchor! rotate=" + msgSetAnchor.anchorData.sendRotation);
            waitToSave = new List<AnchorObjectInfo>();
            for (int i = 0; i < msgSetAnchor.anchorData.anchorNameList.Count; i++)
            {
                string anchorName = msgSetAnchor.anchorData.anchorNameList[i];
                Vector3 pos = msgSetAnchor.anchorData.anchorPosition[i].ToVector3();
                Vector3 forward = msgSetAnchor.anchorData.anchorForward[i].ToVector3();

                // 修改原有anchor
                AnchorObjectInfo info = anchorController.GetAnchorInfo(anchorName);
                if (info != null)
                {
                    anchorController.RemoveAnchor(info);

                    info.rootTrans.position = pos;
                    if (msgSetAnchor.anchorData.sendRotation)
                        info.rootTrans.eulerAngles = forward;
                    else
                        info.rootTrans.forward = forward;
                    //info.mark.followRoot = true;
                    //info.FollowRootObject();

                    anchorController.CreateAnchor(info);
                }
                waitToSave.Add(info);
                anchorController.SaveAllSceneRootAnchor();
            }

            anchorController.ShowAllMark(false);

            // 检查是否存储完毕 
            StartCoroutine(CheckSave());
        }


        private float StartSaveTime = 0;

        private IEnumerator CheckSave()
        {
            bool succ = true;
            string error = null;
            StartSaveTime = Time.realtimeSinceStartup;
            if (waitToSave != null)
            {
                bool finish = false;

                while (!finish)
                {
                    if (Time.realtimeSinceStartup - StartSaveTime > 10)
                    {
                        // 10秒超时 
                        succ = false;
                        error = "Save Anchor over time, error quit";
                        break;
                    }

                    finish = true;
                    for (int i = 0; i < waitToSave.Count; i++)
                    {
                        if (waitToSave[i].needSave)
                        {
                            finish = false;
                            break;
                        }
                    }

                    yield return null;
                }
            }

            if (succ && waitToSave != null)
            {
                for (int i = 0; i < waitToSave.Count; i++)
                {
                    if (!waitToSave[i].saveSucc)
                    {
                        succ = false;
                        error = "Save Anchor Failed!";
                        break;
                    }

                }
            }

            waitToSave = null;

            // 回传结果给PC 
            SendSaveAnchorResult(succ, error);
        }

        private List<AnchorObjectInfo> waitToSave = null;

        private void SendSaveAnchorResult(bool succ, string error)
        {
            if (syncClient != null)
            {
                if (syncClient.Running)
                {
                    LiveMessageSaveAnchorFinish msg = new LiveMessageSaveAnchorFinish();
                    msg.result.success = succ;
                    msg.result.errorString = error;

                    //Debug.Log("Send Anchor sync info!");
                    syncClient.SendMessage(msg.Serialize());
                }
            }
            
        }


        private void DownloadAnchor()
        {
            anchorController.DownloadAnchor(DownloadAnchorFinish);
        }

        private void DownloadAnchorFinish(bool succ, string error)
        {
            if (syncClient != null)
            {
                if (syncClient.Running)
                {
                    LiveMessageDownloadFinish msg = new LiveMessageDownloadFinish();
                    msg.result.success = succ;
                    msg.result.errorString = error;

                    //Debug.Log("send Download Finish!");

                    syncClient.SendMessage(msg.Serialize());

                    if (succ)
                    {
                        // 如果下载成功，还需要重新同步anchor位置 
                        //SendAnchorSynchonize();
                    }
                }
            }
        }

        public void SendSpatialMapping()
        {
            List<MeshFilter> meshes = SpatialMappingManager.Instance.GetMeshFilters();
            byte[] meshData = SimpleMeshSerializer.Serialize(meshes);

            //SpatialMappingManager.Instance.DrawVisualMeshes = true;

            LiveMessageResponseSpatialMapping msg = new LiveMessageResponseSpatialMapping();
            msg.mapData = meshData;

            Debug.Log("Spatial Mapping bytes len=" + meshData.Length);
            Debug.Log("Send Spatial Mapping!");

            syncClient.SendMessage(msg.Serialize());

        }
#else
        // 只实现空方法 
        protected override void _Init()
        {
        }

        // 只实现空方法 
        protected override void _TurnOn()
        {
        }

        // 只实现空方法 
        protected override void _TurnOff()
        {
        }
#endif
    }
}