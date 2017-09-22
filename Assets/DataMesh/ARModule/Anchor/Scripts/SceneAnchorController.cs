using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataMesh.AR.Interactive;

#if UNITY_METRO && !UNITY_EDITOR
using UnityEngine.VR.WSA;
using UnityEngine.VR.WSA.Persistence;
using UnityEngine.VR.WSA.Sharing;

#else
using DataMesh.AR.FakeUWP;
#endif


namespace DataMesh.AR.Anchor
{
    public enum SpatialAdjustType
    {
        None,
        Fitting,
        Move,
        Rotate
    }

    public enum AnchorObjectStatus
    {
        StoreNotReady,
        HasNotSet,
        Saved,
        NoAnchorObject
    }

    public class AnchorObjectInfo
    {
        public string anchorName;
        public GameObject rootObject;
        public AnchorMark mark;

        public WorldAnchor anchor;
        public AnchorDefinition definition;

        public bool needSave = false;
        public bool saveSucc = true;

        public AnchorObjectStatus status = AnchorObjectStatus.HasNotSet;

        /*
        public void FollowRootObject()
        {
            mark.transform.position = rootObject.transform.position + mark.offset;
            mark.transform.rotation = rootObject.transform.rotation;
        }
        */

        public void FollowMark()
        {
            rootObject.transform.position = mark.transform.position - mark.offset;
            rootObject.transform.rotation = mark.transform.rotation;

            CheckLockRotation();
        }

        public void CheckLockRotation()
        {
            /*
            Vector3 eular = rootObject.transform.eulerAngles;
            if (definition.lockX == AnchorDefinition.LockType.FollowCoordinate)
            {
                eular.x = 0;
            }
            else if (definition.lockX == AnchorDefinition.LockType.Lock)
            {
                eular.x = definition.originEular.x;
            }

            if (definition.lockY == AnchorDefinition.LockType.FollowCoordinate)
            {
                eular.y = 0;
            }
            else if (definition.lockY == AnchorDefinition.LockType.Lock)
            {
                eular.y = definition.originEular.y;
            }

            if (definition.lockZ == AnchorDefinition.LockType.FollowCoordinate)
            {
                eular.z = 0;
            }
            else if (definition.lockZ == AnchorDefinition.LockType.Lock)
            {
                eular.z = definition.originEular.z;
            }

            rootObject.transform.eulerAngles = eular;
            */
        }

        public void SetTransform(Vector3 pos,Vector3 up)
        {
            //mark.transform.position = pos;
            rootObject.transform.position = pos;
            //mark.transform.up = up;
            rootObject.transform.up = up;
            CheckLockRotation();
        }
        public void SetPosition(Vector3 pos)
        {
            rootObject.transform.position = pos;
            CheckLockRotation();
        }
        public void SetEular(Vector3 eular)
        {
            rootObject.transform.eulerAngles = eular;
            CheckLockRotation();
        }
        public void Rotate(Vector3 rotate, Space space)
        {
            //rootObject.transform.localEulerAngles += rotate;
            rootObject.transform.Rotate(rotate, space);
            CheckLockRotation();
        }

        public void Clear()
        {
            GameObject.Destroy(mark.gameObject);
            rootObject = null;
            anchor = null;
        }
    }

    public class SceneAnchorController : DataMesh.AR.MEHoloModuleSingleton<SceneAnchorController>
    {
        private enum LayerMaskType
        {
            AnchorMark,
            Spacial,
            None
        }

        [Tooltip("Anchor移动标记所在的层级")]
        public int AnchorMarkLayer = 30;

        [HideInInspector]
        public List<string> anchorNameList = new List<string>();
        [HideInInspector]
        public List<GameObject> anchorRootList = new List<GameObject>();

        public List<AnchorObjectInfo> anchorObjectList = new List<AnchorObjectInfo>();

        public GameObject markPrefab;

        /// <summary>
        /// 因为SpectatorView中可能不止要处理一个app的信息，所以这里需要存储appID，不能直接使用MEHoloEntrance.AppName 
        /// </summary>
        [HideInInspector]
        public string appId = "";

        [HideInInspector]
        public string roomId = "";

        [HideInInspector]
        public string serverHost = "127.0.0.1";

        [HideInInspector]
        public int serverPort = 80;


        [HideInInspector]
        public SpatialAdjustType spatialAdjustType = SpatialAdjustType.None;

