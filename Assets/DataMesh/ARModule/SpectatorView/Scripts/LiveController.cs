using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using MEHoloClient.Sync;
using MEHoloClient.Utils;
using MEHoloClient.UDP;
using DataMesh.AR;
using DataMesh.AR.Anchor;
using DataMesh.AR.Interactive;
using DataMesh.AR.Utility;
using DataMesh.AR.Log;
using System.Runtime.InteropServices;
using System;
//using System.Diagnostics;

namespace DataMesh.AR.SpectatorView
{
    public enum LoadTransType
    {
        Camera,
        Anchor,
        CameraAndAnchor,
    }

    public class LiveController : MEHoloModuleSingleton<LiveController>
    {

        /// <summary>
        /// 监听的端口，用于与配套的Hololens上的SpectatorView应用1沟通 
        /// </summary>
        public int listenPort = 8099;

        /// <summary>
        /// 是否开启UDP传输
        /// </summary>
        public bool useUDP = true;

        /// <summary>
        /// 如果开启UDP传输，监听的UDP端口
        /// </summary>
        public int listenPortUDP = 8098;

        /// <summary>
        /// 视频和图片输出文件夹路径 
        /// </summary>
        public System.String outputPath;

        public System.Action cbStartMoveAnchor;
        public System.Action cbEndMoveAnchor;
        public GameObject holoCameraObject;


#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        #region DLLImports
        [DllImport("UnityCompositorInterface")]
        private static extern void StopFrameProvider();

        [DllImport("UnityCompositorInterface")]
        private static extern int GetFrameWidth();

        [DllImport("UnityCompositorInterface")]
        private static extern int GetFrameHeight();

        [DllImport("UnityCompositorInterface")]
        private static extern void SetFrameOffset(float frameOffset);

        [DllImport("UnityCompositorInterface")]
        private static extern float GetFrameOffset();

        [DllImport("UnityCompositorInterface")]
        private static extern void SetAlpha(float alpha);

        [DllImport("UnityCompositorInterface")]
        private static extern float GetAlpha();

        [DllImport("UnityCompositorInterface")]
        private static extern void TakePicture();
        
        [DllImport("UnityCompositorInterface", CharSet = CharSet.Unicode)]
        private static extern void TakePictureMe(System.String outputPath, System.String photoName);

        [DllImport("UnityCompositorInterface")]
        private static extern void TakeCanonPicture();

        [DllImport("UnityCompositorInterface")]
        private static extern bool CaptureHiResHolograms();

        [DllImport("UnityCompositorInterface")]
        private static extern void StartRecording();
        
        [DllImport("UnityCompositorInterface", CharSet = CharSet.Unicode)]
        private static extern bool StartRecordingMe(System.String outputPath, System.String videoName);

        [DllImport("UnityCompositorInterface")]
        private static extern void StopRecording();

        [DllImport("UnityCompositorInterface")]
        private static extern int StopRecordingMe();

        [DllImport("UnityCompositorInterface")]
        private static extern bool IsRecording();

        [DllImport("UnityCompositorInterface")]
        private static extern void InitializeFrameProvider();

        [DllImport("UnityCompositorInterface")]
        private static extern void PollForColorFrames();

        [DllImport("UnityCompositorInterface")]
        private static extern void Reset();

        [DllImport("UnityCompositorInterface")]
        private static extern bool QueueingHoloFrames();
        #endregion

        [HideInInspector]
        public LiveWDPController wdpController;


        /// <summary>
        /// UI所用到的Prefab
        /// </summary>
        public GameObject uiPrefab;

        /// <summary>
        /// Live拍摄所使用的摄像机的Prefab
        /// </summary>
        //public GameObject holoCameraPrefab;

        [HideInInspector]
        public LiveControllerUI liveUI;

        private string holoServerHost;
        private int holoServerPort;

        private IRecordNamerGenerator nameGenerator = new RecordNameGeneratorDefault();
        private string currentName;

        private List<ILiveListener> listenerList = new List<ILiveListener>();

        private Camera mainCamera;
        private Transform mainCameraTransform;
        private Transform holoCameraTransform;

        [HideInInspector]
        public bool hololensConnected = false;
        [HideInInspector]
        public bool hololensStartSynchronize = false;
        [HideInInspector]
        public bool hololensHasInit = false;
        [HideInInspector]
        public bool waiting = false;
        [HideInInspector]
        public bool anchorLocated = true;

