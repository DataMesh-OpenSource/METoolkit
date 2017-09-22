using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DataMesh.AR.Interactive;
using DataMesh.AR.Utility;

namespace DataMesh.AR.UI
{

    /// <summary>
    /// 一个方形按钮
    /// </summary>
    public class BlockButton : MonoBehaviour
    {
        public static float[] COORDINATE_X = { 0.5f, -0.5f, -0.5f, 0.5f };
        public static float[] COORDINATE_Y = { 0.5f, 0.5f, -0.5f, -0.5f };

        /// <summary>
        /// 按钮的ID
        /// </summary>
        [HideInInspector]
        public string buttonId;

        /// <summary>
        /// 按钮的名称
        /// </summary>
        [HideInInspector]
        public string buttonName;

        /// <summary>
        /// 按钮在当前面板内所属的象限位置
        /// </summary>
        [HideInInspector]
        public QUADRANT Quadrant = QUADRANT.First;

        /// <summary>
        /// 按钮中心点的位置（基于当前面板）
        /// </summary>
        [HideInInspector]
        public Vector2 CenterPosition;

        /// <summary>
        /// 所属的面板
        /// </summary>
        [HideInInspector]
        public BlockPanel currentPanel;

        /// <summary>
        /// 链接的下一级面板，如果没有则为null
        /// </summary>
        [HideInInspector]
        public BlockPanel nextPanel;

        /// <summary>
        /// 按钮是否可以点击
        /// </summary>
        [HideInInspector]
        public bool canClick;


        /// <summary>
        /// 实际的button显示对象
        /// </summary>
        public Button uiButton;

        /// <summary>
        /// 实际的Image显示对象
        /// </summary>
        public Image uiImage;

        /// <summary>
        /// 用作点击的碰撞器
        /// </summary>
        public Collider hitCollider;

        /// <summary>
        /// 按钮正常显示和消失所使用的转场对象，在prefab里指定好，用于区分其他的转场
        /// </summary>
        public TweenerGroupTransitObject NormalTransit;

        /// <summary>
        /// 按钮点击后消失的转场对象，目前只有转出有效，转入没有用到。在prefab里指定好，用于区分其他的转场
        /// </summary>
        public TweenerGroupTransitObject ClickTransit;



        /// <summary>
        /// 按钮出现时所使用的tween对象，在prefab里直接指定比较方便
        /// </summary>
        public TweenRotation tweenRotationForShow;
        /// <summary>
        /// 焦点在按钮上时的方所效果的tween对象，在prefab里直接指定比较方便
        /// </summary>
        public TweenScale tweenScaleForPointEnter;

        [HideInInspector]
        public System.Action<BlockButton> cbClick;



        /// <summary>
        /// 所属的Menu对象
        /// </summary>
        private BlockMenu menu = null;

        private RectTransform transBlock;
        private RectTransform transButton;
        private Text btnText;

        private string txt = null;

        private int transitOutGroup = 0;

        private int pointEnterTimerKey = 0;

        private bool hasShow = false;
        private bool hiding = false;

        //private bool isHover = false;
        private MultiInputManager gm;


        public void SetText(string s)
        {
            if (btnText != null)
            {
                btnText.text = s;
            }
            else
            {
                txt = s;
            }
        }


        public bool GetHasShow()
        {
            return hasShow;
        }

        public bool IsHiding()
        {
            return hiding;
        }

        /// <summary>
        /// 初始化按钮
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="q"></param>
        public void Init(BlockMenu menu, BlockPanel panel, QUADRANT q, BlockButtonData buttonData)
        {
            this.menu = menu;
            currentPanel = panel;
            Quadrant = q;

            buttonId = buttonData.buttonId;
            buttonName = buttonData.buttonName;

            canClick = buttonData.canClick;

            transBlock = transform as RectTransform;
            transBlock.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, menu.ButtonWidth);
            transBlock.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, menu.ButtonHeight);

