using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DataMesh.AR.Interactive;

namespace DataMesh.AR.UI
{

    public class LoadingUI : MonoBehaviour
    {
        public static Color buttonColorNormal = new Color(8f / 255f, 154f / 255f, 228f / 255f);
        public static Color buttonColorHover = new Color(65f / 255f, 188f / 255f, 251f / 255f);
        public static Color buttonColorPressed = new Color(17f / 255f, 127f / 255f, 183f / 255f);


        public GameObject loadingArea;
        public Text loadingText;

        public Image buttonImg;
        public Text buttonText;
        public Text infoText;


        private System.Action cbClick;
        private System.Action cbClickBlank;

        private bool isLoading = false;

        private bool hovering = false;
        private bool pressing = false;
        private float pressTime;

        private MultiInputManager inputManager;

        public void Init()
        {
            inputManager = MultiInputManager.Instance;
            loadingArea.SetActive(false);
            buttonImg.gameObject.SetActive(false);

            gameObject.SetActive(false);
        }

        public void ShowLoading(string text)
        {
            loadingArea.SetActive(true);
            buttonImg.gameObject.SetActive(false);

            loadingText.text = text;

            Show();
        }

        public void ShowButton(string text, System.Action cb, string info = null, System.Action cbBlank = null)
        {
            loadingArea.SetActive(false);
            buttonImg.gameObject.SetActive(true);

            buttonText.text = text;
            cbClick = cb;
            cbClickBlank = cbBlank;

            if (info == null)
            {
                infoText.text = "";
            }
            else
            {
                infoText.text = info;
            }

            SetButtonColorNormal();

            isLoading = false;

            Show();
        }

        /// <summary>
        /// 单纯的显示出来，保留之前的状态
        /// </summary>
        public void Show()
        {
            if (!isLoading && cbClickBlank != null)
            {
                inputManager.cbTap += OnTap;
            }
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            inputManager.cbTap -= OnTap;
            gameObject.SetActive(false);
        }

        private void SetButtonColorNormal()
        {
            pressing = false;
            RefreshButtonColor();
        }

        void OnGazeEnterObject()
        {
            hovering = true;
            pressing = false;
            RefreshButtonColor();
        }

        void OnGazeExitObject()
        {
            hovering = false;
            pressing = false;
            RefreshButtonColor();
        }

        private void RefreshButtonColor()
        {
            if (pressing)
            {
                buttonImg.color = buttonColorPressed;
            }
            else if (hovering)
            {
                buttonImg.color = buttonColorHover;
            }
            else
            {
                buttonImg.color = buttonColorNormal;
            }
        }

        void OnTapOnObject()
        {
            pressing = true;
            pressTime = Time.time;
            RefreshButtonColor();

            if (cbClick != null)
                cbClick();
        }

        void Update()
        {
            float curTime = Time.time;
            if (pressing)
            {
                if (curTime - pressTime > 0.2f)
                {
                    SetButtonColorNormal();
                }
            }
        }

        private void OnTap(int count)
        {
            if (cbClickBlank != null)
            {
                if (inputManager.FocusedObject == null)
                {
                    cbClickBlank();
                }
            }
        }
    }
}