        private float waitingStringShowTime = 0;
        private const float WAIT_STRING_DISAPPEAR_INTERVAL = 5f;
        private string _waitingString = null;
        public string waitingString
        {
            get { return _waitingString; }
            set
            {
                _waitingString = value;
                waitingStringShowTime = Time.realtimeSinceStartup;
            }
        }

        private SyncServer server;
        private LiveServerHandler handler;
        private UdpListener udpListener;
        public HolographicCameraManager holoCamera;
        [HideInInspector]
        public CalibrationManager calibrationManager;

        private SceneAnchorController anchorController;
        private MultiInputManager inputManager;

        private float lastSyncTime = 0;
        private float syncTimeoutInterval = 0.6f;
        private int lastSyncIndex = int.MinValue;

        private LinkedList<LiveMessageSynchronizeAll> syncMsgList = new LinkedList<LiveMessageSynchronizeAll>();

        private int oldCameraMask;

        [HideInInspector]
        public short spectatorViewVersion = -1;

        //public float syncDelayTime = 0.2f;

        private float saveAnchorInterval = 1f;
        private float lastSaveAnchorTime = 0;
        private string saveAnchorFileName = "AnchorAndCamera.sav";
        [HideInInspector]
        public bool saveAnchorDirty = false;

        private LogManager logManager;
        private int currentLogIndex = 0;
        private string currentLogName = null;

        private float lastSaveParamTime;
        private const float saveParamInterval = 1.0f;

        protected override void Awake()
        {
            base.Awake();

            gameObject.SetActive(false);
        }

        protected override void _Init()
        {
            mainCamera = Camera.main;

            mainCameraTransform = Camera.main.transform;
            holoCameraTransform = holoCamera.transform;
            Common.FFT fft = holoCamera.GetComponent<Common.FFT>();
            if (fft == null)
            {
                fft = holoCamera.gameObject.AddComponent<Common.FFT>();
            }
            anchorController = SceneAnchorController.Instance;
            inputManager = MultiInputManager.Instance;
            wdpController = LiveWDPController.Instance;

            if (MEHoloConstant.IsLiveActive)
            {
                // 如果Live激活，则自动启动 
                AutoTurnOn = MEHoloConstant.IsLiveActive;
            }
            else
            {
                holoCamera.gameObject.SetActive(false);
            }

        }

        protected override void _TurnOn()
        {
            // 加载Live配置文件
            AppConfig config = AppConfig.Instance;
            config.LoadConfig(MEHoloConstant.LiveConfigFile);
            listenPort = int.Parse(AppConfig.Instance.GetConfigByFileName(MEHoloConstant.LiveConfigFile, "Live_Port", "8099"));
            listenPortUDP = int.Parse(AppConfig.Instance.GetConfigByFileName(MEHoloConstant.LiveConfigFile, "Live_Port_UDP", "8098"));
            useUDP = bool.Parse(AppConfig.Instance.GetConfigByFileName(MEHoloConstant.LiveConfigFile, "Use_UDP", "TRUE"));
            outputPath = AppConfig.Instance.GetConfigByFileName(MEHoloConstant.LiveConfigFile,"Out_Put_Path","C:\\HologramCapture");

            config.LoadConfig(MEHoloConstant.CalibrationConfigFile);
            holoServerHost = config.GetConfigByFileName(MEHoloConstant.NetworkConfigFile, "Server_Host", "127.0.0.1");
            int.TryParse(config.GetConfigByFileName(MEHoloConstant.NetworkConfigFile, "Server_Port", "8848"), out holoServerPort);

            outputPath = config.GetConfigByFileName(MEHoloConstant.LiveConfigFile, "Out_Put_Path", "./");
            if (!outputPath.EndsWith("/") && !outputPath.EndsWith("\\"))
            {
                outputPath += "/";
            }

            // 读取参数设置 
            if (!LiveParam.LoadParam())
            {
                LiveParam.SoundVolume = AudioListener.volume;
                LiveParam.SaveParam();
            }



            // 读取本地位置存储文件 
            LoadTransformByFile(LoadTransType.CameraAndAnchor);

            // 初始化并启动UI 
            GameObject uiObj = PrefabUtils.CreateGameObjectToParent(this.gameObject, uiPrefab);
            liveUI = uiObj.GetComponent<LiveControllerUI>();
            liveUI.Init(this, (float)GetFrameWidth() / (float)GetFrameHeight());
            liveUI.TurnOn();

            // 初始化Windows device protocal
            wdpController.Init(this, liveUI);
                
            calibrationManager = holoCamera.gameObject.AddComponent<CalibrationManager>();
            calibrationManager.Init();

            // 刷新显示 
          //  liveUI.mainPanel.RefreshFOVInput();

            // 延迟启动全息摄影机 
            StartHoloCamera();

            // Log
            logManager = LogManager.Instance;

            // 关闭主摄影机的显示 
            oldCameraMask = mainCamera.cullingMask;
            //mainCamera.cullingMask = 0;

            // 设置一下frameoffset
            SetFrameOffset(0f);

            // 停止input操作
            /*
            if (inputManager != null)
            {
                inputManager.StopCapture();
            }
            */

            // 绑定Anchor回调 
            anchorController.AddCallbackTurnOn(OnAnchorTurnOn);
            anchorController.AddCallbackTurnOff(OnAnchorTurnOff);
            StartCoroutine(WaitForSetAlpha());
            StartCoroutine(WaitForSetFilter());

        }
        private IEnumerator WaitForSetAlpha()
        {
            yield return new WaitForSeconds(2.1f);

            while (Time.time<10f)
            {
                yield return new WaitForSeconds(0.1f);
                if (GetAlpha() != LiveParam.Alpha)
                {
                    SetAlpha(LiveParam.Alpha);

                }
                else {
                    Debug.Log(Time.time);
                    break;
                }
            }


        }

