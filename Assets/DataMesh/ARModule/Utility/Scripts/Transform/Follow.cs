using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataMesh.AR.Utility
{ 
    public class Follow : MonoBehaviour
    {
        public GameObject followTargetObject;
        private Vector3 v3Offset;
        public bool RelativeDistanceEnabled;
        public bool SmoothEnabled;
        public float SmoothMoveScale=100f;
        public float SmoothRotateScale=100f;
        public bool localScaleEnabled;
        // Use this for initialization
        void Start()
        {
            v3Offset = transform.position - followTargetObject.transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            if (RelativeDistanceEnabled)
            {

                if (SmoothEnabled)
                {
                    transform.position=Vector3.Slerp(transform.position, followTargetObject.transform.position + v3Offset, Time.deltaTime * SmoothMoveScale);
                    transform.rotation = Quaternion.Slerp(transform.rotation, followTargetObject.transform.rotation, Time.deltaTime* SmoothRotateScale);
                }
                else
                {
                    transform.position = followTargetObject.transform.position + v3Offset;
                    transform.rotation = followTargetObject.transform.rotation;
                }
            }
            else {
                if (SmoothEnabled)
                {
                    transform.position = Vector3.Slerp(transform.position, followTargetObject.transform.position , Time.deltaTime * SmoothMoveScale);
                    transform.rotation = Quaternion.Slerp(transform.rotation, followTargetObject.transform.rotation, Time.deltaTime * SmoothRotateScale);
                }
                else
                {
                    transform.position = followTargetObject.transform.position;
                    transform.rotation = followTargetObject.transform.rotation;
                }
            }
            if (localScaleEnabled) {
                transform.localScale = followTargetObject.transform.localScale;
            }
            
            
        }
    }
}
