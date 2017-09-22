using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataMesh.AR.Utility
{
    public class TweenMaterialFloat : TweenMaterial
    {
        public string propertyName;
        public float from = 0f;
        public float to = 0f;

        public float value
        {
            get
            {
                if (!mCached) Cache();
                if (mats.Count > 0)
                {
                    Material mat = mats[0];
                    if (mat.HasProperty(propertyName))
                        return mats[0].GetFloat(propertyName);
                }
                return 0;
            }
            set
            {
                if (!mCached) Cache();

                if (mats != null)
                {
                    for (int i = 0;i < mats.Count;i ++)
                    {
                        mats[i].SetFloat(propertyName, value);
                        //Debug.Log("Material:" + mats + " pro=" + propertyName + " value=" + value);
                    }
                }
                else
                {
                    Debug.LogWarning("mat is null!");
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