        private IEnumerator WaitForSetFilter()
        {
            yield return new WaitForSeconds(2.1f);
            ShaderManager mana = GameObject.FindObjectOfType<ShaderManager>();
            while (Time.time < 10)
            {
                yield return new WaitForSeconds(0.1f);

                if (LiveParam.Filter != mana.alphaBlendPreviewMat.GetFloat("_Filter"))
                {
                    LiveParam.Filter = LiveParam.Filter;
                }
                else
                {
                    Debug.Log(Time.time);
                    break;
                }
            }
        }


        private void StratLog()
        {
            currentLogIndex = UnityEngine.Random.Range(0, 99999);
            currentLogName = "ReceiveSync_" + currentLogIndex;
        }

        private void StopLog()
        {
            if (currentLogName != null)
            {
                logManager.StopLog(currentLogName);
                currentLogName = null;
            }
        }
        
        private void StartHoloCamera()
        {
            CameraFlyController fly = mainCamera.GetComponent<CameraFlyController>();
            if (fly != null)
            {
                fly.followedObjects.Add(holoCamera.transform);
            }

            // 启动 
            holoCamera.Init(holoServerHost, holoServerPort);

            // 调整摄影机
            SetupCameraValues(mainCamera);
            SetupCameraValues(holoCamera.GetComponent<Camera>());

            // 重设Anchor的FOV
            anchorController.InitFOV();

            holoCamera.gameObject.SetActive(true);

            holoCameraTransform = holoCamera.transform;

        }

        private void SetupCameraValues(Camera camera)
        {
            if (camera == null)
            {
                Debug.LogError("Camera is null.");
                return;
            }

            camera.nearClipPlane = 0.01f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0, 0, 0, 0);

            camera.fieldOfView = calibrationManager.data.DSLR_fov.y;
            camera.enabled = true;
        }

        public void OnFOVChange()
        {
            SetupCameraValues(mainCamera);
            SetupCameraValues(holoCamera.GetComponent<Camera>());

            // 重设Anchor的FOV
            anchorController.InitFOV();

        }

        protected override void _TurnOff()
        {
            anchorController.RemoveCallbackTurnOn(OnAnchorTurnOn);
            anchorController.RemoveCallbackTurnOn(OnAnchorTurnOff);
        }

        public void StartHololensServer()
        {
            server = new SyncServer(listenPort);
            server.AddHandler<LiveServerHandler>(MEHoloConstant.LiveServerHandlerName);

            server.StartServer();
            LiveServerHandler.cbOpen = OnServerOpen;
            LiveServerHandler.cbClose = OnServerClose;
            Debug.Log("Begin listen to " + listenPort);

            if (useUDP)
            {
                udpListener = new UdpListener(listenPortUDP, UdpMode.Unicast, null, 2);
                udpListener.StartListen();
                Debug.Log("Begin listen UDP to " + listenPortUDP);
            }

            wdpController.TurnOn();
        }


        public void StopHololesServer()
        {
            if (server != null)
                server.Stop();
            server = null;

            if (udpListener != null)
                udpListener.Dispose();
            udpListener = null;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            StopHololesServer();
        }

        /// <summary>
        /// 注册一个视频的命名器
        /// </summary>
        /// <param name="generator"></param>
        public void SetRecordNameGenerator(IRecordNamerGenerator generator)
        {
            this.nameGenerator = generator;
        }

