using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DataMesh.AR.Event;

namespace DataMesh.AR.SpectatorView
{

    public class LiveSpectatorViewHololensPanel : MonoBehaviour
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

        public Button DownloadAnchor;
        public Button DownloadSpatial;

        public Button buttonStart;
        public Button buttonStop;
        public Button buttonOpenStatus;
        public Button buttonCloseStatus;

        public Text statusText;
        public Text waitingText;
        public Text anchorLocatedText;

        public LiveHololensStatusPanel statusPanel;

        private LiveController liveController;
        private LiveControllerUI controlPanel;

        private bool hasConnected = false;
        private bool hasSync = false;

        private bool hasInit = false;

        public void Init(LiveController b, LiveControllerUI panel)
        {
            liveController = b;
            controlPanel = panel;

            ETListener.Get(buttonStart.gameObject).onClick = OnStartClick;
            ETListener.Get(buttonStop.gameObject).onClick = OnStopClick;
            ETListener.Get(DownloadAnchor.gameObject).onClick = OnDownloadAnchorClick;
            ETListener.Get(DownloadSpatial.gameObject).onClick = OnDownloadSpatialClick;
            ETListener.Get(buttonOpenStatus.gameObject).onClick = OnOpenStatusPanel;
            ETListener.Get(buttonCloseStatus.gameObject).onClick = OnCloseStatusPanel;

            waitingText.text = "";

            statusText.text = "Hololens offline";
            statusText.color = new Color(1f, 0.5f, 0.5f);

            anchorLocatedText.gameObject.SetActive(false);

            buttonOpenStatus.gameObject.SetActive(true);
            buttonCloseStatus.gameObject.SetActive(false);

            statusPanel.gameObject.SetActive(false);

            hasInit = true;
        }

        void Update()
        {
            if (!hasInit)
                return;

            if (liveController.hololensStartSynchronize)
            {
                buttonStart.gameObject.SetActive(false);
                buttonStop.gameObject.SetActive(true);
            }
            else
            {
                buttonStart.gameObject.SetActive(true);
                buttonStop.gameObject.SetActive(false);
            }

            if (liveController.hololensConnected)
            {
                if (!hasConnected)
                {
                    hasConnected = true;
                    statusText.text = "Hololens connected";
                    statusText.color = new Color(0.5f, 1f, 0.5f);
                }

                if (liveController.waiting)
                {
                    buttonStart.interactable = false;
                    buttonStop.interactable = false;
                    DownloadAnchor.interactable = false;
                    DownloadSpatial.interactable = false;
                }
                else
                {
                    if (liveController.hololensStartSynchronize)
                    {
                        hasSync = true;

                        buttonStart.interactable = false;
                        buttonStop.interactable = true;
                        DownloadAnchor.interactable = false;
                        DownloadSpatial.interactable = false;


                    }
                    else
                    {
                        hasSync = false;

                        buttonStart.interactable = true;
                        buttonStop.interactable = false;
                        DownloadAnchor.interactable = true;
                        DownloadSpatial.interactable = true;
                    }

                }
                if (liveController.waitingString != null)
                    waitingText.text = liveController.waitingString;
                else
                    waitingText.text = "";
            }
            else
            {
                if (hasConnected)
                {
                    hasConnected = false;
                    statusText.text = "Hololens offline";
                    statusText.color = new Color(1f, 0.5f, 0.5f);
                }

                buttonStart.interactable = false;
                buttonStop.interactable = false;
                DownloadAnchor.interactable = false;
                DownloadSpatial.interactable = false;
            }

            anchorLocatedText.gameObject.SetActive(!liveController.anchorLocated);

        }


        private void OnStartClick(GameObject go)
        {
            liveController.SetHololensSynchronize(true);
        }


        private void OnStopClick(GameObject go)
        {
            liveController.SetHololensSynchronize(false);
        }

        private void OnDownloadSpatialClick(GameObject go)
        {
            liveController.DownloadSpatial();
        }

        private void OnDownloadAnchorClick(GameObject go)
        {
            liveController.DownloadAnchor();
        }

        public void OnOpenStatusPanel(GameObject go)
        {
            statusPanel.gameObject.SetActive(true);
            buttonOpenStatus.gameObject.SetActive(false);
            buttonCloseStatus.gameObject.SetActive(true);

            liveController.wdpController.OnOpenStatusPanel();
        }

        public void OnCloseStatusPanel(GameObject go)
        {
            statusPanel.gameObject.SetActive(false);
            buttonOpenStatus.gameObject.SetActive(true);
            buttonCloseStatus.gameObject.SetActive(false);
        }

#endif
    }
}