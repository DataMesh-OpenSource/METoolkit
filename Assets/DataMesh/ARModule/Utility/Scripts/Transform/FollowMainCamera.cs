using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataMesh.AR.Utility
{
    /// <summary>
    /// 将自身位置随时设置到主摄影机的位置
    /// 可用于帮助其他摄影机跟随主摄影机位置
    /// </summary>
    public class FollowMainCamera : MonoBehaviour
    {
        private Transform transSelf;
        private Transform transMain;

        public Vector3 positionOffset;
        public Vector3 rotationOffset;

        // Use this for initialization
        void Start()
        {
            transSelf = transform;
            transMain = Camera.main.transform;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            transSelf.position = transMain.position + positionOffset;
            transSelf.eulerAngles = transMain.eulerAngles + rotationOffset;
        }
    }

}