        /// <summary>
        /// 添加一个bev监听器 
        /// </summary>
        /// <param name="listener"></param>
        public void AddLiveListener(ILiveListener listener)
        {
            listenerList.Add(listener);
        }

        private void OnServerOpen(LiveServerHandler liveHandler)
        {
            hololensConnected = true;
            handler = liveHandler;
        }

        private void OnServerClose(LiveServerHandler liveHandler)
        {
            if (liveHandler != handler)
                return;

            hololensConnected = false;
            hololensHasInit = false;
            handler = null;

            StopLog();
        }

        // Update is called once per frame
        void Update()
        {
            CheckPerformence();

            CheckSyncTransform();

            CheckAnchorSave();

            CheckUI();

            CheckParam();

            CheckMessage();
        }

        private void CheckPerformence()
        {
            /*
            System.Diagnostics.PerformanceCounter cpuPerformanceCounter = new System.Diagnostics.PerformanceCounter();
            cpuPerformanceCounter.CategoryName = "Processor";
            cpuPerformanceCounter.CounterName = "% Processor Time";
            cpuPerformanceCounter.InstanceName = "_Total";
            for (int i = 0;i < 4;i ++)
                Debug.Log("Cpu[" + i + "]--->" + cpuPerformanceCounter.NextValue());
            */
        }

        private void CheckParam()
        {
            float realTime = Time.realtimeSinceStartup;
            if (LiveParam.IsDirty && realTime - lastSaveParamTime > saveParamInterval)
            {
                LiveParam.SaveParam();
                lastSaveParamTime = realTime;
            }
        }

        void OnApplicationQuit()
        {
            if (logManager != null)
            {
                logManager.Clear();   
            }
        }

        private void CheckUI()
        {
            float realTime = Time.realtimeSinceStartup;

            if (realTime > waitingStringShowTime + WAIT_STRING_DISAPPEAR_INTERVAL)
            {
                _waitingString = null;
            }
        }

        private void DealSyncMessage(LiveMessageSynchronizeAll syncMsg)
        {
            if (!hololensHasInit)
            {
                // 这时的消息很可能是不对的！不能直接设置 

                // 告诉hololens不要发了 
                SetHololensSynchronize(false);

                return;
            }

            float realTime = Time.realtimeSinceStartup;
            syncMsg.receiveTime = realTime;

            // 记录Log
            logManager.Log(currentLogName, syncMsg.FormatLogString());

            // 过滤过期消息 
            if (syncMsg.seq > lastSyncIndex)
            {
                lastSyncIndex = syncMsg.seq;

                // 加入待处理队列 
                syncMsgList.AddLast(syncMsg);

                // 重设一下状态 
                lastSyncTime = Time.time;
                hololensStartSynchronize = true;


            }
        }




