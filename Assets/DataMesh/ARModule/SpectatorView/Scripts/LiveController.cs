using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using MEHoloClient.Sync;
using MEHoloClient.Utils;
using DataMesh.AR;
using DataMesh.AR.Anchor;
using DataMesh.AR.Interactive;
using System.Runtime.InteropServices;
//using System.Diagnostics;

namespace DataMesh.AR.SpectatorView
{
    public class LiveController : MEHoloModuleSingleton<LiveController>
    {
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
#endif
        /// <summary>
        /// 监听的端口，用于与配套的Hololens上的SpectatorView应用1沟通 
        /// </summary>
        public int listenPort;

        /// <summary>
        /// 视频和图片输出文件夹路径 
        /// </summary>
        public System.String outputPath;

        /// <summary>
        /// 拍摄所使用的摄像机
        /// </summary>
        public Camera bevCamera;

        /// <summary>
        /// 操作面板
        /// </summary>
        public LiveControllerPanel panel;

        public System.Action cbStartMoveAnchor;
        public System.Action cbEndMoveAnchor;

        private IRecordNamerGenerator nameGenerator = new RecordNameGeneratorDefault();
        private string currentName;

        private List<ILiveListener> listenerList = new List<ILiveListener>();

        private Camera mainCamera;
        private Transform mainCameraTransform;

        [HideInInspector]
        public bool hololensConnected = false;
        [HideInInspector]
        public bool hololensStartSynchronize = false;
        [HideInInspector]
        public bool hololensHasInit = false;
        [HideInInspector]
        public bool waiting = false;
        [HideInInspector]
        public string waitingString = null;
        [HideInInspector]
        public bool anchorLocated = true;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        private SyncServer server;
        private LiveServerHandler handler;
#endif

        public HolographicCameraManager holoCamera;

        private SceneAnchorController anchorController;
        private MultiInputManager inputManager;

        private float lastSyncTime = 0;
        private float syncTimeoutInterval = 0.6f;

        private int oldCameraMask;

        [HideInInspector]
        public short spectatorViewVersion = -1;

        private float _alpha = 0.9f;
        public float alpha
        {
            get { return _alpha; }
            set
            {
                _alpha = value;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                SetAlpha(_alpha);
                /*
                if (holoCamera != null &&
                    holoCamera.shaderManager != null &&
                    holoCamera.shaderManager.colorTexture != null &&
                    holoCamera.shaderManager.renderTexture != null &&
                    holoCamera.shaderManager.holoTexture != null &&
                    holoCamera.shaderManager.alphaBlendVideoMat != null &&
                    holoCamera.shaderManager.alphaBlendOutputMat != null &&
                    holoCamera.shaderManager.alphaBlendPreviewMat != null)
                {
                    holoCamera.shaderManager.alphaBlendVideoMat.SetFloat("_Alpha", _alpha);
                    holoCamera.shaderManager.alphaBlendOutputMat.SetFloat("_Alpha", _alpha);
                    holoCamera.shaderManager.alphaBlendPreviewMat.SetFloat("_Alpha", _alpha);
                }
                */
#endif
            }
        }
        private float _frameOffset = 0.8f;
        public float frameOffset
        {
            get { return _frameOffset; }
            set
            {
                _frameOffset = value;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                SetFrameOffset(_frameOffset);
#endif
            }
        }

        protected override void Awake()
        {
            base.Awake();

            gameObject.SetActive(false);
        }

        protected override void _Init()
        {
            mainCameraTransform = Camera.main.transform;
            anchorController = SceneAnchorController.Instance;
            inputManager = MultiInputManager.Instance;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            panel.Init(this, (float)GetFrameWidth() / (float)GetFrameHeight());
#endif
        }



        protected override void _TurnOn()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

            holoCamera.Init();


            


            //gameObject.SetActive(true);

            // 启动面板 
            panel.TurnOn();

            // 关闭主摄影机的显示 
            mainCamera = Camera.main;
            oldCameraMask = mainCamera.cullingMask;
            //mainCamera.cullingMask = 0;


            // 设置参数 
            //holoCamera.frameProviderInitialized = false;
            alpha = alpha;
            frameOffset = frameOffset;

            StartCoroutine(StartHoloCamera());

