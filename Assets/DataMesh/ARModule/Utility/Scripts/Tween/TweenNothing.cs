using UnityEngine;

namespace DataMesh.AR.Utility
{

    public class TweenNothing : UITweener
    {
        [HideInInspector]

        /// <summary>
        /// Tween's current value.
        /// </summary>

        public System.Action<float, bool> OnChanged;

        protected override void OnUpdate(float val, bool isFinished)
        {
            float factor = animationCurve != null ? animationCurve.Evaluate(val) : val;
            if (OnChanged != null) { OnChanged(factor, isFinished); }
        }



    }
}