        private void CheckMessage()
        {
            float realTime = Time.realtimeSinceStartup;

            if (server != null)
            {
                //Debug.Log("Server Listening? " + server.IsListening);
                if (hololensConnected && !hololensHasInit && !waiting)
                {
                    // 如果连上了，就开始初始化 
                    SetHololensSynchronize(false);
                    SetAnchorToHololens(true);

                    // 这时需要把传输序列号清空，避免新接入的Hololens的同步消息被忽略 
                    lastSyncIndex = int.MinValue;
                }

                while (SyncServer.SyncQueue.GetCount() > 0)
                {
                    byte[] messageBytes = SyncServer.SyncQueue.Dequeue();
                    //Debug.Log("receive [" + messageBytes.Length + "] type=" + messageBytes[0]);

                    // 处理消息 
                    LiveMessage msg = LiveMessageManager.ParseMessage(messageBytes);

                    switch (msg.type)
                    {
                        case LiveMessageConstant.BEV_MESSAGE_TYPE_SYNCHRONIZE_ALL:
                            LiveMessageSynchronizeAll syncMsg = msg as LiveMessageSynchronizeAll;

                            DealSyncMessage(syncMsg);

                            break;

                        case LiveMessageConstant.BEV_MESSAGE_TYPE_DOWNLOAD_ANCHOR_FINISH:
                            LiveMessageDownloadFinish downloadFinishMsg = msg as LiveMessageDownloadFinish;
                            waiting = false;
                            if (downloadFinishMsg.result.success)
                            {
                                waitingString = "Download Anchors Success! ";
                            }
                            else
                            {
                                waitingString = downloadFinishMsg.result.errorString;
                            }
                            break;

                        case LiveMessageConstant.BEV_MESSAGE_TYPE_SET_ANCHOR_FINISH:
                            LiveMessageSetAnchorFinish anchorFinishMsg = msg as LiveMessageSetAnchorFinish;
                            spectatorViewVersion = anchorFinishMsg.version;
                            if (spectatorViewVersion < LiveHololens.version)
                            {
                                // SpectatorView版本太低！ 
                                liveUI.ShowInfoDialog("Spectator View's verion is too old!");
                                hololensConnected = false;
                                StopHololesServer();
                            }

                            waiting = false;
                            waitingString = "init OK!";
                            hololensHasInit = true;

                            // 初始化完成，就先开启位置传输 
                            SetHololensSynchronize(true);
                            break;

                        case LiveMessageConstant.BEV_MESSAGE_TYPE_SAVE_ANCHOR_FINISH:
                            LiveMessageSaveAnchorFinish savehMsg = msg as LiveMessageSaveAnchorFinish;

                            waiting = false;
                            if (savehMsg.result.success)
                            {
                                waitingString = "Save Anchor OK!";

                                // 初始化完成，就先开启位置传输 
                                // 此功能先取消，因为产生了许多误解
                                //SetHololensSynchronize(true);
                            }
                            else
                            {
                                waitingString = savehMsg.result.errorString;

                            }
                            break;

                        case LiveMessageConstant.BEV_MESSAGE_TYPE_RESPONSE_SPATIAL_MAPPING:
                            LiveMessageResponseSpatialMapping spatialMsg = msg as LiveMessageResponseSpatialMapping;
                            SetSpatialMapping(spatialMsg);
                            break;
                    }

                }

                // 判断是否已经不传输了 
                if (lastSyncTime >= 0)
                {
                    if (Time.time - lastSyncTime > syncTimeoutInterval)
                    {
                        hololensStartSynchronize = false;
                    }
                }
            }

            if (useUDP && udpListener != null)
            {
                while (udpListener.UdpReceiveQueue.GetCount() > 0)
                {
                    byte[] messageBytes = udpListener.UdpReceiveQueue.Dequeue();
                    //Debug.Log(messageBytes.Length + "---" + messageBytes[0]);
                    LiveMessage msg = LiveMessageManager.ParseMessage(messageBytes);

                    if (msg == null)
                        continue;

                    switch (msg.type)
                    {
                        case LiveMessageConstant.BEV_MESSAGE_TYPE_SYNCHRONIZE_ALL:
                            LiveMessageSynchronizeAll syncMsg = msg as LiveMessageSynchronizeAll;
                            //Debug.Log("-->"+syncMsg.seq);
                            DealSyncMessage(syncMsg);

                            break;
                    }
                }
            }

        }

        private Vector3 syncCameraPos = Vector3.zero;
        private Vector4 syncCameraRotTemp = Vector4.zero;
        private Quaternion syncCameraRot = Quaternion.identity;
        private Quaternion syncCameraRotFirst = Quaternion.identity;
        private Vector3[] syncAnchorPos;
        private Vector4[] syncAnchorRotTemp;
        private Quaternion[] syncAnchorRot;
        private Quaternion[] syncAnchorRotFirst;

        private void InitSyncAnchorData<T>(ref T[] list, int anchorCount)
        {
            if (list == null || list.Length != anchorCount)
            {
                list = new T[anchorCount];
            }
            for (int i = 0;i < list.Length;i ++)
            {
                list[i] = default(T);
            }
        }

        private void CountOneSyncMessage(ref int count, LiveMessageSynchronizeAll syncMsg)
        {
            // 求取平均值 
            count++;

            syncCameraPos += syncMsg.position;
            if (count <= 1)
            {
                syncCameraRot = syncMsg.rotation;
                syncCameraRotFirst = syncCameraRot;
            }
            else
            {
                syncCameraRot = MathUtility.AverageQuaternion(ref syncCameraRotTemp, syncMsg.rotation, syncCameraRotFirst, count);
            }

            for (int i = 0; i < anchorController.anchorObjectList.Count; i++)
            {
                if (i >= syncMsg.anchorCount)
                    continue;

                syncAnchorPos[i] += syncMsg.anchorPositionList[i];

                if (count <= 1)
                {
                    syncAnchorRot[i] = syncMsg.anchorRotationList[i];
                    syncAnchorRotFirst[i] = syncAnchorRot[i];
                }
                else
                {
                    Vector4 temp = syncAnchorRotTemp[i];
                    syncAnchorRot[i] = MathUtility.AverageQuaternion(ref temp, syncMsg.anchorRotationList[i], syncAnchorRotFirst[i], count);
                    syncAnchorRotTemp[i] = temp;
                }

                // 只要还有消息表明没能定位，就表示没定位 
                if (!syncMsg.anchorIsLocated[i])
                    anchorLocated = false;
            }

        }

