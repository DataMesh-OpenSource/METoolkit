using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DataMesh.AR.Event;

namespace DataMesh.AR.SpectatorView
{

    public class LiveControllerUI : MonoBehaviour
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

        public LiveUIForms liveUIForms;
        public LiveHololensStatusPanel hololensStatusPanel;
        public LivePreview livePreview;
        public SocialControl socialControl;

        public GameObject InfoDialog;
        public Text InfoDialogText;
        public Button infoDialogClose;

        private LiveController liveController;

        [HideInInspector]
        public bool isModifyAnchor = false;
        [HideInInspector]
        public bool isFullScreen = false;

        // Use this for initialization
        public void Init(LiveController b, float _frameAspect)
        {
            liveController = b;

            InfoDialogText.text = "";
            InfoDialog.SetActive(false);
            ETListener.Get(infoDialogClose.gameObject).onClick = OnCloseInfoDialog;

            CanvasScaler canvasScaler = GetComponent<CanvasScaler>();
            float screenWidth = canvasScaler.referenceResolution.x;
            livePreview.Init(liveController, screenWidth, _frameAspect);

            liveUIForms.functionUIForm.Init(b, this);
            socialControl.Init();
            liveUIForms.anchorControlUIForm.Init();
            liveUIForms.hololensAgentUIForm.Init();
            liveUIForms.mediaOperationUIForm.Init();
            liveUIForms.socialUIForm.Init();

            hololensStatusPanel.gameObject.SetActive(false);

        }

        private void Start()
        {


        }

        private void OnCloseInfoDialog(GameObject obj)
        {
            InfoDialog.SetActive(false);
        }

        public void ShowInfoDialog(string info)
        {
            InfoDialogText.text = info;
            InfoDialog.SetActive(true);
        }

        public void TurnOn()
        {
            gameObject.SetActive(true);
        }

        public void TurnOff()
        {
            gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnSaveAnchorClick(GameObject go)
        {
            liveController.SaveAnchorToHololens();
        }


        public void OnStartHololensConnect()
        {
            // spectatorViewSelectPanel.gameObject.SetActive(false);
            // spectatorViewHololensPanel.gameObject.SetActive(true);
        }


#endif
    }
}