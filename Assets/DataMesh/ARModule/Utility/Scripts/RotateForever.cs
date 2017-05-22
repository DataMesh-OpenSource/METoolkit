using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateForever : MonoBehaviour
{
    public Vector3 speed;
    private Transform trans;

	// Use this for initialization
	void Awake ()
    {
        trans = transform;
	}
	
	// Update is called once per frame
	void Update ()
    {
        trans.Rotate(speed * Time.deltaTime);
	}
}