        /// <summary>
        /// 获得当时所有Anchor状态的综述 
        /// 如果是HasNotSet状态，应当提示使用者先设置Anchor
        /// </summary>
        public AnchorObjectStatus anchorStatusSummary
        {
            get
            {
                if (anchorStore == null)
                    return AnchorObjectStatus.StoreNotReady;
                if (anchorObjectList.Count == 0)
                    return AnchorObjectStatus.NoAnchorObject;

                AnchorObjectStatus rs = AnchorObjectStatus.Saved;
                for (int i = 0;i < anchorObjectList.Count;i ++)
                {
                    if (anchorObjectList[i].status == AnchorObjectStatus.HasNotSet)
                    {
                        rs = AnchorObjectStatus.HasNotSet;
                        break;
                    }
                }
                return rs;
            }
        }

        /// <summary>
        /// 对anchor操作全部完成后触发的回调，以便通知使用者，操作已经完成
        /// </summary>
        private System.Action cbAnchorControlFinish;
        public void AddCallbackFinish(System.Action cb) { cbAnchorControlFinish += cb; }
        public void RemoveCallbackFinish(System.Action cb) { cbAnchorControlFinish -= cb; }

        /// <summary>
        /// 开启调整时的回调 
        /// </summary>
        private System.Action cbTurnOn;
        public void AddCallbackTurnOn(System.Action cb) { cbTurnOn += cb; }
        public void RemoveCallbackTurnOn(System.Action cb) { cbTurnOn -= cb; }

        /// <summary>
        /// 关闭调正时的回调 
        /// </summary>
        private System.Action cbTurnOff;
        public void AddCallbackTurnOff(System.Action cb) { cbTurnOff += cb; }
        public void RemoveCallbackTurnOff(System.Action cb) { cbTurnOff -= cb; }

        protected WorldAnchorStore anchorStore;
        protected SpatialMappingManager spatialMappingManager;
        protected MeshSurfaceObserver meshObserver;

        protected MultiInputManager inputManager;
        protected AnchorShared anchorShared;



        public AnchorObjectInfo currentAnchorInfo;

        private bool needSaveLater = false;
        private List<AnchorObjectInfo> anchorToAdd = new List<AnchorObjectInfo>();

        private GameObject anchorMarkRoot;

        private int originInputLayer;
        private bool originNeedAssistKey;

        public Camera markCamera;
        //private GameObject markCameraObj;

