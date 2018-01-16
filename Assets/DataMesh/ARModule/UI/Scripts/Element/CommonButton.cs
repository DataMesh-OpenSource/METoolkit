using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DataMesh.AR.Interactive;

namespace DataMesh.AR.UI
{

    public class CommonButton : MonoBehaviour
    {
        public enum TransitionType
        {
            None,
            Color,
            Image
        }

        public TransitionType transitionType = TransitionType.Color;

        public Image targetImage;

        public Color normalColor;
        public Color hoverColor;
        public Color clickColor;

        public Image hoverImage;
        public Image clickImage;

        public System.Action<CommonButton> callbackClick;

        /// <summary>
        /// 作为备注的内容
        /// </summary>
        [HideInInspector]
        public object param;

        protected MultiInputManager inputManager;

        private bool isHover = false;
        private bool isPress = false;

        private Text text;
        private string buttonName;


        public string ButtonName
        {
            get { return buttonName; }
            set
            {
                buttonName = value;
                if (text != null)
                {
                    text.text = buttonName;
                }
            }
        }

        public void SetSize(int w, int h)
        {
            RectTransform trans = transform as RectTransform;
            trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
            trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);

            BoxCollider buttonCollider = GetComponent<BoxCollider>();
            buttonCollider.size = new Vector3(w, h, 0);

        }

        private void Awake()
        {
            Transform t = transform.Find("Text");
            if (t != null)
            {
                text = t.GetComponent<Text>();
                if (buttonName != null)
                    text.text = buttonName;
            }
        }

        private void Start()
        {
            switch (transitionType)
            {
                case TransitionType.Color:
                    if (targetImage != null)
                    {
                        targetImage.color = normalColor;
                    }
                    break;
                case TransitionType.Image:
                    break;
            }
        }

        private void OnTapOnObject()
        {
            switch (transitionType)
            {
                case TransitionType.Color:
                    if (targetImage != null)
                    {
                        targetImage.color = clickColor;
                    }
                    break;
                case TransitionType.Image:
                    if (clickImage != null)
                        clickImage.gameObject.SetActive(true);
                    break;
            }

            DealClick();

            isPress = true;

            if (callbackClick != null)
                callbackClick(this);

            TimerManager.Instance.RegisterTimer(ClickOver, 0.1f, 1);
        }

        private void ClickOver(Hashtable hashtable)
        {
            isPress = false;

            switch (transitionType)
            {
                case TransitionType.Color:
                    if (targetImage != null)
                    {
                        targetImage.color = isHover ? hoverColor : normalColor;
                    }
                    break;
                case TransitionType.Image:
                    if (clickImage != null)
                        clickImage.gameObject.SetActive(false);
                    if (isHover)
                        hoverImage.gameObject.SetActive(true);
                    break;
            }
        }


        private void OnGazeEnterObject()
        {
            switch (transitionType)
            {
                case TransitionType.Color:
                    if (targetImage != null)
                    {
                        targetImage.color = hoverColor;
                    }
                    break;
                case TransitionType.Image:
                    if (hoverImage != null)
                        hoverImage.gameObject.SetActive(true);
                    break;
            }

            isHover = true;

            DealEnter();
        }

        private void OnGazeExitObject()
        {
            switch (transitionType)
            {
                case TransitionType.Color:
                    if (targetImage != null)
                    {
                        targetImage.color = normalColor;
                    }
                    break;
                case TransitionType.Image:
                    if (hoverImage != null)
                        hoverImage.gameObject.SetActive(false);
                    break;
            }

            isHover = false;

            DealExit();
        }

        /// <summary>
        /// 子类可实现自己的点击处理方法 
        /// </summary>
        protected virtual void DealClick()
        {

        }

        /// <summary>
        /// 游标移入时，子类可实现自己的处理方法
        /// </summary>
        protected virtual void DealEnter()
        {

        }

        /// <summary>
        /// 游标移出时，子类可实现自己的处理方法
        /// </summary>
        protected virtual void DealExit()
        {

        }

    }
}