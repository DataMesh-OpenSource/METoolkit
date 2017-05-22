using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataMesh.AR.UI
{

    public enum QUADRANT
    {
        First = 0,
        Second,
        Thrid,
        Fourth
    }

    /// <summary>
    /// 整个按钮菜单
    /// </summary>
    public class BlockMenu : MonoBehaviour
    {
        [HideInInspector]
        public float ButtonWidth = 100;
        [HideInInspector]
        public float ButtonHeight = 100;

        /// <summary>
        /// menu的名字
        /// </summary>
        [HideInInspector]
        public string menuName;

        /// <summary>
        /// 按钮之间的间隔尺寸
        /// </summary>
        public float ButtonInterval = 10;

        /// <summary>
        /// 视觉焦点移出面板之后，面板关闭的等待时间
        /// </summary>
        public float hideTimerInterval = 2;


        /// <summary>
        /// menu所弹出的父节点 
        /// </summary>
        public Canvas MenuBase;

        /// <summary>
        /// 用来产生block的prefab 
        /// </summary>
        public GameObject BlockButtonPrefab;

        /// <summary>
        /// 作为根存在的最上层面板
        /// </summary>
        [HideInInspector]
        public BlockPanel rootPanel;

        /// <summary>
        /// 面板全部关闭时的回调
        /// </summary>
        [HideInInspector]
        public System.Action cbHide;

        private Dictionary<string, System.Action> clickCallbackDic = new Dictionary<string, System.Action>();

        private Dictionary<string, BlockButton> buttonDic = new Dictionary<string, BlockButton>();

        [HideInInspector]
        public string clickedButtonId;

        /// <summary>
        /// 当前是否可以点击。主要给外部使用，表示当前是不是出于一个可点击的UI上面 
        /// </summary>
        [HideInInspector]
        public bool uiCanBeClick;

        //public TextAsset menuData;

        // Use this for initialization
        void Start()
        {


        }

        private bool isBusy;
        public bool IsBusy()
        {
            return isBusy;
        }

        /// <summary>
        /// 用Data数据对Menu进行初始化
        /// </summary>
        /// <param name="data"></param>
        public void BuildMenu(BlockMenuData data)
        {
            menuName = data.name;

            BlockPanel panel = null;
            panel = CreatePanel(data.rootPanel, null);
            rootPanel = panel;
        }

        /// <summary>
        /// 创建一级菜单
        /// </summary>
        /// <param name="panelData">菜单所使用的数据</param>
        /// <param name="parentButton">打开菜单的按钮，如果为null表示根菜单</param>
        /// <returns></returns>
        private BlockPanel CreatePanel(BlockPanelData panelData, BlockButton parentButton)
        {
            //GameObject panelObj = PrefabUtils.CreateGameObjectToParent(MenuBase.gameObject, panelPrefab);
            GameObject panelObj = new GameObject("Panel");
            panelObj.transform.parent = MenuBase.transform;
            panelObj.transform.localPosition = Vector3.zero;
            panelObj.transform.localScale = Vector3.one;
            BlockPanel panel = panelObj.AddComponent<BlockPanel>();

            panel.menu = this;

            if (parentButton != null)
            {
                parentButton.AddNextPanel(panel);
            }

            for (int i = 0; i < panelData.buttons.Count; i++)
            {
                BlockButtonData buttonData = panelData.buttons[i];
                if (buttonData != null)
                {
                    BlockButton button = CreateButton(panel, (QUADRANT)i, buttonData);

                    if (buttonData.subPanel != null)
                    {
                        BlockPanel subPanel = CreatePanel(buttonData.subPanel, button);
                    }

                }
            }

            return panel;

        }

        /// <summary>
        /// 在指定象限添加一个按钮
        /// </summary>
        /// <param name="button"></param>
        /// <param name="q"></param>
        public BlockButton CreateButton(BlockPanel panel, QUADRANT q, BlockButtonData buttonData)
        {
            GameObject buttonObj = PrefabUtils.CreateGameObjectToParent(panel.gameObject, BlockButtonPrefab);
            BlockButton button = buttonObj.GetComponent<BlockButton>();

            // 初始化按钮 
            button.Init(this, panel, q, buttonData);

            panel.SetButton(button, q);

            // 汇总按钮点击回调
            //button.cbClick += this.OnClickButton;

            buttonObj.SetActive(false);

            // 所有button在menu中留记录，以便修改
            // 这里暂时不判断按钮重名了……原则上，按钮不允许重名，可以在编辑器层面保证 
            buttonDic.Add(button.buttonId, button);

            return button;
        }

        /// <summary>
        /// 通过按钮ID来获取一个按钮，如果没有返回null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BlockButton GetButton(string id)
        {
            if (buttonDic.ContainsKey(id))
            {
                return buttonDic[id];
            }

            return null;
        }

        public bool HasShow()
        {
            return rootPanel.GetHasShow();
        }

        public void Show(Vector3 basePos, Vector3 forward)
        {
            if (rootPanel.GetHasShow())
                return;

            // 把面板放置在当前焦点位置 
            MenuBase.transform.position = basePos;
            MenuBase.transform.forward = forward;

            rootPanel.cbHide += OnHide;
            rootPanel.Show();

            //TransitObject.StartTransit(this.gameObject, 0, true);

        }

        private void OnHide()
        {
            //Debug.Log("Menu hide");

            if (cbHide != null)
                cbHide();

            // 这里处理点击 
            if (clickedButtonId != null)
            {
                OnClickButton(clickedButtonId);
                clickedButtonId = null;
            }


            rootPanel.cbHide -= OnHide;
        }

        public void NeedHide()
        {
            rootPanel.NeedHide();
        }

        public void NeedHideImmediately()
        {
            rootPanel.HideImmediately();
        }

        /// <summary>
        /// 注册一个按钮的回调函数，使用按钮名称做索引
        /// </summary>
        /// <param name="buttonName"></param>
        /// <param name="callback"></param>
        public void RegistButtonClick(string buttonId, System.Action callback)
        {
            if (clickCallbackDic.ContainsKey(buttonId))
            {
                System.Action cb = clickCallbackDic[buttonId];
                cb += callback;
            }
            else
            {
                clickCallbackDic.Add(buttonId, callback);
            }
        }

        /// <summary>
        /// 注销一个按钮的回调函数
        /// </summary>
        /// <param name="buttonName"></param>
        /// <param name="callback"></param>
        public void UnregistButtonClick(string buttonId, System.Action callback)
        {
            if (clickCallbackDic.ContainsKey(buttonId))
            {
                System.Action cb = clickCallbackDic[buttonId];
                cb -= callback;
            }
            else
            {
            }
        }

        /// <summary>
        /// 点击按钮的响应函数 
        /// </summary>
        /// <param name="button"></param>
        private void OnClickButton(string buttonId)
        {
            if (clickCallbackDic.ContainsKey(buttonId))
            {
                System.Action cb = clickCallbackDic[buttonId];
                cb();
            }
        }
    }
}