        private void CheckSyncTransform()
        {
            if (syncMsgList.Count == 0)
                return;

            float curTime = Time.realtimeSinceStartup - LiveParam.SyncDelayTime;
            float timeBefore = curTime + LiveParam.AntiShakeBeforeTime;
            float timeAfter = curTime + LiveParam.AntiShakeAfterTime;

            LinkedListNode<LiveMessageSynchronizeAll> node = syncMsgList.First;

            syncCameraPos = Vector3.zero;
            syncCameraRot = Quaternion.identity;
            syncCameraRotTemp = Vector4.zero;

            int anchorCount = anchorController.anchorObjectList.Count;

            InitSyncAnchorData(ref syncAnchorPos, anchorCount);
            InitSyncAnchorData(ref syncAnchorRot, anchorCount);
            InitSyncAnchorData(ref syncAnchorRotFirst, anchorCount);
            InitSyncAnchorData(ref syncAnchorRotTemp, anchorCount);

            int count = 0;
            anchorLocated = true;
            LiveMessageSynchronizeAll lastMsg = null;
            while (node != null)
            {
                LiveMessageSynchronizeAll syncMsg = node.Value;
                LinkedListNode<LiveMessageSynchronizeAll> removeNode = null;
                if (syncMsg.receiveTime <= timeBefore)
                {
                    lastMsg = syncMsg;
                    removeNode = node;
                }
                else if (syncMsg.receiveTime <= timeAfter)
                {
                    CountOneSyncMessage(ref count, syncMsg);
                }

                node = node.Next;
                if (removeNode != null)
                    syncMsgList.Remove(removeNode);


            }

            if (count == 0 && lastMsg != null)
            {
                // 如果时间窗口内没有任何对象，并且有删除，则取之前删除的最后一个对象 
                CountOneSyncMessage(ref count, lastMsg);
            }

            if (count > 0)
            {
                mainCameraTransform.position = syncCameraPos / (float)count + calibrationManager.data.Translation;
                mainCameraTransform.rotation = syncCameraRot;
                mainCameraTransform.eulerAngles += calibrationManager.data.Rotation.eulerAngles;

                holoCameraTransform.position = mainCameraTransform.position;
                holoCameraTransform.rotation = mainCameraTransform.rotation;

                for (int i = 0; i < anchorController.anchorObjectList.Count; i++)
                {
                    AnchorObjectInfo info = anchorController.anchorObjectList[i];
                    info.rootTrans.position = syncAnchorPos[i] / (float)count;
                    info.rootTrans.rotation = syncAnchorRot[i];

                    //Debug.Log(" --->Set Anchor [" + info.anchorName + "] to " + syncMsg.anchorPositionList[i] + " | " + syncMsg.anchorRotationList[i]);
                }

            }

            saveAnchorDirty = true;

        }

        public void SetHololensSynchronize(bool b)
        {

            if (server == null)
                return;

            if (b)
            {
                LiveMessageStart msg = new LiveMessageStart();
                if (handler != null)
                    handler.SendMessage(msg.Serialize(), null);
            }
            else
            {
                LiveMessageStop msg = new LiveMessageStop();
                if (handler != null)
                    handler.SendMessage(msg.Serialize(), null);
            }

            if (b)
            {
                // 屏蔽掉主摄像机
                //mainCamera.cullingMask = 0;
            }
            else
            {
                mainCamera.cullingMask = oldCameraMask;
            }
        }

        /// <summary>
        /// 将anchor信息初始化到Hololens端 
        /// </summary>
        public void SetAnchorToHololens(bool isInit)
        {

            if (!hololensConnected)
                return;

            StratLog();

            LiveMessageSetAnchor msg = new LiveMessageSetAnchor();
            msg.anchorData = new LiveMessageSetAnchor.LiveMessageSetAnchorData();
            msg.anchorData.serverHost = anchorController.serverHost;
            msg.anchorData.serverPort = anchorController.serverPort;
            msg.anchorData.appId = anchorController.appId;
            msg.anchorData.roomId = anchorController.roomId;
            msg.anchorData.useUDP = this.useUDP;
            msg.anchorData.serverPortUDP = this.listenPortUDP;
            msg.anchorData.sendRotation = true;
            msg.anchorData.logIndex = currentLogIndex;

            for (int i = 0; i < anchorController.anchorObjectList.Count; i++)
            {
                AnchorObjectInfo info = anchorController.anchorObjectList[i];
                msg.anchorData.anchorNameList.Add(info.anchorName);
                msg.anchorData.anchorPosition.Add(info.rootTrans.position);
                msg.anchorData.anchorForward.Add(info.rootTrans.eulerAngles);
            }

            Debug.Log("Send Set Anchor msg type=" + msg.type);
            if (handler != null)
                handler.SendMessage(msg.Serialize(), null);
            waiting = true;
            waitingString = "Waiting for anchor init...";
        }

