using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataMesh.AR.Utility
{
    public abstract class TweenMaterial : UITweener
    {
        public List<string> materialName = new List<string>();

        protected bool mCached = false;
        protected List<Material> mats;

        protected virtual void Cache()
        {
            Renderer[] ren = GetComponentsInChildren<Renderer>();
            mats = new List<Material>();
            //Debug.Log("Renders=" + ren.Length);
            if (ren.Length > 0)
            {
                for (int i = 0;i < ren.Length;i ++)
                {
                    Material[] matsInRenderer = ren[i].materials;
                    //Debug.Log("Materials=" + matsInRenderer.Length);
                    for (int j = 0;j < matsInRenderer.Length;j ++)
                    {
                        Material mat = matsInRenderer[j];

                        if (materialName.Count > 0)
                        {
                            string matName = mat.name;
                            matName = matName.Replace("(Instance)", "");
                            matName = matName.Trim();

                            //Debug.Log("mat name=" + mat.name);
                            //Debug.Log("Mat new name=" + matName);
                            if (materialName.IndexOf(matName) >= 0)
                            {
                                mats.Add(mat);
                            }
                        }
                        else
                        {
                            mats.Add(mat);
                        }
                    }
                }
            }
            mCached = true;
            //Debug.Log("Find " + mats.Count + " materials!");
        }

    }
}