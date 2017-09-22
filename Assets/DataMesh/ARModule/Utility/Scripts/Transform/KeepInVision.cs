using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataMesh.AR.Utility
{
    public class KeepInVision : MonoBehaviour
    {
        public enum FaceCameraType
        {
            None,
            FaceToCamera
        }

        private Transform cameraTrans;

        // Use this for initialization
        void Start()
        {
            cameraTrans = Camera.main.transform;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}