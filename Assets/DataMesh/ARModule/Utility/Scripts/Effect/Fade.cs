using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataMesh.AR.Utility
{
    public class Fade : MonoBehaviour
    {
        public enum FadeType
        {
            None,
            FadeIn,
            FadeOut
        }

        public float deltaAlpha = 0.04f;

        public bool needDestory = false;

        private List<Material> matList = new List<Material>();

        private Color color;

        private FadeType currentType = FadeType.None;

        // Use this for initialization
        void Awake()
        {
            Renderer[] renderer = GetComponentsInChildren<Renderer>();
            if (renderer != null)
            {
                for (int i = 0; i < renderer.Length; i++)
                {
                    Material mat = renderer[i].material;
                    matList.Add(mat);
                }

            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (currentType == FadeType.FadeIn)
            {
                bool finish = false;
                for (int i = 0; i < matList.Count; i++)
                {
                    Material mat = matList[i];
                    if (!mat.HasProperty("_Color"))
                        continue;

                    color = mat.color;
                    color.a += deltaAlpha;
                    if (color.a >= 1)
                        color.a = 1;
                    mat.color = color;
                    if (color.a == 1)
                    {
                        finish = true;
                    }
                }
                if (finish)
                {
                    this.currentType = FadeType.None;
                }
            }
            else if (currentType == FadeType.FadeOut)
            {
                bool finish = false;
                for (int i = 0; i < matList.Count; i++)
                {
                    Material mat = matList[i];
                    if (!mat.HasProperty("_Color"))
                        continue;

                    color = mat.color;
                    color.a -= deltaAlpha;
                    if (color.a <= 0)
                        color.a = 0;
                    mat.color = color;
                    if (color.a == 0)
                    {
                        finish = true;
                    }
                }
                if (finish)
                {
                    this.currentType = FadeType.None;

                    if (needDestory)
                        Destroy(gameObject);
                    else
                        gameObject.SetActive(false);
                }
            }
        }

        public void FadeIn()
        {
            gameObject.SetActive(true);

            for (int i = 0; i < matList.Count; i++)
            {
                Material mat = matList[i];
                if (!mat.HasProperty("_Color"))
                    continue;
                color = mat.color;
                color.a = 0;
                mat.color = color;
            }
            currentType = FadeType.FadeIn;
        }

        public void FadeOut()
        {
            for (int i = 0; i < matList.Count; i++)
            {
                Material mat = matList[i];
                if (!mat.HasProperty("_Color"))
                    continue;
                color = mat.color;
                color.a = 1;
                mat.color = color;
            }
            currentType = FadeType.FadeOut;
        }
    }
}