            transButton = uiButton.transform as RectTransform;
            transButton.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, menu.ButtonWidth);
            transButton.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, menu.ButtonHeight);


            CenterPosition = new Vector2(
            (menu.ButtonWidth + menu.ButtonInterval) * COORDINATE_X[(int)q],
            (menu.ButtonHeight + menu.ButtonInterval) * COORDINATE_Y[(int)q]);


            //uiButton.onClick.AddListener(OnClick);

            // 设置按钮标题  
            SetText(buttonData.buttonName);

            // 设置颜色 
            SetButtonColor(buttonData.buttonColor);

            // 设置图片 
            SetButtonPic("UI/Texture/" + buttonData.buttonPic);
        }

        /// <summary>
        /// 设置按钮图片，如果没有图片则设置为空白
        /// </summary>
        /// <param name="picPath"></param>
        public void SetButtonPic(string picPath)
        {
            //Debug.Log("pic:" + picPath);
            Sprite sp = null;
            if (picPath != null)
            {
                sp = Resources.Load<Sprite>(picPath);
            }
            if (sp != null)
            {
                uiImage.sprite = sp;
                uiImage.gameObject.SetActive(true);
            }
            else
            {
                uiImage.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 设置按钮颜色。此颜色会用作按钮的普通显示颜色，高亮、按下等颜色则会根据主颜色自动生成
        /// </summary>
        /// <param name="color"></param>
        public void SetButtonColor(Color color)
        {
            Color normalColor = color;
            float h, s, v;
            Color.RGBToHSV(normalColor, out h, out s, out v);

            float v1 = v * 1.3f;
            Color highlightColor = Color.HSVToRGB(h, s, v1);

            float v2 = v * 0.8f;
            Color pressColor = Color.HSVToRGB(h, s, v2);

            //Color disableColor = Color.HSVToRGB(h, 0, v);
            Color disableColor = new Color(0.5f, 0.5f, 0.5f);

            ColorBlock btnColors = new ColorBlock();
            btnColors.normalColor = normalColor;
            btnColors.highlightedColor = highlightColor;
            btnColors.pressedColor = pressColor;
            btnColors.disabledColor = disableColor;
            btnColors.fadeDuration = uiButton.colors.fadeDuration;
            btnColors.colorMultiplier = uiButton.colors.colorMultiplier;

            uiButton.colors = btnColors;

        }

        /// <summary>
        /// 按钮每次显示之前，都需要初始化的一些内容
        /// </summary>
        public void InitToShow()
        {
            // 设置离开方式为普通 
            transitOutGroup = 0;

            ClickTransit.resetTransit(false);
            NormalTransit.resetTransit(true);

            if (tweenScaleForPointEnter.direction != TweenDirection.Forward)
            {
                tweenScaleForPointEnter.Toggle();
            }
            tweenScaleForPointEnter.ResetToBeginning();
            tweenScaleForPointEnter.enabled = false;
        }

        /// 根据输入的按钮出现顺序，设置一个按钮的基准点和实际位置
        /// </summary>
        /// <param name="qFrom">从哪个象限出发</param>
        public void SetShowDirection(QUADRANT qFrom)
        {
            QUADRANT qTo = this.Quadrant;

            float dx = COORDINATE_X[(int)qFrom] - COORDINATE_X[(int)qTo];
            float dy = COORDINATE_Y[(int)qFrom] - COORDINATE_Y[(int)qTo];

            // 设置重心 
            Vector2 pivot;
            pivot.x = ((dx * 0.5f) + 0.5f);
            pivot.y = ((dy * 0.5f) + 0.5f);
            transBlock.pivot = pivot;

            // 设置位置 
            Vector3 pos;
            pos.x = CenterPosition.x + (pivot.x - 0.5f) * menu.ButtonWidth;
            pos.y = CenterPosition.y + (pivot.y - 0.5f) * menu.ButtonHeight;
            pos.z = 0;
            transBlock.localPosition = pos;

            // 设置转场 
            tweenRotationForShow.from.x = dy * 90;
            tweenRotationForShow.from.y = dx * 90;


            //Debug.Log(" " + qFrom + " -> " + Quadrant);
            //Debug.Log("center=" + CenterPosition);
            //Debug.Log("pivot=" + pivot);
            //Debug.Log("pos=" + pos);

        }


        // Use this for initialization
        void Awake()
        {

            btnText = uiButton.GetComponentInChildren<Text>();
            if (txt != null)
            {
                btnText.text = txt;
                txt = null;
            }

        }

        void Start()
        {
            gm = MultiInputManager.Instance;
        }

        /// <summary>
        /// 焦点进入时的处理
        /// </summary>
        /// <param name="eventData"></param>
        public void OnGazeEnterObject()
        {
            if (!currentPanel.GetHasShow())
                return;

            // 播放动画 
            tweenScaleForPointEnter.PlayForward();

            // 准备打开子面板
            if (nextPanel != null)
            {
                if (pointEnterTimerKey <= 0)
                {
                    pointEnterTimerKey = TimerManager.Instance.RegisterTimer(TimerShowNextPanel, 0.2f, 1);
                }
            }

            // 检查同级其他按钮，如果有面板，则关闭 
            BlockPanel p = this.currentPanel;
            p.HideOthersSubPanel(this.Quadrant);

            if (nextPanel != null)
                nextPanel.SetKeepShow(true);

            currentPanel.SetKeepShow(true);

            if (canClick)
            {
                menu.uiCanBeClick = true;
            }

            // 这里屏蔽GazeGestureManager
            //GazeGestureManager.Instance.StopCapture();
        }

        private void TimerShowNextPanel(Hashtable param)
        {
            nextPanel.Show();
            nextPanel.cbHide += OnSubPanelClose;
            pointEnterTimerKey = 0;

            // 如果按钮不可点，就变更颜色 
            if (!canClick)
            {
                uiButton.interactable = false;
            }

        }

        private void OnSubPanelClose()
        {
            // 如果按钮不可点，就变更颜色 
            if (!canClick)
            {
                uiButton.interactable = true;
            }
            nextPanel.cbHide -= OnSubPanelClose;
        }

        /// <summary>
        /// 焦点离开时的处理
        /// </summary>
        /// <param name="eventData"></param>
        public void OnGazeExitObject()
        {
            if (pointEnterTimerKey > 0)
            {
                TimerManager.Instance.RemoveTimer(pointEnterTimerKey);
                pointEnterTimerKey = 0;
            }

            if (!currentPanel.GetHasShow())
                return;

            tweenScaleForPointEnter.PlayReverse();

            menu.NeedHide();

            if (nextPanel != null)
                nextPanel.SetKeepShow(false);

            if (canClick)
            {
                menu.uiCanBeClick = false;
            }


            /*
            if (nextPanel != null)
            {
                nextPanel.Hide();
            }
            */

            //GazeGestureManager.Instance.StartCapture();

        }

        /// <summary>
        /// 点击按钮的处理
        /// </summary>
        public void OnTapOnObject()
        {
            if (!currentPanel.GetHasShow())
                return;

            if (!canClick)
                return;

            // 如果还在转场，则不能点 
            if (NormalTransit.isBusy())
                return;

            //Debug.Log("click");

            // 点击后，设置离开方式为点击离开 
            transitOutGroup = 1;

            menu.clickedButtonId = this.buttonId;

            menu.NeedHideImmediately();


            //TimerManager.Instance.RegisterTimer(DoClick, 0.5f, 1);
        }

        /*
        private void DoClick(Hashtable param)
        {

            if (cbClick != null)
            {
                cbClick(this);
            }
        }
        */

        /// <summary>
        /// 给按钮添加下一级面板
        /// </summary>
        /// <param name="next"></param>
        public void AddNextPanel(BlockPanel next)
        {
            this.nextPanel = next;
            next.parentButton = this;

            Vector3 pos = Vector3.zero;
            pos.x = currentPanel.centerPosition.x + CenterPosition.x + COORDINATE_X[(int)Quadrant] * (menu.ButtonWidth + menu.ButtonInterval);
            pos.y = currentPanel.centerPosition.y + CenterPosition.y + COORDINATE_Y[(int)Quadrant] * (menu.ButtonHeight + menu.ButtonInterval);

            next.centerPosition = pos;
        }

        /// <summary>
        /// 显示一个按钮，播放出现动画
        /// </summary>
        /// <param name="delay">显示延迟，单位为秒</param>
        public void Show(float delay)
        {
            NormalTransit.resetTransit(true);
            NormalTransit.delayType = TransitDelayType.WaitForTime;
            NormalTransit.delayTime = delay;

            TransitObject.StartTransit(this.gameObject, 0, true);

            hasShow = true;
            hiding = false;

            //Debug.Log("Begin show!");
        }

        // 隐藏一个按钮，播放关闭动画
        public void Hide()
        {
            if (pointEnterTimerKey > 0)
            {
                TimerManager.Instance.RemoveTimer(pointEnterTimerKey);
                pointEnterTimerKey = 0;
            }
            hiding = true;
            //Debug.Log("Begin Hide!");
            TransitObject.StartTransit(this.gameObject, transitOutGroup, false, HideFinish);
        }

        private void HideFinish()
        {
            //Debug.Log("=-============ Hide!");
            hasShow = false;
            hiding = false;
        }

        void Update()
        {
            /*
            if (isHover)
            {
                if (gm.hitCollider != this.hitCollider)
                {
                    isHover = false;
                    //Debug.Log("exit!");
                    OnPointerExit();
                }
            }
            else
            {
                if (gm.hitCollider == this.hitCollider)
                {
                    isHover = true;
                    //Debug.Log("Enter");
                    OnPointerEnter();
                }
            }
            */
        }
    }
}