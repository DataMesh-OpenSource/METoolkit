using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceToCamera : MonoBehaviour
{
    public bool useOffset = false;
    public Vector3 rotateOffset;

    private Transform cameraTrans;
    private Transform selfTrans;

    void Awake()
    {
        cameraTrans = Camera.main.transform;
        selfTrans = transform;
    }

	
	// Update is called once per frame
	void LateUpdate ()
    {
        Vector3 dir = selfTrans.position - cameraTrans.position;
        dir.y = 0;
        selfTrans.forward = dir;
        if (useOffset)
        {
            transform.localEulerAngles += rotateOffset;
        }
        
	}
}
