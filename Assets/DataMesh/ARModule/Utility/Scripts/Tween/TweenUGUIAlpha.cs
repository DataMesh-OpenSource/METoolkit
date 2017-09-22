using UnityEngine;
using UnityEngine.UI;

namespace DataMesh.AR.Utility
{

    public class TweenUGUIAlpha : UITweener
    {
        [Range(0f, 1f)]
        public float from = 1f;
        [Range(0f, 1f)]
        public float to = 1f;

        bool mCached = false;
        CanvasRenderer mCr;
        CanvasRenderer[] subCrs = null;

        [System.Obsolete("Use 'value' instead")]
        public float alpha { get { return this.value; } set { this.value = value; } }
        public bool controlSubObject = false;

        void Cache()
        {
            mCached = true;
            mCr = GetComponent<CanvasRenderer>();

            if (controlSubObject)
            {
                subCrs = GetComponentsInChildren<CanvasRenderer>();
            }

        }

        /// <summary>
        /// Tween's current value.
        /// </summary>

        public float value
        {
            get
            {
                if (!mCached) Cache();
                float rs = 1;
                if (mCr != null) rs = mCr.GetAlpha();
                else
                {
                    if (subCrs != null)
                    {
                        rs = subCrs[0].GetAlpha();
                    }
                }
                return rs;
            }
            set
            {
                if (!mCached) Cache();

                if (mCr != null)
                {
                    mCr.SetAlpha(value);
                }
                if (subCrs != null)
                {
                    for (int i = 0; i < subCrs.Length; i++)
                    {
                        subCrs[i].SetAlpha(value);
                    }
                }
            }
        }

        /// <summary>
        /// Tween the value.
        /// </summary>

        protected override void OnUpdate(float val, bool isFinished)
        {
            float factor = animationCurve != null ? animationCurve.Evaluate(val) : val;
            value = Mathf.Lerp(from, to, factor);
        }


        public override void SetStartToCurrentValue() { from = value; }
        public override void SetEndToCurrentValue() { to = value; }
    }

}