        /// <summary>
        /// 将anchor信息存储到Hololens端 
        /// </summary>
        public void SaveAnchorToHololens()
        {

            if (!hololensConnected)
                return;

            LiveMessageSaveAnchor msg = new LiveMessageSaveAnchor();
            msg.anchorData = new LiveMessageSaveAnchor.LiveMessageSetAnchorData();
            msg.anchorData.sendRotation = true;

            for (int i = 0; i < anchorController.anchorObjectList.Count; i++)
            {
                AnchorObjectInfo info = anchorController.anchorObjectList[i];
                msg.anchorData.anchorNameList.Add(info.anchorName);
                msg.anchorData.anchorPosition.Add(info.rootTrans.position);
                msg.anchorData.anchorForward.Add(info.rootTrans.eulerAngles);
            }

            Debug.Log("send message type=" + msg.type);

            if (handler != null)
                handler.SendMessage(msg.Serialize(), null);
            waiting = true;
            waitingString = "Waiting for anchor init...";
        }

        public void DownloadAnchor()
        {
            LiveMessageDownload msg = new LiveMessageDownload();
            if (handler != null)
                handler.SendMessage(msg.Serialize(), null);
            waiting = true;
            waitingString = "Waiting for download anchors...";
        }

        public void DownloadSpatial()
        {
            LiveMessageRequestSpatialMapping msg = new LiveMessageRequestSpatialMapping();
                if (handler != null)
            handler.SendMessage(msg.Serialize(), null);
            waiting = true;
            waitingString = "Download spatial mapping...";
        }


        public void SetSpatialMapping(LiveMessageResponseSpatialMapping msg)
        {
            Debug.Log("Receive Spatial Mapping! len=" + msg.mapData);

            List<Mesh> meshes = SimpleMeshSerializer.Deserialize(msg.mapData) as List<Mesh>;
            anchorController.SetSpatialMeshToObserver(meshes);
            waiting = false;
            waitingString = "Download spatial mapping OK!";
        }

        private void OnAnchorTurnOn()
        {
            Debug.Log("Live receive Anchor turn on!");
            //panel.ShowBottomBar(false);
            liveUI.isModifyAnchor = true;

            // 先停止同步 
            SetHololensSynchronize(false);

            if (cbStartMoveAnchor != null)
                cbStartMoveAnchor();

            anchorController.AddCallbackFinish(AnchorControlFinish);
        }

        private void OnAnchorTurnOff()
        {
            Debug.Log("Live receive Anchor turn off!");

            anchorController.RemoveCallbackFinish(AnchorControlFinish);

            if (cbEndMoveAnchor != null)
                cbEndMoveAnchor();

            // 如果hololens存在，发送存储 
            SaveAnchorToHololens();

            // 设置本地存储 
            saveAnchorDirty = true;

            liveUI.isModifyAnchor = false;
            //panel.ShowBottomBar(true);
        }

        public void StartMoveAnchor()
        {
            StartCoroutine(StartSceneAnchorController());
        }

        private IEnumerator StartSceneAnchorController()
        {
            yield return null;
            anchorController.TurnOn();
        }

        private void AnchorControlFinish()
        {
            anchorController.TurnOff();
        }


        public void StartCapture()
        {
            currentName = nameGenerator.GetName();
            for (int i = 0;i < listenerList.Count;i ++)
            {
                listenerList[i].OnRecordStart(currentName);
            }
            //StartRecording();
            StartRecordingMe(outputPath, currentName + ".mp4");
        }

        public void StopCapture()
        {
            StopRecording();
            //Debug.Log("[" + index + "] Record Finish！");

            for (int i = 0; i < listenerList.Count; i++)
            {
                listenerList[i].OnRecordStop(outputPath, currentName);
            }
        }

        public void StopCapture(ref string videoOutputPath , ref string videoName)
        {
            StopRecording();
            videoOutputPath = outputPath;
            videoName = currentName;
            for (int i = 0; i < listenerList.Count; i++)
            {
                listenerList[i].OnRecordStop(videoOutputPath,videoName);
            }
        }

