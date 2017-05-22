using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DataMesh.AR.Event;

namespace DataMesh.AR.SpectatorView
{

    public class LiveControllerPanel : MonoBehaviour
    {
        public Button buttonStart;
        public Button buttonStop;
        public Button DownloadAnchor;
        public Button DownloadSpatial;
        public Button MoveAnchor;
        public Button StartCapture;
        public Button StopCapture;
        public Button TakeSnap;
        public Button FullScreen;
        public Button ClosePreview;

        public Button StartHololensSpectatorView;


        public LivePreview livePreview;
        private RectTransform LivePreviewPanel;
        public RectTransform LivePreviewImage;
        public GameObject ControlPanel;

        public GameObject InfoDialog;
        public Text InfoDialogText;
        public Button infoDialogClose;

        public GameObject SpectatorViewPanel;
        public GameObject HololensPanel;

        public InputField frameOffsetInput;
        public Slider alphaSlider;

        public Text statusText;
        public Text systemInfoText;
        public Text waitingText;
        public Text recordText;
        public Text anchorLocatedText;

        private LiveController liveController;

        private bool hasConnected = false;
        private bool hasSync = false;

        private bool isRecoding = false;

        private float screenWidth;
        private float screenHeight;
        private float aspect;

        private float frameAspect;


        private bool isFullScreen = false;


        private Canvas canvas;

        // Use this for initialization
        public void Init(LiveController b, float _frameAspect)
        {
            liveController = b;

            EventTriggerListener.Get(buttonStart.gameObject).onClick = OnStartClick;
            EventTriggerListener.Get(buttonStop.gameObject).onClick = OnStopClick;
            EventTriggerListener.Get(DownloadAnchor.gameObject).onClick = OnDownloadAnchorClick;
            EventTriggerListener.Get(MoveAnchor.gameObject).onClick = OnMoveAnchor;
            EventTriggerListener.Get(StartCapture.gameObject).onClick = OnStartCapture;
            EventTriggerListener.Get(StopCapture.gameObject).onClick = OnStopCapture;
            EventTriggerListener.Get(TakeSnap.gameObject).onClick = OnTakeSnap;

            EventTriggerListener.Get(ClosePreview.gameObject).onClick = OnShowHidePreview;
            EventTriggerListener.Get(FullScreen.gameObject).onClick = OnFullScreen;

            EventTriggerListener.Get(StartHololensSpectatorView.gameObject).onClick = OnStartHololensConnect;

            alphaSlider.onValueChanged.AddListener(OnSliderChange);
            frameOffsetInput.onValueChanged.AddListener(OnFrameOffsetInput);

            waitingText.text = "";
            systemInfoText.text = "";
            recordText.gameObject.SetActive(false);
            anchorLocatedText.gameObject.SetActive(false);

            InfoDialogText.text = "";
            InfoDialog.SetActive(false);
            EventTriggerListener.Get(infoDialogClose.gameObject).onClick = OnCloseInfoDialog;

            canvas = GetComponent<Canvas>();

            aspect = (float)Screen.width / (float)Screen.height;
            screenWidth = 1024;
            screenHeight = screenWidth / aspect;

            Debug.Log("w=" + screenWidth + " h=" + screenHeight + " a=" + aspect);

            frameAspect = _frameAspect;



            RefreshInput();

            LivePreviewPanel = livePreview.transform as RectTransform;
            livePreview.Init(liveController);
            RefreshPreview();

            HololensPanel.SetActive(false);
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

        private void RefreshInput()
        {
            alphaSlider.value = liveController.alpha;
            frameOffsetInput.text = liveController.frameOffset.ToString();
        }

        private void OnSliderChange(float value)
        {
            liveController.alpha = value;
        }

        private void OnFrameOffsetInput(string value)
        {
            float frame = -9999;
            float.TryParse(value, out frame);

            if (frame != -9999)
            {
                liveController.frameOffset = frame;
            }

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

                return;
            }

            if (isRecoding)
            {
                StartCapture.gameObject.SetActive(false);
                StopCapture.gameObject.SetActive(true);
            }
            else
            {
                StartCapture.gameObject.SetActive(true);
                StopCapture.gameObject.SetActive(false);
            }

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

            anchorLocatedText.gameObject.SetActive(!liveController.anchorLocated);

            MoveAnchor.interactable = true;
            StartCapture.interactable = true;
            StopCapture.interactable = true;

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

                    waitingText.text = liveController.waitingString;
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
                    waitingText.text = liveController.waitingString;

                }
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
        }

        private void OnStartClick(GameObject go)
        {
            liveController.SetHololensSynchronize(true);
        }


        private void OnStopClick(GameObject go)
        {
            liveController.SetHololensSynchronize(false);
        }

        private void OnSaveAnchorClick(GameObject go)
        {
            liveController.SaveAnchorToHololens();
        }

        private void OnDownloadAnchorClick(GameObject go)
        {
            liveController.DownloadAnchor();
        }

        private void OnMoveAnchor(GameObject go)
        {
            liveController.StartMoveAnchor();
        }

        private void OnStartCapture(GameObject go)
        {
            liveController.StartCapture();
            isRecoding = true;
            recordText.gameObject.SetActive(true);
        }

        private void OnStopCapture(GameObject go)
        {
            liveController.StopCapture();
            isRecoding = false;
            recordText.gameObject.SetActive(false);
        }

        public void OnTakeSnap(GameObject go)
        {
            liveController.TakeSnap();
            systemInfoText.text = "Save Picture OK!";
        }

        private void OnFullScreen(GameObject go)
        {
            isFullScreen = true;
            ControlPanel.gameObject.SetActive(false);
            RefreshPreview();
        }

        private void ExitFullScreen()
        {
            isFullScreen = false;
            ControlPanel.gameObject.SetActive(true);
            RefreshPreview();
        }

        private void RefreshPreview()
        {
            if (isFullScreen)
            {
                float w = screenWidth;
                float h = w / frameAspect;
                LivePreviewPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, screenWidth);
                LivePreviewPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, screenHeight);
                LivePreviewImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
                LivePreviewImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
            }
            else
            {
                float w = screenWidth / 3;
                float h = w / frameAspect;
                LivePreviewPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
                LivePreviewPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
                LivePreviewImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
                LivePreviewImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
            }
        }

        private void OnShowHidePreview(GameObject go)
        {
            if (LivePreviewPanel.gameObject.activeSelf)
                LivePreviewPanel.gameObject.SetActive(false);
            else
                LivePreviewPanel.gameObject.SetActive(true);
        }

        private void OnStartHololensConnect(GameObject go)
        {
            liveController.StartHololensServer();
            SpectatorViewPanel.SetActive(false);
            HololensPanel.SetActive(true);
        }
    }
}