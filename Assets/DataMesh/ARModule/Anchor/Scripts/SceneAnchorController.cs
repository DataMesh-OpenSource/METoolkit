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
        Rotate,
        Readjust
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
        }

        public void SetTransform(ref Vector3 pos, ref Vector3 up)
        {
            //mark.transform.position = pos;
            rootObject.transform.position = pos;
            //mark.transform.up = up;
            rootObject.transform.up = up;
        }
        public void SetTransform(ref Vector3 pos)
        {
            rootObject.transform.position = pos;
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

        public int appId = 1;

        public string roomId = "MyRoom";

        public string serverHost = "127.0.0.1";

        public int serverPort = 8823;


        [HideInInspector]
        public SpatialAdjustType spatialAdjustType = SpatialAdjustType.None;

        /// <summary>
        /// 对anchor操作全部完成后触发的回调，以便通知使用者，操作已经完成
        /// </summary>
        [HideInInspector]
        public System.Action cbAnchorControlFinish;

        protected WorldAnchorStore anchorStore;
        protected SpatialMappingManager spatialMappingManager;

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
            spatialMappingManager = SpatialMappingManager.Instance;

            inputManager = MultiInputManager.Instance;
            anchorShared = gameObject.AddComponent<AnchorShared>();

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
            FitCamera();

            anchorMarkRoot.SetActive(true);
            BindGazeManager(true);

            markCamera.gameObject.SetActive(true);


        }

        /// <summary>
        /// 关闭使用
        /// </summary>
        protected override void _TurnOff()
        {
            anchorMarkRoot.SetActive(false);
            BindGazeManager(false);

            markCamera.gameObject.SetActive(false);
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
            Debug.Log("Add Anchor [" + info.anchorName + "]");

            //Debug.Log("  ---anchor=" + info.anchor + " store=" + anchorStore);
            if (anchorStore == null)
            {
                needSaveLater = true;
                anchorToAdd.Add(info);
                return;
            }
            if (info.anchor == null)
            {
                if (anchorStore != null)
                {
                    string saveName = GetSaveAnchorName(info.anchorName);
                    WorldAnchor savedAnchor = anchorStore.Load(saveName, info.rootObject);
                    
                    if (savedAnchor != null)
                    {
                        Debug.Log("Load anchor [" + saveName + "] success!");
                        info.anchor = savedAnchor;
                        //info.mark.followRoot = true;
                        //info.FollowRootObject();
                    }
                    else
                    {
                        Debug.Log("Can not load anchor [" + info.anchorName + "]");
                    }
                }

                if (info.anchor == null)
                {
                    CreateAnchor(info);
                    SaveAnchor(info);
                }
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
        }

        /// <summary>
        /// 清除Anchor。注意，这里仅清除WorldAnchor，并不会清除info数据，也不会从存盘中删除Anchor 
        /// </summary>
        /// <param name="info"></param>
        public void RemoveAnchor(AnchorObjectInfo info)
        {
            if (info.anchor != null)
            {
                //anchorStore.Delete(info.anchorName);
                DestroyImmediate(info.anchor);
                info.anchor = null;
            }
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
        }

        ////////////////////////////////////////////
#region Anchor标志物操作相关
        ////////////////////////////////////////////

        private float moveSpeed = 1f;
        private float rotateSpeed = 0.3f;
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
                    currentAnchorInfo.SetTransform(ref inputManager.hitPoint, ref n);
                }
                else
                {
                    Vector3 headPosition = Camera.main.transform.position;
                    Vector3 gazeDirection = Camera.main.transform.forward;
                    //Vector3 gazeUp = Camera.main.transform.up;

                    Vector3 pos = headPosition + gazeDirection * 3;

                    currentAnchorInfo.SetTransform(ref pos);

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

            //info.mark.followRoot = false;

            info.mark.ShowTips();
        }

        private void StopAdjust(AnchorObjectInfo info)
        {

            // 这里只能Create，不能Add，因为Add会先去store里加载一次 
            CreateAnchor(info);

            // 存储一下
            SaveAnchor(info);

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
            else if (type == SpatialAdjustType.Readjust)
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
            if (spatialAdjustType == SpatialAdjustType.Readjust)
            {
                isNav = true;
                manipulationStartPos = currentAnchorInfo.mark.transform.position;

                currentAnchorInfo.mark.HideTips();

                CalMoveDirection(delta);

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
                    currentAnchorInfo.mark.transform.position = manipulationStartPos + d;
                    currentAnchorInfo.FollowMark();
                }
            }
            else if (spatialAdjustType == SpatialAdjustType.Readjust)
            {
                if (isNav)
                {
                    if (moveDirection == Vector3.zero)
                    {
                        CalMoveDirection(delta);
                        //Debug.Log("Move Dir=" + moveDirection);
                    }
                    else
                    {
                        Vector3 d = new Vector3(delta.x * moveDirection.x, delta.y * moveDirection.y, delta.z * moveDirection.z);
                        //Debug.Log("M--->" + d);
                        currentAnchorInfo.mark.transform.position = manipulationStartPos + d;
                        currentAnchorInfo.FollowMark();
                    }
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
                    Vector3 deltaRot = new Vector3(-delta.y, -delta.x, -delta.z);
                    currentAnchorInfo.mark.transform.Rotate(deltaRot * rotateSpeed, Space.Self);
                    currentAnchorInfo.FollowMark();
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
            if (spatialAdjustType == SpatialAdjustType.Readjust)
            {
                currentAnchorInfo.mark.ShowMoveAxis(Vector3.zero);
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