using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataMesh.AR.Utility
{
    public class RotateForever : MonoBehaviour
    {
        public bool randomInit;


        public Vector3 speed;
        private Transform trans;

        // Use this for initialization
        void Awake()
        {
            trans = transform;
            if (randomInit)
            {
                speed.x = Random.Range(0.2f, 1.4f) * speed.x;
                speed.y = Random.Range(0.2f, 1.4f) * speed.y;
                speed.z = Random.Range(0.2f, 1.4f) * speed.z;
            }
        }

        // Update is called once per frame
        void Update()
        {
            trans.Rotate(speed * Time.deltaTime);
        }
    }
}