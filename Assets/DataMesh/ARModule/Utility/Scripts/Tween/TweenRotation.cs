using UnityEngine;

namespace DataMesh.AR.Utility
{

    public class TweenRotation : UITweener
    {
        public Vector3 from;
        public Vector3 to;
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
        public Quaternion value
        {
            get
            {
                return cachedTransform.localRotation;
            }
            set
            {
                cachedTransform.localRotation = value;
            }
        }

        /// <summary>
        /// Tween the value.
        /// </summary>

        protected override void OnUpdate(float val, bool isFinished)
        {
            if (UseAdditionalCurve)
            {
                float fx = GetFloatValueByCurve(animationCurve, val, from.x, to.x);
                float fy = GetFloatValueByCurve(additionalAnimationCurveList.Count > 0 ? additionalAnimationCurveList[0] : null, val, from.y, to.y);
                float fz = GetFloatValueByCurve(additionalAnimationCurveList.Count > 1 ? additionalAnimationCurveList[1] : null, val, from.y, to.y);
                value = Quaternion.Euler(new Vector3(fx, fy, fz));
            }
            else
            {
                float fx = GetFloatValueByCurve(animationCurve, val, from.x, to.x);
                float fy = GetFloatValueByCurve(animationCurve, val, from.y, to.y);
                float fz = GetFloatValueByCurve(animationCurve, val, from.z, to.z);
                value = Quaternion.Euler(new Vector3(fx, fy, fz));
            }

        }



        [ContextMenu("Set 'From' to current value")]
        public override void SetStartToCurrentValue() { from = value.eulerAngles; }

        [ContextMenu("Set 'To' to current value")]
        public override void SetEndToCurrentValue() { to = value.eulerAngles; }

        [ContextMenu("Assume value of 'From'")]
        void SetCurrentValueToStart() { value = Quaternion.Euler(from); }

        [ContextMenu("Assume value of 'To'")]
        void SetCurrentValueToEnd() { value = Quaternion.Euler(to); }
    }

}