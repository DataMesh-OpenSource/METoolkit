using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataMesh.AR.Utility
{
    public class TweenMaterialColor : TweenMaterial
    {
        public string propertyName;
        public Color from;
        public Color to;
        public bool tweenR = true;
        public bool tweenG = true;
        public bool tweenB = true;
        public bool tweenA = true;

        public Color value
        {
            get
            {
                if (!mCached) Cache();
                if (mats.Count > 0)
                {
                    Material mat = mats[0];
                    if (mat.HasProperty(propertyName))
                        return mat.GetColor(propertyName);
                }
                return Color.white;
            }
            set
            {
                if (!mCached) Cache();

                if (mats != null)
                {
                    for (int i = 0; i < mats.Count; i++)
                    {
                        Material mat = mats[i];
                        if (mat.HasProperty(propertyName))
                        {
                            Color color = mat.GetColor(propertyName);
                            if (tweenR) color.r = value.r;
                            if (tweenG) color.g = value.g;
                            if (tweenB) color.b = value.b;
                            if (tweenA) color.a = value.a;
                            mats[i].SetColor(propertyName, color);
                        }
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
            value = Color.Lerp(from, to, factor);
        }

        public override void SetStartToCurrentValue() { from = value; }
        public override void SetEndToCurrentValue() { to = value; }
    }
}