        public void TakeSnap()
        {
            TakePictureMe(outputPath, nameGenerator.GetName() + ".png");
        }

        //添加一个可以返回outputPath和name的方法
        public void TakeSnap(ref string imageOutputPath , ref string imageName)
        {
            imageOutputPath = outputPath;
            imageName = nameGenerator.GetName() + ".png";
            TakePictureMe(imageOutputPath,imageName);
        }



        /// <summary>
        /// 从文件中读取所有Anchor和主摄像机的位置 
        /// </summary>
        public void LoadTransformByFile(LoadTransType type)
        {
            string path = GetSavePath();

            Dictionary<string, string> data = AppConfig.AnalyseConfigFile(path);
            Vector3 v;
            if (data != null)
            {
                if (type == LoadTransType.Camera || type == LoadTransType.CameraAndAnchor)
                {
                    if (data.ContainsKey("CameraPos"))
                    {
                        if (FillVectorFromString(data["CameraPos"], out v))
                        {
                            mainCameraTransform.position = v;
                            holoCameraTransform.position = mainCameraTransform.position;
                        }
                    }
                    if (data.ContainsKey("CameraRot"))
                    {
                        if (FillVectorFromString(data["CameraRot"], out v))
                        {
                            mainCameraTransform.eulerAngles = v;
                            holoCameraTransform.rotation = mainCameraTransform.rotation;
                        }
                    }
                }

                if (type == LoadTransType.Anchor || type == LoadTransType.CameraAndAnchor)
                {
                    for (int i = 0; i < anchorController.anchorObjectList.Count; i++)
                    {
                        AnchorObjectInfo info = anchorController.anchorObjectList[i];

                        if (data.ContainsKey("[" + info.anchorName + "]pos"))
                        {
                            if (FillVectorFromString(data["[" + info.anchorName + "]pos"], out v))
                            {
                                info.SetPosition(v);
                            }
                        }

                        if (data.ContainsKey("[" + info.anchorName + "]rot"))
                        {
                            if (FillVectorFromString(data["[" + info.anchorName + "]rot"], out v))
                            {
                                info.SetEular(v);
                            }
                        }

                    }
                }
            }
            else
            {
                Debug.Log("Can not fild Anchor save file.");
            }

        }


        /// <summary>
        /// 存储所有Anchor以及主摄像机的位置到文件之中 
        /// </summary>
        public void SaveTransformToFile()
        {
            string path = GetSavePath();

            Dictionary<string, string> data = new Dictionary<string, string>();

            data.Add("CameraPos", GetVectorString(mainCamera.transform.position));
            data.Add("CameraRot", GetVectorString(mainCamera.transform.eulerAngles));

            for (int i = 0; i < anchorController.anchorObjectList.Count; i++)
            {
                AnchorObjectInfo info = anchorController.anchorObjectList[i];

                data.Add("[" + info.anchorName + "]pos", GetVectorString(info.rootTrans.position));
                data.Add("[" + info.anchorName + "]rot", GetVectorString(info.rootTrans.eulerAngles));

            }

            //Debug.Log("Save Anchor file [" + path + "]");
            AppConfig.SaveConfigFile(path, data);
        }

        private string GetVectorString(Vector3 v)
        {
            return v.x + "," + v.y + "," + v.z;

        }

        private bool FillVectorFromString(string s, out Vector3 v)
        {
            v = new Vector3();

            string[] args = s.Split(',');
            if (args.Length != 3)
                return false;

            try
            {
                v.x = float.Parse(args[0]);
                v.y = float.Parse(args[1]);
                v.z = float.Parse(args[2]);
            }
            catch (System.Exception e)
            {
                return false;
            }

            return true;
        }

        private void CheckAnchorSave()
        {
            if (saveAnchorDirty)
            {
                float curTime = Time.realtimeSinceStartup;
                if (curTime - lastSaveAnchorTime > saveAnchorInterval)
                {
                    SaveTransformToFile();
                    lastSaveAnchorTime = curTime;
                    saveAnchorDirty = false;
                }
            }
        }

        public string GetSavePath()
        {
            return Application.dataPath + "/../SaveData/" + saveAnchorFileName;
        }
#else
        // 仅实现空接口 
        protected override void _Init()
        {
            holoCameraObject.gameObject.SetActive(false);
        }
        // 仅实现空接口 
        protected override void _TurnOn()
        {
            
        }
        // 仅实现空接口 
        protected override void _TurnOff()
        {
            
        }
        // 仅实现空接口 
        public void AddLiveListener(ILiveListener listener)
        {
        }
#endif
    }
}
