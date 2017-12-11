using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DataMesh.AR.Anchor
{
    public class VuforiaFollow : MonoBehaviour
    {
        public GameObject ImageTarget;
        public GameObject FollowObject;
#if ME_VUFORIA_ACTIVE

        void Start() {
            if (ImageTarget && FollowObject)
            {
                 ImageTarget.transform.position= FollowObject.transform.position;
                 ImageTarget.transform.rotation= FollowObject.transform.rotation;
            }
        }
        // Update is called once per frame
        void Update()
        {
            if (ImageTarget && FollowObject)
            {
                FollowObject.transform.position = ImageTarget.transform.position;
                FollowObject.transform.rotation = ImageTarget.transform.rotation;
            }

        }
#endif
    }

}
