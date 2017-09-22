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

        public LiveMainPanel mainPanel;
        public LiveSpectatorViewSelectPanel spectatorViewSelectPanel;
        public LiveSpectatorViewHololensPanel spectatorViewHololensPanel;
        public LiveHololensStatusPanel hololensStatusPanel;
        public LivePreview livePreview;

        public GameObject BottomBar;

        public GameObject InfoDialog;
        public Text InfoDialogText;
        public Button infoDialogClose;

        public LiveAlbumCloudPannel albumCloudPannel;


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
            EventTriggerListener.Get(infoDialogClose.gameObject).onClick = OnCloseInfoDialog;

            mainPanel.Init(liveController, this);

            CanvasScaler canvasScaler = GetComponent<CanvasScaler>();
            float screenWidth = canvasScaler.referenceResolution.x;
            livePreview.Init(liveController, screenWidth,  _frameAspect);

            spectatorViewSelectPanel.Init(b, this);
            spectatorViewHololensPanel.Init(b, this);
            hololensStatusPanel.Init(b);

            spectatorViewHololensPanel.gameObject.SetActive(false);
            spectatorViewSelectPanel.gameObject.SetActive(true);

            hololensStatusPanel.gameObject.SetActive(false);


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
            if (isFullScreen)
            {
                if (Input.GetKey(KeyCode.Escape))
                {
                    ExitFullScreen();
                }
            }

            // 确定下方菜单是否显示 
            if (isFullScreen || isModifyAnchor)
            {
                BottomBar.gameObject.SetActive(false);
            }
            else
            {
                BottomBar.gameObject.SetActive(true);
            }

        }

        public void OnStartCapture()
        {
            liveController.StartCapture();
            //livePreview.SetFullScreen(true);
        }

        public void OnStopCapture()
        {
            liveController.StopCapture();
            //livePreview.SetFullScreen(false);
        }

        public void OnFullScreen()
        {
            isFullScreen = true;
            livePreview.SetFullScreen(true);
            livePreview.gameObject.SetActive(true);

            //ShowBottomBar(false);
        }

        public void ExitFullScreen()
        {
            isFullScreen = false;
            livePreview.SetFullScreen(false);

            //ShowBottomBar(true);
        }


        private void OnSaveAnchorClick(GameObject go)
        {
            liveController.SaveAnchorToHololens();
        }



        public void OnShowHidePreview()
        {
            if (livePreview.gameObject.activeSelf)
                livePreview.gameObject.SetActive(false);
            else
                livePreview.gameObject.SetActive(true);
        }

        public void OnStartHololensConnect()
        {
            spectatorViewSelectPanel.gameObject.SetActive(false);
            spectatorViewHololensPanel.gameObject.SetActive(true);
        }

        public void ShowAlbumUI()
        {
            albumCloudPannel.gameObject.SetActive(true);
            albumCloudPannel.OpenAndRefreshAlbumProfileName();
        }

        public void HidingAlbumUI()
        {
            albumCloudPannel.CloseAlbumCloudPannelWindow();
        }

#endif
    }
}