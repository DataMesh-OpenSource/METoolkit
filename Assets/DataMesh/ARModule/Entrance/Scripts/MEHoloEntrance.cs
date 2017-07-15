using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataMesh.AR;
using DataMesh.AR.Anchor;
using DataMesh.AR.Interactive;
using DataMesh.AR.SpectatorView;
using DataMesh.AR.UI;
using DataMesh.AR.Network;
using DataMesh.AR.MRC;
using DataMesh.AR.Storage;
using DataMesh.AR.Utility;

namespace DataMesh.AR
{
    public class MEHoloEntrance : MonoBehaviour
    {
        public static MEHoloEntrance Instance { get; private set; }

        public string AppID = null;

        public GameObject CommonPrefab;
        public GameObject AnchorPrefab;
        public GameObject InputPrefab;
        public GameObject SpeechPrefab;
        public GameObject UIPerfab;
        public GameObject CollaborationPrefab;
        public GameObject StoragePrefab;
        public GameObject SocialPrefab;
        public GameObject LivePrefab;

        [HideInInspector]
        public bool NeedAnchor = true;
        [HideInInspector]
        public bool NeedInput = true;
        [HideInInspector]
        public bool NeedSpeech = true;
        [HideInInspector]
        public bool NeedUI = true;
        [HideInInspector]
        public bool NeedCollaboration = true;
        [HideInInspector]
        public bool NeedStorage = true;
        [HideInInspector]
        public bool NeedSocial = true;
        [HideInInspector]
        public bool NeedLive = true;

        [HideInInspector]
        public bool HasInit = false;



        private SceneAnchorController anchorController;
        private LiveController liveController;
        private MultiInputManager inputManager;
        private SpeechManager speechManager;
        private UIManager uiManager;
        private CollaborationManager collaborationManager;
        private MixedRealityCapture mrc;
        private StorageManager storageManager;

        private List<bool> moduleSwitch = new List<bool>();
        private List<MEHoloModule> moduleList = new List<MEHoloModule>();

        void Awake()
        {
            Instance = this;
        }

        // Use this for initialization
        void Start()
        {
            StartCoroutine(CheckSystem());
        }

        /// <summary>
        /// 检查系统是否满足条件 
        /// </summary>
        /// <returns></returns>
        private IEnumerator CheckSystem()
        {
            // 检查应用名称是否已经设置 
            while (string.IsNullOrEmpty(AppID))
            {
                Debug.LogError("AppName can not be null! Please set AppName.");
                yield return new WaitForSeconds(1);
            }

            InitSystem();
        }

        /// <summary>
        /// 初始化系统 
        /// </summary>
        private void InitSystem()
        {
            anchorController = SceneAnchorController.Instance;
            liveController = LiveController.Instance;
            inputManager = MultiInputManager.Instance;
            speechManager = SpeechManager.Instance;
            uiManager = UIManager.Instance;
            collaborationManager = CollaborationManager.Instance;
            storageManager = StorageManager.Instance;

            mrc = MixedRealityCapture.Instance;

            moduleSwitch.Add(NeedAnchor);
            moduleSwitch.Add(NeedInput);
            moduleSwitch.Add(NeedSpeech);
            moduleSwitch.Add(NeedUI);
            moduleSwitch.Add(NeedCollaboration);
            moduleSwitch.Add(NeedStorage);
            moduleSwitch.Add(NeedSocial);
            moduleSwitch.Add(NeedLive);

            moduleList.Add(anchorController);
            moduleList.Add(inputManager);
            moduleList.Add(speechManager);
            moduleList.Add(uiManager);
            moduleList.Add(collaborationManager);
            moduleList.Add(storageManager);
            moduleList.Add(mrc);
            moduleList.Add(liveController);

            // 按需求启动模块 
            for (int i = 0; i < moduleSwitch.Count; i++)
            {
                if (moduleList[i] == null)
                    continue;

                if (moduleSwitch[i])
                {
                    moduleList[i].gameObject.SetActive(true);
                    moduleList[i].Init();
                }
                else
                {
                    moduleList[i].gameObject.SetActive(false);
                }
            }

            HasInit = true;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}