        public int markLayer
        {
            get
            {
                return 1 << AnchorMarkLayer;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            WorldAnchorStore.GetAsync(AnchorStoreReady);

            anchorMarkRoot = new GameObject("AnchorMarkRoot");
            anchorMarkRoot.transform.parent = this.transform;
            anchorMarkRoot.transform.localPosition = Vector3.zero;
            anchorMarkRoot.transform.localRotation = Quaternion.identity;
            anchorMarkRoot.SetActive(false);

        }

        private void AnchorStoreReady(WorldAnchorStore store)
        {
            anchorStore = store;

            Debug.Log("Anchor store ready!! need save? " + needSaveLater);

            if (needSaveLater)
            {
                for (int i = 0;i < anchorToAdd.Count;i ++)
                {
                    Debug.Log("Re add anchor...");
                    AddAnchor(anchorToAdd[i]);
                }

                anchorToAdd.Clear();

                TrySaveAllSceneRootAnchor();

                needSaveLater = false;
            }
        }

        /// <summary>
        /// 初始化，仅设置信息，并不投入使用
        /// </summary>
        protected override void _Init()
        {
            appId = MEHoloEntrance.Instance.AppID;
            roomId = Network.RoomManager.Instance.GetCurrentRoom();

            Utility.AppConfig config = Utility.AppConfig.Instance;
            config.LoadConfig(MEHoloConstant.NetworkConfigFile);
            serverHost = Utility.AppConfig.Instance.GetConfigByFileName(MEHoloConstant.NetworkConfigFile, "Server_Host", "127.0.0.1");

            spatialMappingManager = SpatialMappingManager.Instance;
            meshObserver = spatialMappingManager.gameObject.GetComponent<MeshSurfaceObserver>();

            inputManager = MultiInputManager.Instance;
            anchorShared = gameObject.AddComponent<AnchorShared>();

            anchorShared.Init();

            // 设置所有的anchor信息 
            AnchorDefinition[] defines = GameObject.FindObjectsOfType<AnchorDefinition>();
            for (int i = 0;i < defines.Length;i ++)
            {
                AnchorDefinition define = defines[i];
                GameObject obj = define.gameObject;
                int find = anchorRootList.IndexOf(obj);
                if (find >= 0)
                {
                    anchorNameList.RemoveAt(find);
                    anchorRootList.RemoveAt(find);
                }

                string aName = define.anchorName;
                if (string.IsNullOrEmpty(aName))
                {
                    aName = obj.name;
                    define.anchorName = obj.name;
                }
                AddAnchorObject(aName, obj);
            }

            // 兼容老数据 
            for (int i = 0; i < anchorNameList.Count; i++)
            {
                if (anchorRootList[i] != null)
                {
                    AddAnchorObject(anchorNameList[i], anchorRootList[i]);
                    Debug.Log("Fit old anchor [" + anchorNameList[i] + "]");
                }
            }

            FitCamera();


            /*
            DataMesh.AR.Common.FollowMainCamera follow = markCameraObj.AddComponent<DataMesh.AR.Common.FollowMainCamera>();
            follow.positionOffset = Vector3.zero;
            follow.rotationOffset = Vector3.zero;
            */


            markCamera.gameObject.SetActive(false);

            Debug.Log("Anchor count=" + anchorObjectList.Count);

        }

        private void FitCamera()
        {
            Camera mainCamera = Camera.main;
            markCamera.fieldOfView = mainCamera.fieldOfView;
            markCamera.nearClipPlane = mainCamera.nearClipPlane;
            markCamera.farClipPlane = mainCamera.farClipPlane;
            markCamera.cullingMask = markLayer | LayerMask.GetMask("UI");
        }

        /// <summary>
        /// 真正投入使用。callback将会在调整完成之后触发
        /// </summary>
        protected override void _TurnOn()
        {
            Debug.Log("Turn On anchor!!!");

            FitCamera();

            anchorMarkRoot.SetActive(true);
            BindGazeManager(true);

            markCamera.gameObject.SetActive(true);

            if (cbTurnOn != null)
                cbTurnOn();

        }

        /// <summary>
        /// 关闭使用
        /// </summary>
        protected override void _TurnOff()
        {
            anchorMarkRoot.SetActive(false);
            BindGazeManager(false);

            markCamera.gameObject.SetActive(false);

            if (cbTurnOff != null)
                cbTurnOff();
        }

        /// <summary>
        /// 绑定输入
        /// </summary>
        /// <param name="bind"></param>
        private void BindGazeManager(bool bind)
        {
            if (bind)
            {
                inputManager.cbTap += OnTap;
                inputManager.cbNavigationStart += OnNavigationStart;
                inputManager.cbNavigationUpdate += OnNavigationUpdate;
                inputManager.cbNavigationEnd += OnNavigationEnd;
                inputManager.cbManipulationStart += OnManipulationStart;
                inputManager.cbManipulationUpdate += OnManipulationUpdate;
                inputManager.cbManipulationEnd += OnManipulationEnd;

                originInputLayer = inputManager.layerMask;
                SetLayerMask(LayerMaskType.AnchorMark);

                originNeedAssistKey = inputManager.needAssistKey;
                inputManager.needAssistKey = false;

                //Debug.Log("---------->" + originInputLayer);

            }
            else
            {
                inputManager.cbTap -= OnTap;
                inputManager.cbNavigationStart -= OnNavigationStart;
                inputManager.cbNavigationUpdate -= OnNavigationUpdate;
                inputManager.cbNavigationEnd -= OnNavigationEnd;
                inputManager.cbManipulationStart -= OnManipulationStart;
                inputManager.cbManipulationUpdate -= OnManipulationUpdate;
                inputManager.cbManipulationEnd -= OnManipulationEnd;

                inputManager.layerMask = originInputLayer;
                inputManager.needAssistKey = originNeedAssistKey;
            }
        }


        /// <summary>
        /// 设置输入的层过滤信息
        /// </summary>
        /// <param name="type"></param>
        private void SetLayerMask(LayerMaskType type)
        {
            switch (type)
            {
                case LayerMaskType.AnchorMark:
                    inputManager.layerMask = markLayer;
                    break;
                case LayerMaskType.Spacial:
                    inputManager.layerMask = SpatialMappingManager.Instance.LayerMask;
                    break;
                case LayerMaskType.None:
                    inputManager.layerMask = 0;
                    break;
            }

        }

        /// <summary>
        /// 手工设置一组Mesh，作为SpatialMapping信息，替代原有的Observer
        /// 主要用于电脑端模拟SpatialMapping使用。
        /// </summary>
        /// <param name="meshes">Mesh应为HoloLens中下载的数据</param>
        public void SetSpatialMeshToObserver(List<Mesh> meshes)
        {
            if (meshObserver != null)
            {
                meshObserver.SetMeshes(meshes);
            }
        }

        ////////////////////////////////////////////
        #region 创建、删除、本地存储Anchor相关内容
        ////////////////////////////////////////////

        /// <summary>
        /// 将指定的物体设置为Anchor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj"></param>
        public AnchorObjectInfo AddAnchorObject(string name, GameObject obj)
        {
            AnchorObjectInfo info = new AnchorObjectInfo();
            info.anchorName = name;
            info.rootObject = obj;

            AnchorDefinition define = obj.GetComponent<AnchorDefinition>();
            if (define == null)
            {
                define = obj.AddComponent<AnchorDefinition>();
                define.anchorName = name;
            }
            info.definition = define;

            GameObject markObj = PrefabUtils.CreateGameObjectToParent(anchorMarkRoot, markPrefab);
            AnchorMark mark = markObj.GetComponent<AnchorMark>();
            mark.Init(name, info);
            mark.rootObjectTransform = obj.transform;

            // 设置位置 
            Vector3 originPos = obj.transform.position;
            markObj.transform.position = originPos;

            info.mark = mark;

            AddAnchor(info);

            //info.mark.followRoot = true;
            //info.FollowRootObject();

            //mark.gameObject.SetActive(false);

            anchorObjectList.Add(info);

            Debug.Log("Add anchor [" + name + "] to obj [" + obj.name + "]");

            return info;
        }

        public AnchorObjectInfo GetAnchorInfo(string name)
        {
            for (int i = 0;i < anchorObjectList.Count;i ++)
            {
                AnchorObjectInfo info = anchorObjectList[i];
                if (info.anchorName == name)
                    return info;
            }
            return null;
        }

        /// <summary>
        /// 为一个物体添加anchor信息并存储。这里会先尝试读取Anchor，如果读取不到则创建并存储 
        /// </summary>
        /// <param name="info"></param>
        private void AddAnchor(AnchorObjectInfo info)
        {
            //Debug.Log("Add Anchor [" + info.anchorName + "]");

            //Debug.Log("  ---anchor=" + info.anchor + " store=" + anchorStore);
            if (anchorStore == null)
            {
                info.status = AnchorObjectStatus.StoreNotReady;
                needSaveLater = true;
                anchorToAdd.Add(info);
                return;
            }
            if (info.anchor == null)
            {
                string saveName = GetSaveAnchorName(info.anchorName);
                WorldAnchor savedAnchor = anchorStore.Load(saveName, info.rootObject);
                    
                if (savedAnchor != null)
                {
                    Debug.Log("Load anchor [" + saveName + "] success!");
                    info.anchor = savedAnchor;
                    //info.mark.followRoot = true;
                    //info.FollowRootObject();

                    info.status = AnchorObjectStatus.Saved;
                }
                else
                {
                    Debug.Log("Can not load anchor [" + info.anchorName + "]");
                    info.status = AnchorObjectStatus.HasNotSet;
                }

                /**
                 * 这里先不要添加anchor组件 
                if (info.anchor == null)
                {
                    CreateAnchor(info);
                    SaveAnchor(info);
                }
                */
            }
        }

        /// <summary>
        /// 为一个已有的对象，添加anchor组件，并不做存储和读取的动作
        /// </summary>
        /// <param name="info"></param>
        public void CreateAnchor(AnchorObjectInfo info)
        {
            if (info.anchor != null)
                return;

            Debug.Log("Create Anchor for [" + info.anchorName + "]");

            WorldAnchor anchor = info.rootObject.AddComponent<WorldAnchor>();
            info.anchor = anchor;

            info.status = AnchorObjectStatus.Saved;
        }

        /// <summary>
        /// 清除Anchor。注意，这里仅清除WorldAnchor，并不会清除info数据，也不会从存盘中删除Anchor 
        /// </summary>
        /// <param name="info"></param>
        public void RemoveAnchor(AnchorObjectInfo info)
        {
            WorldAnchor an = info.rootObject.GetComponent<WorldAnchor>();
            while (an != null)
            {
                DestroyImmediate(an);
                an = info.rootObject.GetComponent<WorldAnchor>();
            }

            info.anchor = null;
        }

        /// <summary>
        /// 放弃所有Anchor信息，会删除所有绑定物体上的WorldAnchor组件，并清空info信息
        /// 但是不会删除存盘信息
        /// </summary>
        public void ClearAllAnchorInfo(bool removeRootObject = false)
        {
            for (int i = 0;i < anchorObjectList.Count;i ++)
            {
                AnchorObjectInfo info = anchorObjectList[i];
                RemoveAnchor(info);

                info.Clear();

                if (removeRootObject)
                {
                    GameObject.DestroyImmediate(info.rootObject);
                }
            }
            anchorObjectList.Clear();
        }

        /// <summary>
        /// 强制存储所有anchor到本地
        /// </summary>
        public void SaveAllSceneRootAnchor()
        {
            for (int i = 0; i < anchorObjectList.Count; i++)
            {
                AnchorObjectInfo info = anchorObjectList[i];
                SaveAnchor(info);
            }
        }

        /// <summary>
        /// 检查所有anchor是否需要存储，如果有需要存储的则存储 
        /// </summary>
        private void TrySaveAllSceneRootAnchor()
        {
            for (int i = 0; i < anchorObjectList.Count; i++)
            {
                AnchorObjectInfo info = anchorObjectList[i];
                TrySaveAnchor(info);
            }
        }

        /// <summary>
        /// 强制存储一个Anchor
        /// </summary>
        /// <param name="info"></param>
        private void SaveAnchor(AnchorObjectInfo info)
        {
            if (info.anchor == null)
                return;

            Debug.Log("Save anchor [" + info.anchorName + "] ....");
            info.needSave = true;
            if (info.anchor.isLocated)
            {
                TrySaveAnchor(info);
            }
            else
            {
                info.anchor.OnTrackingChanged += Anchor_OnTrackingChanged;
            }
        }


        /// <summary>
        /// 检查一个anchor是否需要存储，如果需要则存储
        /// </summary>
        /// <param name="info"></param>
        private void TrySaveAnchor(AnchorObjectInfo info)
        {
            if (!info.needSave)
                return;

            if (anchorStore == null)
            {
                needSaveLater = true;
                return;
            }

            string saveName = GetSaveAnchorName(info.anchorName);

            // 需要先删除一下，否则同名无法存储
            anchorStore.Delete(saveName);

            if (anchorStore.Save(saveName, info.anchor))
            {
                info.saveSucc = true;
                Debug.Log("Anchor " + saveName + " Saved!");


            }
            else
            {
                info.saveSucc = false;
                Debug.LogError("Anchor " + saveName + " save error!");
            }


            info.needSave = false;
        }

        private string GetSaveAnchorName(string anchorName)
        {
            return "[" + appId + "][" + roomId + "]" + anchorName;
        }

        /// <summary>
        /// 延迟处理存储
        /// </summary>
        /// <param name="self"></param>
        /// <param name="located"></param>
        private void Anchor_OnTrackingChanged(WorldAnchor self, bool located)
        {
            if (located)
            {
                Debug.Log(self.name + " : World anchor located successfully.");

                // 因为无法记录是哪个anchor被存储，所以只能尝试存储所有anchor了 
                TrySaveAllSceneRootAnchor();

                self.OnTrackingChanged -= Anchor_OnTrackingChanged;
            }
            else
            {
                Debug.LogWarning(self.name + " : World anchor failed to locate.");
            }
        }

#endregion

        ////////////////////////////////////////////
#region 上传、下载Anchor数据
        ////////////////////////////////////////////

        /// <summary>
        /// 上传anchor资料
        /// 回调中两个参数：
        /// bool 表示是否成功
        /// string 表示失败原因
        /// </summary>
        /// <param name="cbFinish"></param>
        public void UploadAnchor(System.Action<bool, string> cbFinish)
        {
#if UNITY_METRO && !UNITY_EDITOR
            anchorShared.appId = appId;
            anchorShared.roomId = roomId;
            anchorShared.serverHost = serverHost;
            anchorShared.serverPort = serverPort;

            anchorShared.ExportGameRootAnchor(anchorObjectList, cbFinish);
#else
            // 非windows平台，直接调用成功的结果，跳过执行过程 
            cbFinish(true, null);
#endif
        }

        /// <summary>
        /// 下载一个anchor
        /// 回调中两个参数
        /// bool 表示是否成功
        /// string 表示失败原因
        /// </summary>
        /// <param name="cbFinish"></param>
        public void DownloadAnchor(System.Action<bool, string> cbFinish)
        {
#if UNITY_METRO && !UNITY_EDITOR
            anchorShared.appId = appId;
            anchorShared.roomId = roomId;
            anchorShared.serverHost = serverHost;
            anchorShared.serverPort = serverPort;

            anchorShared.ImportRootGameObject((bool succ, string error, WorldAnchorTransferBatch batch) =>
            {
                if (!succ)
                {
                    cbFinish(succ, error);
                }
                else
                {
                    bool saveSucc = true;
                    for (int i = 0; i < anchorObjectList.Count; i++)
                    {
                        AnchorObjectInfo info = anchorObjectList[i];
                        // 先删除anchor 
                        RemoveAnchor(info);

                        WorldAnchor anchor = batch.LockObject(info.anchorName, info.rootObject);
                        if (anchor == null)
                        {
                            Debug.LogWarning("Anchor [" + info.anchorName + "] can not be found in batch!");

                            saveSucc = false;

                            // 如果没读出来，则需要再次创建本地anchor 
                            CreateAnchor(info);

                        }
                        else
                        {
                            Debug.Log("Anchor [" + info.anchorName + "] has been load!");
                            Debug.Log("Andhor in Object? " + info.rootObject.GetComponent<WorldAnchor>());
                            info.anchor = anchor;
                            //info.FollowRootObject();
                        }


                    }
                    // 存储 
                    SaveAllSceneRootAnchor();

                    if (!saveSucc)
                        cbFinish(false, "Lock anchor failed!");
                    else
                        cbFinish(true, null);
                }
            });
#else
            // 非windows平台，直接调用成功的结果，跳过执行过程 
            cbFinish(true, null);
#endif
        }

#endregion


        // Update is called once per frame
        void Update()
        {
            FitMarkToSpatial();

            // 如果有接入手柄，则进行一些方便的设置 
            if (currentAnchorInfo != null && inputManager.controllerInput.hasContoller)
            {
                if (spatialAdjustType == SpatialAdjustType.None)
                {
                    if (inputManager.controllerInput.GetAxisLeftThumbstickX() != 0 || inputManager.controllerInput.GetAxisLeftThumbstickY() != 0)
                    {
                        ChangeAdjustType(SpatialAdjustType.Move);
                    }
                    else if (inputManager.controllerInput.GetAxisRightThumbstickX() != 0 || inputManager.controllerInput.GetAxisRightThumbstickY() != 0)
                    {
                        ChangeAdjustType(SpatialAdjustType.Rotate);
                    }
                }
                else if (spatialAdjustType == SpatialAdjustType.Move)
                {
                    if (inputManager.controllerInput.GetAxisLeftThumbstickX() == 0 && inputManager.controllerInput.GetAxisLeftThumbstickY() == 0)
                    {
                        ChangeAdjustType(SpatialAdjustType.None);
                    }
                }
                else if (spatialAdjustType == SpatialAdjustType.Rotate)
                { 
                    if (inputManager.controllerInput.GetAxisRightThumbstickX() == 0 && inputManager.controllerInput.GetAxisRightThumbstickY() == 0)
                    {
                        ChangeAdjustType(SpatialAdjustType.None);
                    }
                }
            }
        }

        ////////////////////////////////////////////
#region Anchor标志物操作相关
        ////////////////////////////////////////////

        private float moveSpeed = 1f;
        private float rotateSpeed = 60f;
        private Vector3 manipulationStartPos;
        private bool isFitting = false;
        private bool isNav = false;

        private Vector3 moveDirection = Vector3.up;

        private void FitMarkToSpatial()
        {
            if (isFitting)
            {
                if (inputManager.FocusedObject != null)
                {
                    Vector3 n = inputManager.hitNormal;
                    //n.y = 0;
                    currentAnchorInfo.SetTransform(inputManager.hitPoint, n);
                }
                else
                {
                    Vector3 headPosition = Camera.main.transform.position;
                    Vector3 gazeDirection = Camera.main.transform.forward;
                    //Vector3 gazeUp = Camera.main.transform.up;

                    Vector3 pos = headPosition + gazeDirection * 3;

                    currentAnchorInfo.SetPosition(pos);

                    //Debug.Log("pos=" + pos);
                }
            }
        }


        /// <summary>
        /// 选取一个Anchor的标志物
        /// </summary>
        /// <param name="mark"></param>
        private void SelectMark(AnchorMark mark)
        {
            if (currentAnchorInfo != null)
            {
                ChangeAdjustType(SpatialAdjustType.None);
                //currentAnchorInfo.mark.SetAdjustType(AnchorAdjestType.None);
                currentAnchorInfo.mark.SetMarkSelected(false);
                StopAdjust(currentAnchorInfo);
            }

            if (mark == null)
            {
                currentAnchorInfo = null;
                return;
            }

            for (int i = 0; i < anchorObjectList.Count; i++)
            {
                AnchorObjectInfo info = anchorObjectList[i];
                if (info.anchorName == mark.anchorName)
                {
                    currentAnchorInfo = info;

                    currentAnchorInfo.mark.SetMarkSelected(true);

                    StartAdjust(currentAnchorInfo);
                    break;
                }
            }
        }

        private void OnTap(int tapCount)
        {
            if (currentAnchorInfo == null)
            {
                if (inputManager.FocusedObject != null)
                {
                    AnchorMark mark = inputManager.FocusedObject.GetComponentInParent<AnchorMark>();
                    if (mark != null)
                    {
                        SelectMark(mark);
                    }
                }
                else
                {
                    FinishAdjust();
                }
            }
            else
            {
                if (isFitting)
                {
                    StopFit();
                }
                else
                {
                    if (inputManager.FocusedObject != null)
                    {
                        AnchorAdjestButton button = inputManager.FocusedObject.GetComponent<AnchorAdjestButton>();
                        if (button != null)
                        {
                            // 点到按钮了 
                            ChangeAdjustType(button.type);
                            //currentAnchorInfo.mark.SetAdjustType(button.type);
                        }
                        else
                        {
                            AnchorMark mark = inputManager.FocusedObject.GetComponentInParent<AnchorMark>();
                            if (mark != null)
                            {
                                // 点到anchor物体了，切换 
                                if (mark != currentAnchorInfo.mark)
                                {
                                    SelectMark(mark);
                                }
                            }

                        }
                    }
                    else
                    {
                        SelectMark(null);

                    }
                }
            }
        }


        private void StartAdjust(AnchorObjectInfo info)
        {
            RemoveAnchor(info);

            info.mark.StartAdjust();

            info.definition.startLock = true;

            //info.mark.followRoot = false;

            info.mark.ShowTips();
        }

        private void StopAdjust(AnchorObjectInfo info)
        {

            // 这里只能Create，不能Add，因为Add会先去store里加载一次 
            CreateAnchor(info);

            // 存储一下
            SaveAnchor(info);

            info.definition.startLock = false;

            ChangeAdjustType(SpatialAdjustType.None);
            info.mark.HideTips();

            //info.mark.followRoot = true;
        }

        private void FinishAdjust()
        {
            currentAnchorInfo = null;

            // 结束操作 
            if (cbAnchorControlFinish != null)
                cbAnchorControlFinish();
        }

        public void ChangeAdjustType(SpatialAdjustType type)
        {
            spatialAdjustType = type;

            if (type == SpatialAdjustType.Move)
            {
                inputManager.ChangeToManipulationRecognizer();
                currentAnchorInfo.mark.SetAdjustType(AnchorAdjestType.Move);
            }
            else if (type == SpatialAdjustType.Rotate)
            {
                inputManager.ChangeToNavigationRecognizer();
                currentAnchorInfo.mark.SetAdjustType(AnchorAdjestType.Rotate);
            }
            else if (type == SpatialAdjustType.Fitting)
            {
                inputManager.ChangeToManipulationRecognizer();
                currentAnchorInfo.mark.SetAdjustType(AnchorAdjestType.Free);
                // 直接开始fitting 
                //Debug.Log("start fitting!");
                StartFit();
            }
            else
            {
                inputManager.ChangeToManipulationRecognizer();
                currentAnchorInfo.mark.SetAdjustType(AnchorAdjestType.None);
            }

        }

        private void StartFit()
        {
            if (currentAnchorInfo == null)
                return;

            spatialMappingManager.DrawVisualMeshes = true;
            SetLayerMask(LayerMaskType.Spacial);


            currentAnchorInfo.mark.HideTips();

            StartCoroutine(StartFitLate());
        }

        private IEnumerator StartFitLate()
        {
            yield return null;
            isFitting = true;
        }

        private void StopFit()
        {
            isFitting = false;

            if (currentAnchorInfo == null)
                return;

            spatialMappingManager.DrawVisualMeshes = false;
            SetLayerMask(LayerMaskType.AnchorMark);

            currentAnchorInfo.mark.ShowTips();

            ChangeAdjustType(SpatialAdjustType.None);

        }


        private void OnNavigationStart(Vector3 delta)
        {
            if (currentAnchorInfo == null)
                return;

            if (spatialAdjustType == SpatialAdjustType.Rotate)
            {
                isNav = true;
                currentAnchorInfo.mark.HideTips();
            }
        }

        private void OnManipulationStart(Vector3 delta)
        {
            if (currentAnchorInfo == null)
                return;

            if (spatialAdjustType == SpatialAdjustType.Move)
            {
                isNav = true;
                manipulationStartPos = currentAnchorInfo.mark.transform.position;

                currentAnchorInfo.mark.HideTips();

            }
        }


        private void CalMoveDirection(Vector3 delta)
        {
            //Debug.Log("Delta=" + delta);

            float dx = Mathf.Abs(delta.x);
            float dy = Mathf.Abs(delta.y);
            float dz = Mathf.Abs(delta.z);

            if (dx == 0 && dy == 0 && dz == 0)
            {
                moveDirection = Vector3.zero;
                return;
            }

            if (dx > dy && dx > dz)
            {
                dx = moveSpeed;
                dy = 0;
                dz = 0;
            }
            else if (dy > dx && dy > dz)
            {
                dy = moveSpeed;
                dx = 0;
                dz = 0;
            }
            else
            {
                dz = moveSpeed;
                dx = 0;
                dy = 0;
            }

            moveDirection = new Vector3(dx, dy, dz);

            currentAnchorInfo.mark.ShowMoveAxis(moveDirection);
        }

        private void OnManipulationUpdate(Vector3 delta)
        {
            if (currentAnchorInfo == null)
                return;

            if (spatialAdjustType == SpatialAdjustType.Move)
            {
                if (isNav)
                {
                    Vector3 d = new Vector3(delta.x * moveSpeed, delta.y * moveSpeed, delta.z * moveSpeed);
                    //Debug.Log("M--->" + d);
                    currentAnchorInfo.SetPosition(manipulationStartPos + d);
                    //currentAnchorInfo.FollowMark();
                }
            }
        }

        private void OnNavigationUpdate(Vector3 delta)
        {
            if (currentAnchorInfo == null)
                return;

            if (spatialAdjustType == SpatialAdjustType.Rotate)
            {
                if (isNav)
                {
                    Vector3 deltaRot = new Vector3(delta.z, -delta.x, -delta.y);


                    currentAnchorInfo.Rotate(deltaRot * rotateSpeed * Time.deltaTime, Space.Self);
                }
            }
        }


        private void OnManipulationEnd(Vector3 delta)
        {
            isNav = false;
            if (currentAnchorInfo == null)
                return;

            currentAnchorInfo.mark.ShowTips();

            if (spatialAdjustType == SpatialAdjustType.Move)
            {
                currentAnchorInfo.mark.ShowMoveAxis(Vector3.one);
            }
        }

        private void OnNavigationEnd(Vector3 delta)
        {
            isNav = false;
            if (currentAnchorInfo == null)
                return;

            currentAnchorInfo.mark.ShowTips();

        }


        public void ShowAllMark(bool b)
        {
            for (int i = 0;i < anchorObjectList.Count;i ++)
            {
                AnchorObjectInfo info = anchorObjectList[i];
                if (b)
                    info.mark.ShowTips();
                else
                    info.mark.HideTips();
            }
        }
        #endregion




    }
}