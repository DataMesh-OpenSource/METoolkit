using UnityEngine;
using System.Collections.Generic;

namespace DataMesh.AR.Interactive
{
    public class CameraFlyController : MonoBehaviour
    {
        private float speed = 4f;

        private Transform tr;

        private Vector3 mpStart;
        private Vector3 originalRotation;

        private float t = 0f;

        public List<Transform> followedObjects = new List<Transform>();

        // 
        void Awake()
        {
            tr = GetComponent<Transform>();
            t = Time.realtimeSinceStartup;
        }

        // 
        void Update()
        {

            // Movement
            float forward = 0f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) { forward += 1f; }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) { forward -= 1f; }

            float right = 0f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) { right += 1f; }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) { right -= 1f; }

            float up = 0f;
            if (Input.GetKey(KeyCode.E)) { up += 1f; }
            if (Input.GetKey(KeyCode.Q)) { up -= 1f; }

            float dT = Time.realtimeSinceStartup - t;
            t = Time.realtimeSinceStartup;

            if (forward != 0 || right != 0 || up != 0)
            {
                tr.position += tr.TransformDirection(new Vector3(right, up, forward) * speed * (Input.GetKey(KeyCode.LeftShift) ? 2f : 1f) * dT);
            }


            // Rotation
            Vector3 mpEnd = Input.mousePosition;

            // Right Mouse Button Down
            if (Input.GetMouseButtonDown(1))
            {
                originalRotation = tr.localEulerAngles;
                mpStart = mpEnd;
            }

            // Right Mouse Button Hold
            if (Input.GetMouseButton(1))
            {
                Vector2 offs = new Vector2((mpEnd.x - mpStart.x) / Screen.width, (mpStart.y - mpEnd.y) / Screen.height);
                tr.localEulerAngles = originalRotation + new Vector3(offs.y * 360f, offs.x * 360f, 0f);


            }

            for (int i = 0; i < followedObjects.Count; i++)
            {
                Transform trans = followedObjects[i];
                trans.rotation = tr.rotation;
                trans.position = tr.position;
            }
        }
    }
}

