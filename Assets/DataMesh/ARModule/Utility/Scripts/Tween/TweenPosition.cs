using UnityEngine;

namespace DataMesh.AR.Utility
{

    public class TweenPosition : UITweener
    {
        public Vector3 from;
        public Vector3 to;

        public bool tweenX = true;
        public bool tweenY = true;
        public bool tweenZ = true;

        [HideInInspector]
        public bool worldSpace = false;

        [HideInInspector]
        public bool UseAdditionalCurve = false;

        Transform mTrans;

        /// <summary>
        /// 指定有几个额外曲线。
        /// 子类可以重写
        /// </summary>
        /// <returns></returns>
        public override int GetAdditionalCurveCount()
        {
            return 2;
        }


        public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

        /// <summary>
        /// Tween's current value.
        /// </summary>

        public Vector3 value
        {
            get
            {
                return worldSpace ? cachedTransform.position : cachedTransform.localPosition;
            }
            set
            {
                Vector3 tp = worldSpace ? cachedTransform.position : cachedTransform.localPosition;
                Vector3 rs = new Vector3(
                    tweenX ? value.x : tp.x,
                    tweenY ? value.y : tp.y,
                    tweenZ ? value.z : tp.z
                    );
                if (worldSpace)
                    cachedTransform.position = rs;
                else
                    cachedTransform.localPosition = rs;
            }
        }

        void Awake() { }

        /// <summary>
        /// Tween the value.
        /// </summary>

        protected override void OnUpdate(float val, bool isFinished)
        {
            if (UseAdditionalCurve)
            {
                float fx = GetFloatValueByCurve(animationCurve, val, from.x, to.x);
                float fy = GetFloatValueByCurve(additionalAnimationCurveList.Count > 0 ? additionalAnimationCurveList[0] : null, val, from.y, to.y);
                float fz = GetFloatValueByCurve(additionalAnimationCurveList.Count > 1 ? additionalAnimationCurveList[1] : null, val, from.z, to.z);
                value = new Vector3(fx, fy, fz);
            }
            else
            {
                float factor = animationCurve != null ? animationCurve.Evaluate(val) : val;
                value = from * (1f - factor) + to * factor;
            }
        }




        [ContextMenu("Set 'From' to current value")]
        public override void SetStartToCurrentValue() { from = value; }

        [ContextMenu("Set 'To' to current value")]
        public override void SetEndToCurrentValue() { to = value; }

        [ContextMenu("Assume value of 'From'")]
        void SetCurrentValueToStart() { value = from; }

        [ContextMenu("Assume value of 'To'")]
        void SetCurrentValueToEnd() { value = to; }
    }

}