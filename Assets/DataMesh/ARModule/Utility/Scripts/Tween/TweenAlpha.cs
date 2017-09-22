using UnityEngine;

namespace DataMesh.AR.Utility
{

    public class TweenAlpha : UITweener
    {
        [Range(0f, 1f)]
        public float from = 1f;
        [Range(0f, 1f)]
        public float to = 1f;

        bool mCached = false;
        Material mMat;
        SpriteRenderer mSr;

        [System.Obsolete("Use 'value' instead")]
        public float alpha { get { return this.value; } set { this.value = value; } }

        void Cache()
        {
            mCached = true;
            mSr = GetComponentInChildren<SpriteRenderer>();

            if (mSr == null)
            {
                Renderer ren = GetComponentInChildren<Renderer>();
                if (ren != null)
                {
                    if (!Application.isPlaying)
                    {
                        mMat = ren.sharedMaterial;
                    }
                    else
                    {
                        mMat = ren.material;
                    }
                }
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
                if (mSr != null) return mSr.color.a;
                return mMat != null ? mMat.color.a : 1f;
            }
            set
            {
                if (!mCached) Cache();

                if (mSr != null)
                {
                    Color c = mSr.color;
                    c.a = value;
                    mSr.color = c;
                }
                else if (mMat != null)
                {
                    Color c = mMat.color;
                    c.a = value;
                    mMat.color = c;
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