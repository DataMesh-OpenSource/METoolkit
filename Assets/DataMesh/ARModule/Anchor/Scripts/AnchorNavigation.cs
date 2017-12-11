using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorNavigation : MonoBehaviour
{
    public GameObject targetAnchor;
    private Vector3 direction;
    private Camera mainCamera;
    private void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (targetAnchor != null)
        {
            Vector3 TargetPos = targetAnchor.transform.position;
            Vector3 cursorPos = transform.position;
            direction = TargetPos - cursorPos;
            transform.forward = direction;
        }
    }

}
