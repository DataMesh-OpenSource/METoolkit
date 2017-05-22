using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DataMesh.AR.UI
{

    /// <summary>
    /// 一层按钮的面板，可容纳4个按钮，分别在4个象限的位置
    /// </summary>
    public class BlockPanel : MonoBehaviour
    {


        /// <summary>
        /// 面板中包含的4个按钮
        /// </summary>
        private BlockButton[] buttons = new BlockButton[4];

        /// <summary>
        /// 面板由哪个按钮点出。如果是根面板，则为空
        /// </summary>
        [HideInInspector]
        public BlockButton parentButton = null;

        /// <summary>
        /// 所属的Menu对象
        /// </summary>
        [HideInInspector]
        public BlockMenu menu = null;

        private Vector3 _centerPosition;
        [HideInInspector]
        public Vector3 centerPosition
        {
            get
            {
                return _centerPosition;
            }

            set
            {
                _centerPosition = value;
                transform.localPosition = value;
            }
        }


        private int[] showOrder;


        private bool hasShow = false;
        private bool keepShow = false;

        private Transform trans;

        /// <summary>
        /// panel关闭时的回调
        /// </summary>
        public System.Action cbHide;

        /// <summary>
        /// 按象限获取按钮
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public BlockButton GetButton(QUADRANT q)
        {
            return buttons[(int)q];
        }

        /// <summary>
        /// 设置一个指定位置的按钮
        /// </summary>
        /// <param name="button"></param>
        /// <param name="q"></param>
        public void SetButton(BlockButton button, QUADRANT q)
        {
            buttons[(int)q] = button;
        }

        /*
        /// <summary>
        /// 根据给定象限，计算按钮中心点位置
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public Vector2 GetCenterByQuadrant(QUADRANT q)
        {
            Vector2 vt = new Vector2();
            switch (q)
            {
                case QUADRANT.First:
                    vt.x = (ButtonWidth + ButtonInterval) / 2;
                    vt.y = (ButtonHeight + ButtonInterval) / 2;
                    break;
                case QUADRANT.Second:
                    vt.x = -(ButtonWidth + ButtonInterval) / 2;
                    vt.y = (ButtonHeight + ButtonInterval) / 2;
                    break;
                case QUADRANT.Thrid:
                    vt.x = -(ButtonWidth + ButtonInterval) / 2;
                    vt.y = -(ButtonHeight + ButtonInterval) / 2;
                    break;
                case QUADRANT.Fourth:
                    vt.x = (ButtonWidth + ButtonInterval) / 2;
                    vt.y = -(ButtonHeight + ButtonInterval) / 2;
                    break;
            }

            return vt;
        }
        */

        /// <summary>

        public void ResetButton()
        {
            for (int i = 0; i < 4; i++)
            {
                BlockButton button = buttons[i];
                if (button == null)
                    continue;

                TweenerGroupTransitObject tObj = button.GetComponent<TweenerGroupTransitObject>();
                tObj.resetTransit(true);
            }
        }

        /// <summary>
        /// 显示一个panel
        /// </summary>
        public void Show()
        {
            if (hasShow)
                return;

            showOrder = GetShowOrder();

            float delay = 0;
            for (int i = 0; i < 4; i++)
            {
                int index = showOrder[i];
                BlockButton button = buttons[index];
                if (button == null)
                {
                    continue;
                }

                int from = 0;
                if (i == 0)
                {
                    // 如果是第一个格子，则需要人为指定一个出现方向 
                    from = 3 - index;
                }
                else if (i == 1 || i == 2)
                {
                    from = showOrder[0];
                }
                else
                {
                    from = showOrder[1];
                }


                // 设定按钮出现方向 
                button.SetShowDirection((QUADRANT)from);

                button.InitToShow();

                button.Show(delay);

                delay += 0.12f;


            }

            hasShow = true;
            hideTimerStartTime = Time.time;

            //Debug.Log("panel show:" + hideTimerStartTime);

        }

        /// <summary>
        /// 设置panel为需要关闭。会同时设置所有的子面板
        /// </summary>
        public void NeedHide()
        {
            SetNeedHideWithChild(true, false);
        }

        /// <summary>
        /// 设置panel为需要关闭，并且立刻关闭。会同时设置所有自面板
        /// </summary>
        public void HideImmediately()
        {
            SetNeedHideWithChild(true, true);
        }

        /// <summary>
        /// 设置面板为持续显示状态
        /// </summary>
        /// <param name="b"></param>
        public void SetKeepShow(bool b)
        {
            SetKeepShowWithParent(b);
        }

        private void SetNeedHideWithChild(bool b, bool immediately)
        {
            keepShow = false;
            if (immediately)
            {
                hideTimerStartTime = -1000;
                //Debug.Log("need hide:" + hideTimerStartTime);
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                BlockButton btn = buttons[i];
                if (btn == null)
                    continue;

                if (btn.nextPanel != null)
                {
                    btn.nextPanel.SetNeedHideWithChild(b, immediately);
                }
            }
        }

        private void SetKeepShowWithParent(bool b)
        {
            keepShow = b;
            if (parentButton != null)
            {
                parentButton.currentPanel.SetKeepShowWithParent(b);
            }
        }

        private void _Hide()
        {
            if (!hasShow)
                return;

            //Debug.Log("Panel has hide");


            for (int i = 0; i < 4; i++)
            {
                int index = showOrder[i];

                BlockButton button = buttons[index];
                if (button == null)
                    continue;
                if (button.IsHiding())
                    continue;

                button.Hide();
            }

            //hasShow = false;

        }


        /// <summary>
        /// 关闭除指定位置之外的其他位置上的按钮所包含的自面板
        /// </summary>
        /// <param name="q"></param>
        public void HideOthersSubPanel(QUADRANT q)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (i == (int)q)
                    continue;

                BlockButton otherBtn = buttons[i];
                if (otherBtn == null)
                    continue;

                if (otherBtn.nextPanel != null)
                {
                    otherBtn.nextPanel.HideImmediately();
                }
            }
        }

        /// <summary>
        /// 获取面板生成按钮的顺序。带有一定的随机性 
        /// 注意，这里会获得所有位置的顺序，并不保证每个位置上都有按钮
        /// 尤其注意：对于子面板来说，父节点的位置一定是第一个，但父节点所在位置的按钮一定为空。
        /// </summary>
        /// <returns></returns>
        public int[] GetShowOrder()
        {
            int[] rs = new int[4];
            int parentQuadrant = 0;
            if (parentButton != null)
            {
                parentQuadrant = (int)parentButton.Quadrant;
            }
            else
            {
                // 如果没有父按钮，则随机选择一个位置 
                parentQuadrant = Random.Range(0, 3);
            }

            // 第一个一定是父节点 
            rs[0] = parentQuadrant;

            // 再安排两侧按钮 
            int left = parentQuadrant - 1;
            if (left > 3) left -= 4;
            if (left < 0) left += 4;

            int right = parentQuadrant + 1;
            if (right > 3) right -= 4;
            if (right < 0) right += 4;

            if (Random.Range(0f, 1f) > 0.5f)
            {
                rs[1] = left;
                rs[2] = right;
            }
            else
            {
                rs[1] = right;
                rs[2] = left;
            }

            // 最后是对面 
            int oppo = parentQuadrant + 2;
            if (oppo > 3) oppo -= 4;
            if (oppo < 0) oppo += 4;
            rs[3] = oppo;

            return rs;
        }

        /// <summary>
        /// 获取当前面板是否已经被显示的状态
        /// </summary>
        /// <returns></returns>
        public bool GetHasShow()
        {
            return hasShow;
        }

        private float hideTimerStartTime = -1000;
        private void Update()
        {
            float curTime = Time.time;

            if (hasShow)
            {
                if (keepShow)
                {
                    hideTimerStartTime = curTime;
                    //Debug.Log("keep show:" + hideTimerStartTime);
                }
                else
                {
                    if (curTime - hideTimerStartTime > menu.hideTimerInterval)
                    {
                        //Debug.Log("hide");
                        _Hide();
                    }
                }

                hasShow = false;
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i] == null)
                        continue;
                    if (buttons[i].GetHasShow())
                    {
                        //Debug.Log("has button showed");
                        hasShow = true;
                        break;
                    }

                }

                if (!hasShow)
                {
                    //Debug.Log("All button hide!");

                    if (cbHide != null)
                        cbHide();

                }
            }
        }

    }
}