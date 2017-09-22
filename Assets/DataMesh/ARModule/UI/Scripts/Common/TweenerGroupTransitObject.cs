using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using DataMesh.AR.Utility;

namespace DataMesh.AR.UI
{

    public class TweenerGroupTransitObject : TransitObject
    {
        //    protected Dictionary<int, List<UITweener>> twMap = new Dictionary<int, List<UITweener>>();

        public List<UITweener> transitInTweener = new List<UITweener>();
        public List<UITweener> transitOutTweener = new List<UITweener>();


        private void resetTweenIgnoreTime()
        {
            // 默认清除时间标记 
            for (int i = 0; i < transitInTweener.Count; i++)
            {
                UITweener tw = transitInTweener[i];
                tw.ignoreTimeScale = false;
            }
            for (int i = 0; i < transitOutTweener.Count; i++)
            {
                UITweener tw = transitOutTweener[i];
                tw.ignoreTimeScale = false;
            }
        }

        void Start()
        {
            //synchronizeLocalPosition();


        }

        public void resetTweenDirection(TweenDirection dir)
        {
            for (int i = 0; i < transitInTweener.Count; i++)
            {
                UITweener tw = transitInTweener[i];

                if (tw.direction != dir)
                {
                    tw.Toggle();
                    tw.enabled = false;
                }

            }
            for (int i = 0; i < transitOutTweener.Count; i++)
            {
                UITweener tw = transitOutTweener[i];

                if (tw.direction != dir)
                {
                    tw.Toggle();
                    tw.enabled = false;
                }

            }
            if (needTransitChild)
            {
                TweenerGroupTransitObject[] list = GetComponentsInChildren<TweenerGroupTransitObject>(true);
                for (int i = 0; i < list.Length; i++)
                {
                    TweenerGroupTransitObject tobj = list[i];
                    if (tobj != this && tobj.transitGroup == this.transitGroup)
                    {
                        //tobj.tweenerGroup = this.tweenerGroup;
                        tobj.resetTweenDirection(dir);
                    }
                }
            }
        }

        private void ResetTween(UITweener tw, bool isIn)
        {
            if (tw == null)
                return;
            if ((isIn && tw.direction == TweenDirection.Reverse) || (!isIn && tw.direction == TweenDirection.Forward))
                tw.Toggle();
            tw.ResetToBeginning();
            tw.enabled = false;

        }

        public void resetTransit(bool isIn)
        {
            if (isIn)
            {
                for (int i = 0; i < transitInTweener.Count; i++)
                {
                    UITweener tw = transitInTweener[i];
                    ResetTween(tw, isIn);
                }
            }
            else
            {
                for (int i = 0; i < transitOutTweener.Count; i++)
                {
                    UITweener tw = transitOutTweener[i];
                    ResetTween(tw, isIn);
                }
            }

            if (needTransitChild)
            {
                TweenerGroupTransitObject[] list = GetComponentsInChildren<TweenerGroupTransitObject>(true);
                for (int i = 0; i < list.Length; i++)
                {
                    TweenerGroupTransitObject tobj = list[i];
                    if (tobj != this && tobj.transitGroup == this.transitGroup)
                    {
                        //tobj.tweenerGroup = this.tweenerGroup;
                        tobj.resetTransit(isIn);
                    }
                }
            }
        }


        /// <summary>
        /// 根据物体当前的相对坐标，同步修改所有挂接的TweenPosition的偏移量
        /// </summary>
        public void synchronizeLocalPosition()
        {
            synchronizeLocalPosition(transitInTweener);
            synchronizeLocalPosition(transitOutTweener);
        }

        private void synchronizeLocalPosition(List<UITweener> twList)
        {

            for (int i = 0; i < twList.Count; i++)
            {
                UITweener tw = twList[i];

                if (tw is TweenPosition)
                {
                    TweenPosition tp = tw as TweenPosition;
                    tp.from = new Vector3(
                        tp.from.x * transform.localScale.x + transform.localPosition.x,
                        tp.from.y * transform.localScale.y + transform.localPosition.y,
                        tp.from.z * transform.localScale.z + transform.localPosition.z
                    );
                    tp.to = new Vector3(
                        tp.to.x * transform.localScale.x + transform.localPosition.x,
                        tp.to.y * transform.localScale.y + transform.localPosition.y,
                        tp.to.z * transform.localScale.z + transform.localPosition.z
                        );
                }
                else if (tw is TweenScale)
                {
                    TweenScale ts = tw as TweenScale;
                    ts.from = new Vector3(ts.from.x * transform.localScale.x, ts.from.y * transform.localScale.y, ts.from.z * transform.localScale.z);
                    ts.to = new Vector3(ts.to.x * transform.localScale.x, ts.to.y * transform.localScale.y, ts.to.z * transform.localScale.z);
                }
            }
        }

        protected override void transitContent(bool isIn)
        {
            List<UITweener> twList;
            if (isIn)
                twList = transitInTweener;
            else
                twList = transitOutTweener;

            if (needResetBeforeTransit)
            {
                resetTransit(isIn);
            }

            UITweener longest = null;
            for (int i = 0; i < twList.Count; i++)
            {
                UITweener tw = twList[i];
                if (tw == null)
                    continue;

                if (!isIn && !tw.gameObject.activeInHierarchy)
                {
                    // 如果是出场，但物体已经设置为不可见了，就不再计算它了
                    continue;
                }

                if ((isIn && tw.tweenFactor >= 1) || (!isIn && tw.tweenFactor <= 0))
                {
                    continue;
                }

                if (longest == null)
                {
                    longest = tw;
                }
                else
                {
                    if (longest.duration < tw.duration)
                    {
                        longest = tw;
                    }
                }
            }

            if (longest != null)
            {
                //LogManager.Log("longest=" + longest);
                // 最终转场结束，挂接在最长的转场上
                longest.AddFinishAction(transitOver, true);
            }
            else
            {
                // 这说明没找到任何Tween转场，这时需要立刻结束 
                transitOver();
                return;
            }

            for (int i = 0; i < twList.Count; i++)
            {
                UITweener tw = twList[i];
                if (tw == null)
                    continue;

                if (!isIn && !tw.gameObject.activeSelf)
                {
                    // 如果是出场，但物体已经设置为不可见了，就不再计算它了
                    continue;
                }

                tw.ignoreTimeScale = false;

                //LogManager.Log("play tw" + tw);
                // 播放每一个变换
                tw.Play(isIn);
            }


        }

        /*
        private void preTransitOver()
        {
            TimerManager.GetInstance().RunLater(preTransitOver2, null);
        }
        private void preTransitOver2(Hashtable param)
        {
            transitOver();
        }
    */

        protected override void RetriveTargetList()
        {
            for (int i = 0; i < transitInTweener.Count; i++)
            {
                if (transitInTweener[i] != null)
                {
                    if (!targetObjectList.Contains(transitInTweener[i].gameObject))
                    {
                        targetObjectList.Add(transitInTweener[i].gameObject);
                    }
                }
            }
            for (int i = 0; i < transitOutTweener.Count; i++)
            {
                if (transitOutTweener[i] != null)
                {
                    if (!targetObjectList.Contains(transitOutTweener[i].gameObject))
                    {
                        targetObjectList.Add(transitOutTweener[i].gameObject);
                    }
                }
            }
        }


    }
}