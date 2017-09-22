using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DataMesh.AR.Event;


namespace DataMesh.AR.SpectatorView
{

    public class LiveMainPanel : MonoBehaviour
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

        public Button MoveAnchor;
        public Button StartCapture;
        public Button StopCapture;
        public Button TakeSnap;
        public Button FullScreen;
        public Button ClosePreview;
        public Button buttonOpenAlbum;
        public Button buttonCloseAlbum;

        public InputField frameOffsetInput;
        public Slider alphaSlider;

        public Slider soundSlider;

        public InputField antiShakeBefore;
        public InputField antiShakeAfter;

        public Text systemInfoText;


        private bool isRecoding = false;

        private bool canRecordCPU = true;

        private LiveController liveController;
        private LiveControllerUI liveUI;

        // Use this for initialization
        public void Init(LiveController b, LiveControllerUI u)
        {
            liveController = b;
            liveUI = u;

            EventTriggerListener.Get(MoveAnchor.gameObject).onClick = OnMoveAnchor;
            EventTriggerListener.Get(StartCapture.gameObject).onClick = OnStartCapture;
            EventTriggerListener.Get(StopCapture.gameObject).onClick = OnStopCapture;
            EventTriggerListener.Get(TakeSnap.gameObject).onClick = OnTakeSnap;

            EventTriggerListener.Get(ClosePreview.gameObject).onClick = OnShowHidePreview;
            EventTriggerListener.Get(FullScreen.gameObject).onClick = OnFullScreen;

            EventTriggerListener.Get(buttonOpenAlbum.gameObject).onClick = OpenAlbum;
            EventTriggerListener.Get(buttonCloseAlbum.gameObject).onClick = CloseAlbum; 

            RefreshInput();

            alphaSlider.onValueChanged.AddListener(OnAlphaSliderChange);
            frameOffsetInput.onValueChanged.AddListener(OnFrameOffsetInput);

            soundSlider.onValueChanged.AddListener(OnSoundSliderChange);

            antiShakeBefore.onValueChanged.AddListener(OnAntiShakeBeforeChange);
            antiShakeAfter.onValueChanged.AddListener(OnAntiShakeAfterChange);

            systemInfoText.text = "";

            canRecordCPU = System.Environment.ProcessorCount >= 4;

        }

        private void RefreshInput()
        {
            frameOffsetInput.text = LiveParam.SyncDelayTime.ToString();
            antiShakeBefore.text = LiveParam.AntiShakeBeforeTime.ToString();
            antiShakeAfter.text = LiveParam.AntiShakeAfterTime.ToString();

            alphaSlider.value = LiveParam.Alpha;
            soundSlider.value = LiveParam.SoundVolume;
        }

        // Update is called once per frame
        void Update()
        {
            if (canRecordCPU)
                StartCapture.interactable = true;  
            else
                StartCapture.interactable = false;
        }


        private void OnMoveAnchor(GameObject go)
        {
            liveController.StartMoveAnchor();
        }

        private void OnStartCapture(GameObject go)
        {
            if (!isRecoding && canRecordCPU)
            {
                StartCapture.gameObject.SetActive(false);
                StopCapture.gameObject.SetActive(true);
                isRecoding = true;
                liveUI.OnStartCapture();
            }

        }

        private void OnStopCapture(GameObject go)
        {
            if (isRecoding)
            {
                StartCapture.gameObject.SetActive(true);
                StopCapture.gameObject.SetActive(false);
                isRecoding = false;
                liveUI.OnStopCapture();
            }
        }

        public void OnTakeSnap(GameObject go)
        {
            liveController.TakeSnap();
            systemInfoText.text = "Save Picture OK!";

            StopCoroutine("HideSystemInfo");
            StartCoroutine(HideSystemInfo());
        }



        private IEnumerator HideSystemInfo()
        {
            yield return new WaitForSeconds(5);
            systemInfoText.text = "";
        }

        private void OnFullScreen(GameObject go)
        {
            liveUI.OnFullScreen();
        }

        private void OnShowHidePreview(GameObject go)
        {
            liveUI.OnShowHidePreview();
        }

        private void OnAlphaSliderChange(float value)
        {
            LiveParam.Alpha = value;
        }

        private void OnSoundSliderChange(float value)
        {
            LiveParam.SoundVolume = value;
        }

        private void OnFrameOffsetInput(string value)
        {
            float frame = -9999;
            float.TryParse(value, out frame);

            if (frame != -9999)
            {
                LiveParam.SyncDelayTime = frame;
            }

        }

        private void OnAntiShakeBeforeChange(string value)
        {
            float frame = -9999;
            float.TryParse(value, out frame);

            if (frame != -9999)
            {
                LiveParam.AntiShakeBeforeTime = frame;
            }

        }
        private void OnAntiShakeAfterChange(string value)
        {
            float frame = -9999;
            float.TryParse(value, out frame);

            if (frame != -9999)
            {
                LiveParam.AntiShakeAfterTime = frame;
            }

        }

        //需要加一个获取IsRecording状态的方法
        public bool Recording
        {
            get
            {
                return isRecoding;
            }
            set
            {
                isRecoding = value;
            }
        }

        private void OpenAlbum(GameObject obj)
        {
            liveUI.ShowAlbumUI();
            buttonOpenAlbum.gameObject.SetActive(false);
            buttonCloseAlbum.gameObject.SetActive(true);
        }

        private void CloseAlbum(GameObject obj)
        {
            liveUI.HidingAlbumUI();
            buttonOpenAlbum.gameObject.SetActive(true);
            buttonCloseAlbum.gameObject.SetActive(false);
        }


#endif
    }
}