            /*
            if (inputManager != null)
            {
                inputManager.StopCapture();
            }
            */
#endif
        }

        private IEnumerator StartHoloCamera()
        {
            yield return new WaitForSecondsRealtime(1);
            bevCamera.gameObject.SetActive(true);

        }

        protected override void _TurnOff()
        {

        }

        public void StartHololensServer()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            handler = new LiveServerHandler();
            server = new SyncServer(listenPort);
            server.AddHandler<LiveServerHandler>(LiveConstant.BevServerHandlerName, () => handler);
            server.StartServer();
            handler.cbOpen = OnServerOpen;
            handler.cbClose = OnServerClose;
            Debug.Log("Begin listen to " + listenPort);
#endif
        }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN

        public void StopHololesServer()
        {
            if (handler != null)
            {
                handler.cbClose = null;
                handler.cbClose = null;
            }
            if (server != null)
                server.Stop();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            StopHololesServer();
        }
#endif
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

        private void OnServerOpen()
        {
            hololensConnected = true;
        }

        private void OnServerClose()
        {
            hololensConnected = false;
        }

        // Update is called once per frame
        void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (server != null)
            {
                if (hololensConnected && !hololensHasInit && !waiting)
                {
                    // 如果连上了，就开始初始化 
                    SetHololensSynchronize(false);
                    SetAnchorToHololens(true);
                }

                while (handler.SyncQueue.GetCount() > 0)
                {
                    byte[] messageBytes = handler.SyncQueue.Dequeue();
                    //Debug.Log("receive [" + messageBytes.Length + "] type=" + messageBytes[0]);

                    /*
                    string str = "";
                    for (int i = 0; i < messageBytes.Length; i++)
                    {
                        str += messageBytes[i] + ",";
                    }
                    Debug.Log(str);
                    */


                    // 处理消息 
                    LiveMessage msg = LiveMessageManager.ParseMessage(messageBytes);



                    switch (msg.type)
                    {
                        case LiveMessageConstant.BEV_MESSAGE_TYPE_SYNCHRONIZE_ALL:
                            LiveMessageSynchronizeAll syncMsg = msg as LiveMessageSynchronizeAll;

                            if (!hololensHasInit)
                            {
                                // 这时的消息很可能是不对的！不能直接设置 

                                // 告诉hololens不要发了 
                                SetHololensSynchronize(false);

                                continue;
                            }

                            mainCameraTransform.position = syncMsg.position;
                            mainCameraTransform.eulerAngles = syncMsg.rotation;

                            anchorLocated = true;
                            for (int i = 0; i < anchorController.anchorObjectList.Count; i++)
                            {
                                if (i >= syncMsg.anchorCount)
                                    continue;

                                AnchorObjectInfo info = anchorController.anchorObjectList[i];
                                info.rootObject.transform.position = syncMsg.anchorPositionList[i];
                                info.rootObject.transform.eulerAngles = syncMsg.anchorRotationList[i];
                                //info.mark.followRoot = true;
                                //info.FollowRootObject();

                                if (!syncMsg.anchorIsLocated[i])
                                    anchorLocated = false;

                                //Debug.Log(" --->Set Anchor [" + info.anchorName + "] to " + syncMsg.anchorPositionList[i] + " | " + syncMsg.anchorRotationList[i]);
                            }

                            // 重设一下状态 
                            lastSyncTime = Time.time;
                            hololensStartSynchronize = true;
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
                                panel.ShowInfoDialog("Spectator View's verion is too old!");
                                hololensConnected = false;
                                StopHololesServer();
                            }

                            waiting = false;
                            waitingString = "init OK!";
                            hololensHasInit = true;

                            // 初始化完成，就先开启位置传输 
                            SetHololensSynchronize(true);
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
#endif
        }

        public void SetHololensSynchronize(bool b)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

            if (server == null)
                return;

            if (b)
            {
                LiveMessage msg = new LiveMessage();

                Debug.Log("send!");
                msg.type = LiveMessageConstant.BEV_MESSAGE_TYPE_START;
                handler.SendMessage(msg.Serialize(), null);
            }
            else
            {
                LiveMessage msg = new LiveMessage();
                msg.type = LiveMessageConstant.BEV_MESSAGE_TYPE_STOP;
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
#endif
        }

        /// <summary>
        /// 将anchor信息初始化到Hololens端 
        /// </summary>
        public void SetAnchorToHololens(bool isInit)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

            if (!hololensConnected)
                return;

            LiveMessageSetAnchor msg = new LiveMessageSetAnchor();
            msg.type = LiveMessageConstant.BEV_MESSAGE_TYPE_SET_ANCHOR;
            msg.anchorData = new LiveMessageSetAnchor.LiveMessageSetAnchorData();
            msg.anchorData.serverHost = anchorController.serverHost;
            msg.anchorData.serverPort = anchorController.serverPort;
            msg.anchorData.appId = anchorController.appId;
            msg.anchorData.roomId = anchorController.roomId;
            msg.anchorData.isInit = isInit;

            for (int i = 0; i < anchorController.anchorObjectList.Count; i++)
            {
                AnchorObjectInfo info = anchorController.anchorObjectList[i];
                msg.anchorData.anchorNameList.Add(info.anchorName);
                msg.anchorData.anchorPosition.Add(info.rootObject.transform.position);
                msg.anchorData.anchorForward.Add(info.rootObject.transform.forward);
            }

            handler.SendMessage(msg.Serialize(), null);
            waiting = true;
            waitingString = "Waiting for anchor init...";
#endif
        }

        /// <summary>
        /// 将anchor信息存储到Hololens端 
        /// </summary>
        public void SaveAnchorToHololens()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

            if (!hololensConnected)
                return;

            LiveMessageSaveAnchor msg = new LiveMessageSaveAnchor();
            msg.type = LiveMessageConstant.BEV_MESSAGE_TYPE_SET_ANCHOR;
            msg.anchorData = new LiveMessageSaveAnchor.LiveMessageSetAnchorData();
            msg.anchorData.serverHost = anchorController.serverHost;
            msg.anchorData.serverPort = anchorController.serverPort;
            msg.anchorData.appId = anchorController.appId;
            msg.anchorData.roomId = anchorController.roomId;

            for (int i = 0; i < anchorController.anchorObjectList.Count; i++)
            {
                AnchorObjectInfo info = anchorController.anchorObjectList[i];
                msg.anchorData.anchorNameList.Add(info.anchorName);
                msg.anchorData.anchorPosition.Add(info.rootObject.transform.position);
                msg.anchorData.anchorForward.Add(info.rootObject.transform.forward);
            }

            handler.SendMessage(msg.Serialize(), null);
            waiting = true;
            waitingString = "Waiting for anchor init...";
