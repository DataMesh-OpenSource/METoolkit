using UnityEngine;
using System.Collections;
namespace DataMesh.AR.Common {

    public class FollowGazeRotation : MonoBehaviour
    {
        public GameObject cam;
        public GameObject followobject;
        public GameObject followobjecthit;
        public GameObject showCursor;
        public int distance;
        private Vector3 v3Offset;
        private MeshRenderer cursormeshRenderer;
        // Use this for initialization
        void Start()
        {
            cursormeshRenderer = followobject.gameObject.GetComponentInChildren<MeshRenderer>();
            cursormeshRenderer.enabled = true;
            v3Offset = transform.position - cam.transform.position;

        }

        // Update is called once per frame
        void Update()
        {
            // Do a raycast into the world based on the user's
            // head position and orientation.
            var headPosition = cam.transform.position;
            var gazeDirection = cam.transform.forward;
            followobject.active = false;
            followobjecthit.active = false;
            RaycastHit hitInfo;
            if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
            {
                // If the raycast hit a hologram...
                // Display the cursor mesh.
                showCursor = followobjecthit;

                // Move thecursor to the point where the raycast hit.
                showCursor.transform.position = hitInfo.point;

                // Rotate the cursor to hug the surface of the hologram.
                showCursor.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);

            }
            else
            {
                showCursor = followobject;
                //Quaternion toQuat = Camera.main.transform.localRotation;
                //toQuat.z = 0;
                //followobject.transform.rotation = toQuat;
                showCursor.transform.localPosition = ProposeTransformPosition();

                // Rotate the cursor to hug the surface of the hologram.
                showCursor.transform.rotation = Quaternion.FromToRotation(Vector3.up, cam.transform.forward);
            }
            showCursor.active = true;

        }
        Vector3 ProposeTransformPosition()
        {

            // Put the model 2m in front of the user.
            Vector3 retval = cam.transform.localPosition + cam.transform.forward * distance;

            return retval;
        }

    }

}