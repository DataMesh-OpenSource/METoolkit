using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DataMesh.AR.UI
{

    public enum TransitDelayType
    {
        NoDelay,
        WaitForTime,
        WaitForCall
    }

    public class TransitObject : MonoBehaviour, IFinishObject
    {

        private Action<IFinishObject> _cbFinish;
        public Action<IFinishObject> cbFinish
        {
            get
            {
                return _cbFinish;
            }
            set
            {
                _cbFinish = value;
            }
        }


        [HideInInspector]
        public bool inTrans = false;
        private bool _inTransSelf = false;

        [HideInInspector]
        public bool isTransIn;

        [HideInInspector]
        public bool reverse = false;

        [HideInInspector]
        public bool isWaitingContinue = false;

        /// <summary>
        /// 转场分组 
        /// </summary>
        public int transitGroup;

        /// <summary>
        /// 转出时是否需要隐藏物体 
        /// </summary>
        public bool needHideWhenTransitOut = true;

        /// <summary>
        /// 转入时是否需要显示物体 
        /// </summary>
        public bool needShowWhenTransitIn = true;

        /// <summary>
        /// 是否需要遍历子元件并转场 
        /// </summary>
        public bool needTransitChild = false;

        /// <summary>
        /// 转场前的延迟类型。注意，延迟启动依赖于当前转场本身，延迟后实际启动时并不会驱动子转场，如果子转场设置为调用延迟方式，子转场必须自行处理延迟后的启动，否则可能令转场卡死！
        /// </summary>
        public TransitDelayType delayType = TransitDelayType.NoDelay;

        /// <summary>
        /// 延迟转场的时间。如果delayType设置为WaitForTime才有效 
        /// </summary>
        public float delayTime = 0;

        /// <summary>
        /// 所能驱动的对象列表
        /// </summary>
        protected List<GameObject> targetObjectList = new List<GameObject>();

        //protected List<IFinishObject> transitChildList = new List<IFinishObject>();

        /// <summary>
        /// 是否需要再转场前，重置状态
        /// </summary>
        public bool needResetBeforeTransit = true;

        private FinishObjectController foc = null;

        private int timerKey;

        /// <summary>
        /// 检查是否busy。通常用于转场时的限制。子类可重写。
        /// </summary>
        /// <returns>
        /// The busy.
        /// </returns>
        public virtual bool isBusy()
        {
            return inTrans;
        }


        protected virtual void RetriveTargetList()
        {
        }

        /// <summary>
        /// 调度转场，需设置是否延迟启动
        /// </summary>
        /// <param name='isIn'>
        /// 转入还是转出
        /// </param>
        /// <param name='_cb'>
        /// 转场完成的回调
        /// </param>
        public void transit(bool isIn, Action cb)
        {
            if (reverse)
            {
                isIn = !isIn;
            }

            isTransIn = isIn;
            inTrans = true;
            _inTransSelf = true;

            if (foc != null)
            {
                foc.Terminate();
            }

            // 设置完成回调 
            foc = FinishObjectUtil.getInstance().createFinishObjectController();
            foc.AddObject(this);
            foc.cbFinish += dealTransitOver;
            foc.cbFinish += cb;

            // 驱动子转场 
            int count = 0;
            // 如果有子内容需要转场 
            if (needTransitChild)
            {
                TransitObject[] list = GetComponentsInChildren<TransitObject>(true);
                //Debug.Log("Find " + list.Length + " child");
                for (int i = 0; i < list.Length; i++)
                { 
                    TransitObject tobj = list[i];

                    if (tobj != null && tobj != this && tobj.transitGroup == this.transitGroup)
                    {
                        if (!tobj.needShowWhenTransitIn && !tobj.gameObject.activeSelf)
                            continue;

                        /*
                        if (tobj.isBusy())
                        {
                            Debug.Log("[" + tobj + "] busy!");
                            continue;
                        }
                        */

                        tobj.transit(isIn, null);

                        foc.AddObject(tobj);

                        count++;
                    }
                }
                //Debug.Log("Transit " + count + " sub object");
            }

            foc.Start();


            switch (delayType)
            {
                case TransitDelayType.WaitForTime:      // 等待时间 
                    if (timerKey > 0)
                        TimerManager.Instance.RemoveTimer(timerKey);
                    isWaitingContinue = true;
                    timerKey = TimerManager.Instance.RegisterTimer(DelayByTime, delayTime, 1);
                    //Debug.Log("Regist timer " + timerKey);
                    break;
                case TransitDelayType.WaitForCall:      // 等待调用 
                    isWaitingContinue = true;
                    break;
                default:        // 无延迟 
                    DealShow();
                    transitContent(isIn);
                    break;
            }

        }

        /*
        private void transitChildOver(IFinishObject obj)
        {
            transitChildList.Remove(obj);

            chechTransitOver();
        }
        */

        private void DealShow()
        {
            RetriveTargetList();

            //Debug.Log("Deal Show!~ " + gameObject.name);


            // 如果是进场，则先设置对象为显示
            if (needShowWhenTransitIn && isTransIn)
            {
                gameObject.SetActive(true);
                for (int i = 0; i < targetObjectList.Count; i++)
                {
                    targetObjectList[i].SetActive(true);
                }
            }

        }

        private void DealHide()
        {
            RetriveTargetList();

            // 如果是进场，则先设置对象为显示
            if (needHideWhenTransitOut && !isTransIn)
            {
                for (int i = 0; i < targetObjectList.Count; i++)
                {
                    targetObjectList[i].SetActive(false);
                }
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 专为延迟启动服务的方法，运行后才会真正启动转场
        /// </summary>
        public void continueTransit()
        {
            if (isWaitingContinue)
            {
                DealShow();
                transitContent(isTransIn);
                isWaitingContinue = false;
            }

            /*****
             * 延迟处理只处理自身，不处理子转场！
            if (needTransitChild)
            {
                TransitObject[] list = GetComponentsInChildren<TransitObject>(true);
                for (int i = 0;i < list.Length;i ++)
                {
                    TransitObject tobj = list[i];
                    if (tobj != this && tobj.transitGroup == this.transitGroup)
                    {
                        tobj.continueTransit();
                    }
                }
            }
            */
        }

        public void DelayByTime(Hashtable param)
        {
            continueTransit();
            timerKey = 0;
        }

        /// <summary>
        /// 主要由子类实现的方法
        /// </summary>
        /// <param name='isIn'>
        /// Is in.
        /// </param> 
        protected virtual void transitContent(bool isIn)
        {
            transitOver();
        }

        /// <summary>
        /// 转场完成的处理方法。
        /// 注意，此方法仅仅标记自身转场完成。因为有可能存在子转场 
        /// 为了判断结束的逻辑能正确嵌套，这里需要等到桢末尾才执行 
        /// </summary>
        protected void transitOver()
        {
            CoroutineUtil.Run(transitOverContent());
        }

        protected IEnumerator transitOverContent()
        {
            yield return new WaitForEndOfFrame();

            inTrans = false;

            if (cbFinish != null)
            {
                cbFinish(this);
            }

        }

        /*
        /// <summary>
        /// 检查是否转场完毕。为确保处理顺序正常，此方法需要延迟处理 
        /// </summary>
        private void chechTransitOver()
        {
            TimerManager.GetInstance().RunLater(lateCheckTransitOver, null);
        }

        private void lateCheckTransitOver(Hashtable param)
        {
            if (inTrans)
            {
                //LogManager.Log("Check Transit Over!! [" + this + "]");
                if (!_inTransSelf && transitChildList.Count == 0)
                {
                    inTrans = false;

                    //LogManager.Log("[" + this + "] Transit Over now");

                    dealTransitOver();
                }
                else
                {
                    //LogManager.Log("Transit not over, list="+ transitChildList.Count);
                }
            }
        }
        */

        /// <summary>
        /// 转场完成后的各种处理，包括触发回调等。 
        /// </summary>
        private void dealTransitOver()
        {
            DealHide();

            foc = null;
        }


        /*
        protected virtual void LateUpdate()
        {

            // 判定是否真的转场完毕了。这里会检查是否还有子转场未完成 
            if (inTrans)
            {
                ////LogManager.Log("Check Transit Over!! [" + this + "]");
                if (!_inTransSelf && transitChildList.Count == 0)
                {
                    inTrans = false;

                    ////LogManager.Log("[" + this + "] Transit Over now");

                    dealTransitOver();
                }
            }
        }
        */

        public static void StartTransit(GameObject obj, int group, bool isIn, bool needReset, Action cbFinish)
        {
            bool hasTransit = false;
            TransitObject[] tObjs = obj.GetComponents<TransitObject>();
            //Debug.Log("【" + obj.name + "】len=" + tObjs.Length);
            for (int i = 0; i < tObjs.Length; i++)
            {
                TransitObject tObj = tObjs[i];
                if (group < 0 || tObj.transitGroup == group)
                {
                    tObj.needResetBeforeTransit = needReset;
                    tObj.transit(isIn, cbFinish);

                    hasTransit = true;
                }
            }
            if (!hasTransit)
                if (cbFinish != null)
                    cbFinish();
        }

        public static void StartTransit(GameObject obj, int group, bool isIn, Action cbFinish)
        {
            StartTransit(obj, group, isIn, true, cbFinish);
        }

        public static void StartTransit(GameObject obj, int group, bool isIn)
        {
            StartTransit(obj, group, isIn, null);
        }
    }

}