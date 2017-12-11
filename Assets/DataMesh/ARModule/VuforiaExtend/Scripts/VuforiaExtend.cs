using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DataMesh.AR.Anchor
{

    public class VuforiaExtend : MonoBehaviour
    {
#if ME_VUFORIA_ACTIVE
        private static VuforiaExtend vuforiaExtend = null;

        /// <summary>
        /// A simple static singleton getter to the VuforiaBehaviour (if present in the scene)
        /// Will return null if no VuforiaBehaviour has been instanciated in the scene.
        /// </summary>
        public static VuforiaExtend Instance
        {
            get
            {
                if (vuforiaExtend == null)
                    vuforiaExtend = FindObjectOfType<VuforiaExtend>();

                return vuforiaExtend;
            }
        }
        public GameObject ARCameraPrefab;
        public GameObject ImageTargetPrefab;
        public List<GameObject> ImageTargets = new List<GameObject>();
        public string VuforiaKey;
        public bool VuforiaIsExist;
#endif
    }

}

