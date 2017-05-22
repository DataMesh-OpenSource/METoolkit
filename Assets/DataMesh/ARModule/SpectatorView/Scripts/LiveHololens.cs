using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using MEHoloClient.Entities;
using MEHoloClient.Sync;
using MEHoloClient.Interface.Sync;
using MEHoloClient.Utils;
using DataMesh.AR.Anchor;
using System;

namespace DataMesh.AR.SpectatorView
{
    public class LiveHololens : DataMesh.AR.MEHoloModuleSingleton<LiveHololens>
    {
        /// <summary>
        /// 识别版本号，只用于和Live通讯时使用 
        /// 连接上Live后会发这个版本号过去，Live会和本地的版本号对比 
        /// </summary>
        public static short version = 2;

        private SyncClient syncClient;

        /// <summary>
        /// PC端的IP
        /// </summary>
        public string bevIp;

        /// <summary>
        /// PC端的端口
        /// </summary>
        public int bevPort;

        private string socketUrl;

        private Transform mainCameraTransform;
        private SceneAnchorController anchorController;

        private bool synchronizing = false;

        /// <summary>
        /// 初始化
        /// </summary>
        protected override void _Init()
        {
            anchorController = SceneAnchorController.Instance;
            mainCameraTransform = Camera.main.transform;
        }

        /// <summary>
        /// 开启服务
        /// </summary>
        protected override void _TurnOn()
        {
            socketUrl = "ws://" + bevIp + ":" + bevPort + LiveConstant.BevServerHandlerName;

            syncClient = new SyncClient(socketUrl, true, 100);
            syncClient.StartClient();

            Debug.Log("start connect: " + socketUrl);

        }

        protected override void _TurnOff()
        {
            
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
                        }
                    }

                    // 如果需要同步，则发送摄影机位置 
                    // 不需要发送anchor位置，因为anchor位置只在修改时才需要发送 
                    if (synchronizing)
                    {
                        LiveMessageSynchronizeAll msg = new LiveMessageSynchronizeAll();
                        msg.position = mainCameraTransform.transform.position;
                        msg.rotation = mainCameraTransform.transform.eulerAngles;

                        msg.anchorCount = anchorController.anchorObjectList.Count;
                        msg.anchorPositionList = new Vector3[msg.anchorCount];
                        msg.anchorRotationList = new Vector3[msg.anchorCount];
                        msg.anchorIsLocated = new bool[msg.anchorCount];
                        for (int i = 0; i < anchorController.anchorObjectList.Count; i++)
                        {
                            AnchorObjectInfo info = anchorController.anchorObjectList[i];
                            msg.anchorPositionList[i] = info.rootObject.transform.position;
                            msg.anchorRotationList[i] = info.rootObject.transform.eulerAngles;
                            msg.anchorIsLocated[i] = info.anchor.isLocated;
                        }


                        byte[] msgData = msg.Serialize();

                        /*
                        string str = "send All sync! " + msg.position + "," + msg.rotation + "===";
                        for (int i = 0;i < msg.anchorPositionList.Length;i ++)
                        {
                            str += msg.anchorPositionList[i].ToString() + ",";
                            str += msg.anchorRotationList[i].ToString() + "||";
                        }
                        Debug.Log(str);

                        str = "";
                        for (int i = 0;i < msgData.Length;i ++)
                        {
                            str += msgData[i].ToString() + ",";
                        }
                        Debug.Log(str);
                        */
                        syncClient.SendMessage(msgData);
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
            if (msgSetAnchor.anchorData.isInit)
            {
                //Debug.Log("Init Anchor!");
                anchorController.ClearAllAnchorInfo(true);

                anchorController.serverHost = msgSetAnchor.anchorData.serverHost;
                anchorController.serverPort = msgSetAnchor.anchorData.serverPort;
                anchorController.appId = msgSetAnchor.anchorData.appId;
                anchorController.roomId = msgSetAnchor.anchorData.roomId;
            }


            for (int i = 0; i < msgSetAnchor.anchorData.anchorNameList.Count; i++)
            {
                string anchorName = msgSetAnchor.anchorData.anchorNameList[i];
                Vector3 pos = msgSetAnchor.anchorData.anchorPosition[i].ToVector3();
                Vector3 forward = msgSetAnchor.anchorData.anchorForward[i].ToVector3();

                if (msgSetAnchor.anchorData.isInit)
                {
                    // 创建新anchor 
                    GameObject obj = new GameObject(anchorName);
                    obj.transform.position = pos;
                    obj.transform.forward = forward;

                    //Debug.Log("Add Anchor[" + anchorName + "] at " + pos + " | " + forward);

                    anchorController.AddAnchorObject(anchorName, obj);
                }
                else
                {
                    // 修改原有anchor
                    AnchorObjectInfo info = anchorController.GetAnchorInfo(anchorName);
                    if (info != null)
                    {
                        anchorController.RemoveAnchor(info);

                        info.rootObject.transform.position = pos;
                        info.rootObject.transform.forward = forward;
                        //info.mark.followRoot = true;
                        //info.FollowRootObject();

                        anchorController.CreateAnchor(info);
                    }
                    anchorController.SaveAllSceneRootAnchor();
                }
            }

            anchorController.ShowAllMark(false);

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

                    info.rootObject.transform.position = pos;
                    info.rootObject.transform.forward = forward;
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

            if (succ)
            {
                for (int i = 0; i < waitToSave.Count; i++)
                {
                    if (!waitToSave[i].saveSucc)
                    {
                        succ = false;
                        error = "Save Anchor Failed!";
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
    }
}