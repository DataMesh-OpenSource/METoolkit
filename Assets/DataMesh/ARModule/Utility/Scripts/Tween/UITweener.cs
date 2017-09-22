
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DataMesh.AR.Utility
{

    public enum TweenDirection
    {
        Reverse = -1,
        Toggle = 0,
        Forward = 1,
    }


    public abstract class UITweener : MonoBehaviour
    {
        public class ActionData
        {
            public System.Action action;
            public bool oneShot;
        }

        /// <summary>
        /// Current tween that triggered the callback function.
        /// </summary>

        static public UITweener current;

        public enum Method
        {
            Linear,
            EaseIn,
            EaseOut,
            EaseInOut,
            BounceIn,
            BounceOut,
        }

        public enum Style
        {
            Once,
            Loop,
            PingPong,
        }



        /// <summary>
        /// Tweening method used.
        /// </summary>
        [HideInInspector]
        public Method method = Method.Linear;

        /// <summary>
        /// Does it play once? Does it loop?
        /// </summary>

        [HideInInspector]
        public Style style = Style.Once;

        /// <summary>
        /// loop times. 0 means forever
        /// </summary>
        [HideInInspector]
        public int loopCount = 0;

        /// <summary>
        /// 指定有几个额外曲线。
        /// 子类可以重写
        /// </summary>
        /// <returns></returns>
        public virtual int GetAdditionalCurveCount()
        {
            return 0;
        }

        /// <summary>
        /// Optional curve to apply to the tween's time factor value.
        /// </summary>
        /// 
        [HideInInspector]
        public AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));
        public List<AnimationCurve> additionalAnimationCurveList = new List<AnimationCurve>();

        /// <summary>
        /// the factor when start
        /// </summary>
        public float startFactor = 0;

        /// <summary>
        /// the factor when start reverse
        /// </summary>
        public float startFactorReverse = 1;

        /// <summary>
        /// Whether the tween will ignore the timescale, making it work while the game is paused.
        /// </summary>
        [HideInInspector]
        public bool ignoreTimeScale = true;

        /// <summary>
        /// How long will the tweener wait before starting the tween?
        /// </summary>

        [HideInInspector]
        public float delay = 0f;

        /// <summary>
        /// How long is the duration of the tween?
        /// </summary>

        [HideInInspector]
        public float duration = 1f;

        /// <summary>
        /// Whether the tweener will use steeper curves for ease in / out style interpolation.
        /// </summary>

        [HideInInspector]
        public bool steeperCurves = false;

        /// <summary>
        /// Used by buttons and tween sequences. Group of '0' means not in a sequence.
        /// </summary>

        [HideInInspector]
        public int tweenGroup = 0;

        private int currentLoopCount = 0;

        /// <summary>
        /// Event delegates called when the animation finishes.
        /// </summary>
        private List<ActionData> onFinished = new List<ActionData>();


        bool mStarted = false;
        float mStartTime = 0f;
        float mDuration = 0f;
        float mAmountPerDelta = 1000f;
        float mFactor = 0f;

        public void AddFinishAction(System.Action action, bool oneShot)
        {
            ActionData data = new ActionData();
            data.action = action;
            data.oneShot = oneShot;
            onFinished.Add(data);
        }

        /// <summary>
        /// Amount advanced per delta time.
        /// </summary>

        public float amountPerDelta
        {
            get
            {
                if (mDuration != duration)
                {
                    mDuration = duration;
                    mAmountPerDelta = Mathf.Abs((duration > 0f) ? 1f / duration : 1000f) * Mathf.Sign(mAmountPerDelta);
                }
                return mAmountPerDelta;
            }
        }

        /// <summary>
        /// Tween factor, 0-1 range.
        /// </summary>

        public float tweenFactor { get { return mFactor; } set { mFactor = Mathf.Clamp01(value); } }

        /// <summary>
        /// Direction that the tween is currently playing in.
        /// </summary>

        public TweenDirection direction { get { return amountPerDelta < 0f ? TweenDirection.Reverse : TweenDirection.Forward; } }

        /// <summary>
        /// This function is called by Unity when you add a component. Automatically set the starting values for convenience.
        /// </summary>

        void Reset()
        {
            if (!mStarted)
            {
                SetStartToCurrentValue();
                SetEndToCurrentValue();
            }
        }

        /// <summary>
        /// Update as soon as it's started so that there is no delay.
        /// </summary>

        protected virtual void Start() { Update(); }

        /// <summary>
        /// Update the tweening factor and call the virtual update function.
        /// </summary>

        protected float delta = 0, time = 0;
        private float getTime()
        {
            if (ignoreTimeScale)
            {
                return Time.unscaledTime;
            }
            return Time.time;
        }
        protected virtual void Update()
        {
            //float delta = ignoreTimeScale ? RealTime.deltaTime : Time.deltaTime;
            //float time = ignoreTimeScale ? RealTime.time : Time.time;

            time = getTime();
            delta = time - curTime;
            curTime = time;

            if (!mStarted)
            {
                mStarted = true;
                mStartTime = time + delay;

                delta = 0;
            }

            if (delta == 0)
                return;

            if (time < mStartTime)
            {
                //Debug.Log("!!!!!!");
                return;
            }

            // Advance the sampling factor
            mFactor += amountPerDelta * delta;

            bool finishOneTurn = false;

            // Loop style simply resets the play factor after it exceeds 1.
            if (style == Style.Loop)
            {
                if (mFactor >= 1f)
                {
                    mFactor -= Mathf.Floor(mFactor);
                    finishOneTurn = true;
                }
            }
            else if (style == Style.PingPong)
            {
                // Ping-pong style reverses the direction
                if (mFactor >= 1f)
                {
                    mFactor = 1f - (mFactor - Mathf.Floor(mFactor));
                    mAmountPerDelta = -mAmountPerDelta;

                    finishOneTurn = true;
                }
                else if (mFactor <= 0f)
                {
                    mFactor = -mFactor;
                    mFactor -= Mathf.Floor(mFactor);
                    mAmountPerDelta = -mAmountPerDelta;

                    finishOneTurn = true;
                }
            }
            else if (style == Style.Once)
            {
                if ((mAmountPerDelta > 0 && mFactor >= 1f) || (mAmountPerDelta < 0 && mFactor <= 0f))
                {
                    mFactor = Mathf.Clamp01(mFactor);
                    finishOneTurn = true;
                }
            }

            //Debug.Log("factor=" + mFactor);
            // Finish one play circle
            if (duration == 0f || finishOneTurn)
            {
                Sample(mFactor, true);

                // trigger event every loop
                if (current == null)
                {
                    UITweener before = current;
                    current = this;

                    if (onFinished != null)
                    {
                        //Debug.Log("fire!");
                        mTemp = onFinished;
                        onFinished = new List<ActionData>();

                        for (int i = 0; i < mTemp.Count; i++)
                        {
                            mTemp[i].action();
                        }

                        // Re-add the previous persistent delegates
                        for (int i = 0; i < mTemp.Count; ++i)
                        {
                            ActionData ed = mTemp[i];
                            if (ed != null && !ed.oneShot)
                                AddFinishAction(ed.action, ed.oneShot);
                        }
                        mTemp = null;
                    }

                    current = before;
                }

                // if need stop
                bool needStop = false;
                if (style == Style.Once)
                {
                    needStop = true;
                }
                else if (loopCount > 0)
                {
                    currentLoopCount++;
                    Debug.Log("Finish Loop [" + currentLoopCount + "]");
                    if (currentLoopCount >= loopCount)
                    {
                        needStop = true;
                    }
                }
                if (needStop)
                {
                    //if (duration == 0f || (mFactor == 1f && mAmountPerDelta > 0f || mFactor == 0f && mAmountPerDelta < 0f))
                    enabled = false;
                }
            }
            else Sample(mFactor, false);

            //Debug.Log("factor=" + mFactor + " delta=" + mAmountPerDelta + " dt="+ delta);
        }

        private List<ActionData> mTemp = null;



        /// <summary>
        /// Mark as not started when finished to enable delay on next play.
        /// </summary>

        void OnDisable() { mStarted = false; }

        /// <summary>
        /// Sample the tween at the specified factor.
        /// </summary>

        public void Sample(float factor, bool isFinished)
        {
            // Calculate the sampling value
            float val = Mathf.Clamp01(factor);

            if (method == Method.EaseIn)
            {
                val = 1f - Mathf.Sin(0.5f * Mathf.PI * (1f - val));
                if (steeperCurves) val *= val;
            }
            else if (method == Method.EaseOut)
            {
                val = Mathf.Sin(0.5f * Mathf.PI * val);

                if (steeperCurves)
                {
                    val = 1f - val;
                    val = 1f - val * val;
                }
            }
            else if (method == Method.EaseInOut)
            {
                const float pi2 = Mathf.PI * 2f;
                val = val - Mathf.Sin(val * pi2) / pi2;

                if (steeperCurves)
                {
                    val = val * 2f - 1f;
                    float sign = Mathf.Sign(val);
                    val = 1f - Mathf.Abs(val);
                    val = 1f - val * val;
                    val = sign * val * 0.5f + 0.5f;
                }
            }
            else if (method == Method.BounceIn)
            {
                val = BounceLogic(val);
            }
            else if (method == Method.BounceOut)
            {
                val = 1f - BounceLogic(1f - val);
            }

            // Call the virtual update
            OnUpdate(val, isFinished);
        }

        /// <summary>
        /// Main Bounce logic to simplify the Sample function
        /// </summary>

        float BounceLogic(float val)
        {
            if (val < 0.363636f) // 0.363636 = (1/ 2.75)
            {
                val = 7.5685f * val * val;
            }
            else if (val < 0.727272f) // 0.727272 = (2 / 2.75)
            {
                val = 7.5625f * (val -= 0.545454f) * val + 0.75f; // 0.545454f = (1.5 / 2.75) 
            }
            else if (val < 0.909090f) // 0.909090 = (2.5 / 2.75) 
            {
                val = 7.5625f * (val -= 0.818181f) * val + 0.9375f; // 0.818181 = (2.25 / 2.75) 
            }
            else
            {
                val = 7.5625f * (val -= 0.9545454f) * val + 0.984375f; // 0.9545454 = (2.625 / 2.75) 
            }
            return val;
        }

        /// <summary>
        /// Play the tween.
        /// </summary>

        /// <summary>
        /// Play the tween forward.
        /// </summary>

        public void PlayForward() { Play(true); }

        /// <summary>
        /// Play the tween in reverse.
        /// </summary>

        public void PlayReverse() { Play(false); }

        /// <summary>
        /// Manually activate the tweening process, reversing it if necessary.
        /// </summary>

        public void Play(bool forward)
        {
            curTime = getTime();
            mAmountPerDelta = Mathf.Abs(amountPerDelta);
            if (!forward) mAmountPerDelta = -mAmountPerDelta;
            enabled = true;

            currentLoopCount = 0;

            //Debug.Log("Played!!" + forward + " delta=" +mAmountPerDelta);
            Update();
        }

        [HideInInspector]
        public float curTime;
        /// <summary>
        /// Manually reset the tweener's state to the beginning.
        /// If the tween is playing forward, this means the tween's start.
        /// If the tween is playing in reverse, this means the tween's end.
        /// </summary>
        public void ResetToBeginning()
        {
            curTime = getTime();
            mStarted = false;
            mFactor = (amountPerDelta < 0f) ? startFactorReverse : startFactor;
            Sample(mFactor, false);
        }


        /// <summary>
        /// Manually start the tweening process, reversing its direction.
        /// </summary>

        public void Toggle()
        {
            if (mAmountPerDelta > 0f)
            {
                mAmountPerDelta = -amountPerDelta;
            }
            else
            {
                mAmountPerDelta = Mathf.Abs(amountPerDelta);
            }
            enabled = true;
        }

        /// <summary>
        /// Actual tweening logic should go here.
        /// </summary>

        //abstract protected void OnUpdate(float factor, bool isFinished);
        abstract protected void OnUpdate(float value, bool isFinished);

        /// <summary>
        /// Starts the tweening operation.
        /// </summary>

        static public T Begin<T>(GameObject go, float duration) where T : UITweener
        {
            T comp = go.GetComponent<T>();
#if UNITY_FLASH
		if ((object)comp == null) comp = (T)go.AddComponent<T>();
#else
            // Find the tween with an unset group ID (group ID of 0).
            if (comp != null && comp.tweenGroup != 0)
            {
                comp = null;
                T[] comps = go.GetComponents<T>();
                for (int i = 0, imax = comps.Length; i < imax; ++i)
                {
                    comp = comps[i];
                    if (comp != null && comp.tweenGroup == 0) break;
                    comp = null;
                }
            }

            if (comp == null)
            {
                comp = go.AddComponent<T>();

                if (comp == null)
                {
                    return null;
                }
            }
#endif
            comp.mStarted = false;
            comp.duration = duration;
            comp.mFactor = 0f;
            comp.mAmountPerDelta = Mathf.Abs(comp.amountPerDelta);
            comp.style = Style.Once;
            comp.animationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));
            comp.enabled = true;
            return comp;
        }

        /// <summary>
        /// Set the 'from' value to the current one.
        /// </summary>

        public virtual void SetStartToCurrentValue() { }

        /// <summary>
        /// Set the 'to' value to the current one.
        /// </summary>

        public virtual void SetEndToCurrentValue() { }


        protected float GetFloatValueByCurve(AnimationCurve curve, float val, float from, float to)
        {
            float factor = curve != null ? curve.Evaluate(val) : val;
            float rs = from * (1f - factor) + to * factor;
            return rs;
        }

    }
}