#endif
        }

        public void DownloadAnchor()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            LiveMessage msg = new LiveMessage();
            msg.type = LiveMessageConstant.BEV_MESSAGE_TYPE_DOWNLOAD_ANCHOR;
            handler.SendMessage(msg.Serialize(), null);
            waiting = true;
            waitingString = "Waiting for download anchors...";
#endif
        }

        public void StartMoveAnchor()
        {
            if (cbStartMoveAnchor != null)
                cbStartMoveAnchor();

            anchorController.cbAnchorControlFinish = AnchorControlFinish;
            StartCoroutine(StartSceneAnchorController());
        }

        private IEnumerator StartSceneAnchorController()
        {
            yield return null;
            anchorController.TurnOn();
        }

        private void AnchorControlFinish()
        {
            if (inputManager != null)
            {
                //inputManager.StopCapture();
            }

            anchorController.TurnOff();

            if (cbEndMoveAnchor != null)
                cbEndMoveAnchor();

            // 如果hololens存在，发送存储 
            SaveAnchorToHololens();
        }


        public void StartCapture()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            currentName = nameGenerator.GetName();
            for (int i = 0;i < listenerList.Count;i ++)
            {
                listenerList[i].OnRecordStart(currentName);
            }
            //StartRecording();
            StartRecordingMe(outputPath, currentName + ".mp4");
#endif
        }

        public void StopCapture()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            StopRecording();
            //Debug.Log("[" + index + "] Record Finish！");

            for (int i = 0; i < listenerList.Count; i++)
            {
                listenerList[i].OnRecordStop(outputPath, currentName);
            }

#endif
        }

        public void TakeSnap()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            TakePictureMe(outputPath, nameGenerator.GetName() + ".png");
#endif
        }

    }
}
