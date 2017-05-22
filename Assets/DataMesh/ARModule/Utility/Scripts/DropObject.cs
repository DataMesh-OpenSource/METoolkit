using UnityEngine;
using System.Collections.Generic;
//using System;

public class DropObject : MonoBehaviour 
{
    public Vector3 originSpeed;
    public Vector3 rotateSpeed;
    public float elasticity = 0.5f;

    public float groundY = 0;

    public float g = -20f;
    public int maxStep = 1;
    public float waitTime = -1;

    public bool randomRotateWhenHitGround = true;
    public Vector3 rotateMin;
    public Vector3 rotateMax;

    private int step = -1;

    private Vector3 startPos;
    private float startTime;
    private float g2;
    private Vector3 speed;

    private bool isStart = false;

    private Transform trans;

    public bool needDestory = true;
    
    private System.Action<GameObject, string, string> aniFinishCallBack;
    private GameObject callBackOBJ;
    private string callBackString;
	private string callBackSoldierView;

	// Use this for initialization
	public void StartShow () 
    {
        startPos = transform.localPosition;
        startTime = Time.time;
        g2 = g * 0.5f;

        speed = originSpeed;

        trans = transform;

        step = 0;
	}

    void Start()
    {
        StartShow();
    }

    public void StartShow(System.Action<GameObject, string, string> function, GameObject obj, string str, float aniWaitTime, string id)    
    {
        startPos = transform.localPosition;
        startTime = Time.time;
        g2 = g * 0.5f;

        trans = transform;

        step = 0;
        
        aniFinishCallBack = function;
        callBackOBJ = obj;
        callBackString = str;
        callBackSoldierView = id;

        waitTime = aniWaitTime;
    }

	// Update is called once per frame
	void Update () 
    {
        if(waitTime != -1)
        {
            waitTime -= Time.deltaTime;
            if (waitTime > 0)
            {
                return;
            }
            else
            {
                waitTime = -1;
                startTime = Time.time;
            }
        }

        if (step < 0)
            return;

        float curTime = Time.time;

        if (step < maxStep)
        {
            // 每一次的抛物线 
            float dt = curTime - startTime;
            float dx = startPos.x + dt * speed.x;
            float dz = startPos.z + dt * speed.z;
            float dy = startPos.y + speed.y * dt + g2 * dt * dt;

            if (dy < groundY)
            {
                dy = groundY;

                speed = new Vector3(speed.x, -(speed.y + g * dt * elasticity), speed.z);
                //Debug.Log("new speed=" + speed);
                startPos = new Vector3(dx, dy, dz);
                startTime = curTime;

                trans.localPosition = startPos;

                if (randomRotateWhenHitGround)
                {
                    rotateSpeed = new Vector3(
                        Random.Range(rotateMin.x, rotateMax.x),
                        Random.Range(rotateMin.y, rotateMax.y),
                        Random.Range(rotateMin.z, rotateMax.z)
                        );
                        
                }

                step++;

                return;
            }

            trans.localPosition = new Vector3(dx, dy, dz);

            if (rotateSpeed != Vector3.zero)
                trans.Rotate(rotateSpeed * Time.deltaTime);
            
        }
        else
        {
            // 该消失了 

            if (needDestory)
            {
                if (aniFinishCallBack != null)
                {
					aniFinishCallBack(callBackOBJ, callBackString, callBackSoldierView);
                    needDestory = false;
                }
                else
                {
                    GameObject.Destroy(gameObject);
                }                
            }
        }

	}
}


