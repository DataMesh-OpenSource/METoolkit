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
using DataMesh.AR.Utility;

namespace DataMesh.AR
{
    public class MEHoloEntrance : MonoBehaviour
    {

        public static MEHoloEntrance Instance { get; private set; }

        public GameObject CommonPrefab;
        public GameObject AnchorPrefab;
        public GameObject InputPrefab;
        public GameObject SpeechPrefab;
        public GameObject MenuPrefab;
        public GameObject CursorPrefab;
        public GameObject CollaborationPrefab;
        public GameObject SocialPrefab;
        public GameObject LivePrefab;

        [HideInInspector]
        public bool NeedAnchor = true;
        [HideInInspector]
        public bool NeedInput = true;
        [HideInInspector]
        public bool NeedSpeech = true;
        [HideInInspector]
        public bool NeedMenu = true;
        [HideInInspector]
        public bool NeedCursor = true;
        [HideInInspector]
        public bool NeedCollaboration = true;
        [HideInInspector]
        public bool NeedSocial = true;
        [HideInInspector]
        public bool NeedLive = true;

        [HideInInspector]
        public bool HasInit = false;



        private SceneAnchorController anchorController;
        private LiveController bevController;
        private CursorController cursorController;
        private MultiInputManager inputManager;
        private SpeechManager speechManager;
        private BlockMenuManager menuManager;
        private CollaborationManager collaborationManager;
        private MixedRealityCapture mrc;

        private List<bool> moduleSwitch = new List<bool>();
        private List<MEHoloModule> moduleList = new List<MEHoloModule>();

        void Awake()
        {
            Instance = this;
        }

        // Use this for initialization
        void Start()
        {
            anchorController = SceneAnchorController.Instance;
            bevController = LiveController.Instance;
            cursorController = CursorController.Instance;
            inputManager = MultiInputManager.Instance;
            speechManager = SpeechManager.Instance;
            menuManager = BlockMenuManager.Instance;
            collaborationManager = CollaborationManager.Instance;
            mrc = MixedRealityCapture.Instance;

            moduleSwitch.Add(NeedAnchor);
            moduleSwitch.Add(NeedInput);
            moduleSwitch.Add(NeedSpeech);
            moduleSwitch.Add(NeedMenu);
            moduleSwitch.Add(NeedCursor);
            moduleSwitch.Add(NeedCollaboration);
            moduleSwitch.Add(NeedSocial);
            moduleSwitch.Add(NeedLive);

            moduleList.Add(anchorController);
            moduleList.Add(inputManager);
            moduleList.Add(speechManager);
            moduleList.Add(menuManager);
            moduleList.Add(cursorController);
            moduleList.Add(collaborationManager);
            moduleList.Add(mrc);
            moduleList.Add(bevController);

            // 按需求启动模块 
            for (int i = 0;i < moduleSwitch.Count;i ++)
            {
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