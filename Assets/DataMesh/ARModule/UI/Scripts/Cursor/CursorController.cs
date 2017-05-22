using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DataMesh.AR.Interactive;

public class CursorController : DataMesh.AR.MEHoloModuleSingleton<CursorController>
{
    public Transform CurserRoot;
    public Transform InfoRoot;

    public GameObject cursorNormal;
    public GameObject cursorRemove;
    public GameObject cursorHand;
    public GameObject cursorLoading;
    public GameObject cursorEye;

    public GameObject cursorRecording;
    public GameObject cursorSnap;
    public GameObject infoPrefab;

    public GameObject moveAxis;
    public GameObject moveAxisX;
    public GameObject moveAxisY;
    public GameObject moveAxisZ;

    public GameObject cursorRotateScene;

    public Text timeText;

    //public GameObject infoTextPrefab;

    private float recordTimeRemain;
    private bool recordCountingDown = false;

    public TweenUGUIAlpha tw3;
    public TweenUGUIAlpha tw2;
    public TweenUGUIAlpha tw1;

    public System.Action cbSnapCountDown;


    private MultiInputManager gm;

    private Transform mainCameraTrans;

    private bool isTurnOn = false;

    /// <summary>
    /// 设置游标为忙状态 
    /// </summary>
    [HideInInspector]
    public bool isBusy = false;

    ///////////////////以下是鼠标相关////////////////
    public float DistanceFromCollision;

    protected override void Awake()
    {
        base.Awake();

        if (cursorNormal != null)
            cursorNormal.SetActive(false);
        if (cursorRemove != null)
            cursorRemove.SetActive(false);
        if (cursorHand != null)
            cursorHand.SetActive(false);
        if (cursorLoading != null)
            cursorLoading.SetActive(false);
        if (cursorEye != null)
            cursorEye.SetActive(false);

        cursorRecording.SetActive(false);
        cursorSnap.SetActive(false);
        tw3.gameObject.SetActive(false);
        tw2.gameObject.SetActive(false);
        tw1.gameObject.SetActive(false);

        moveAxis.SetActive(false);

        cursorRotateScene.SetActive(false);
    }

    protected override void _Init()
    {
        gm = MultiInputManager.Instance;

        mainCameraTrans = Camera.main.transform;

    }


    protected override void _TurnOn()
    {
        CurserRoot.gameObject.SetActive(true);
        InfoRoot.gameObject.SetActive(true);

        isTurnOn = true;
    }

    protected override void _TurnOff()
    {
        CurserRoot.gameObject.SetActive(false);
        InfoRoot.gameObject.SetActive(false);

        isTurnOn = false;
    }

    void Update()
    {

        if (!isTurnOn)
            return;

        if (recordCountingDown)
        {
            recordTimeRemain -= Time.deltaTime;
            //Debug.Log("dt=" + Time.deltaTime);
            if (recordTimeRemain <= 0)
            {
                recordTimeRemain = 0;
                recordCountingDown = false;
            }

            timeText.text = recordTimeRemain.ToString("0.00");

        }
    }

    void LateUpdate()
    {
        if (!isTurnOn)
        {
            //Debug.Log("!IsturnOn");
            return;
        }



        if (mainCameraTrans == null)
        {
            Debug.Log("mainCameraTrans == null");
            return;
        }

        
        // 忙的时候，只有转圈
        if (isBusy)
        {
            cursorLoading.SetActive(true);
            cursorNormal.SetActive(false);
            cursorHand.SetActive(false);
            cursorRemove.SetActive(false);
            cursorEye.SetActive(false);
        }
        else
        {
           
            cursorLoading.SetActive(false);
            cursorEye.SetActive(false);

            // 根据主状态显示光标
            if (gm.FocusedObject != null)
            {
                cursorNormal.SetActive(false);
                cursorHand.SetActive(true);
            }
            else
            {
               
                cursorNormal.SetActive(true);
                cursorHand.SetActive(false);
            }

        }

       

        Vector3 cp;
        Vector3 cn;
        Vector3 cf;

        float dis = 5;

        // 计算游标位置和方向，多数需要依赖于碰撞物体的位置 
        if (gm.FocusedObject != null)
        {
            cp = gm.hitPoint;
            cn = gm.hitNormal;
            if (cursorNormal.activeSelf)
                cf = gm.hitNormal;
            else
                cf = gm.gazeDirection;
        }
        else
        {
            cp = gm.headPosition + gm.gazeDirection * dis;
            cn = -gm.gazeDirection;
            cf = gm.gazeDirection;
        }

        CurserRoot.position = cp + cn * DistanceFromCollision;
        CurserRoot.forward = cf;

        // 计算提示图标的位置和方向，通常与碰撞物体无关，固定距离 
        InfoRoot.position = gm.headPosition + gm.gazeDirection * 2;
        InfoRoot.forward = gm.gazeDirection;

        moveAxis.transform.rotation = Quaternion.identity;
    }



    public void StartCountdown(float n)
    {
        recordTimeRemain = n;
        recordCountingDown = true;

        cursorRecording.SetActive(true);
    }

    public void StopCountdown()
    {
        cursorRecording.SetActive(false);
    }


    public void ShowSnapCountDown(System.Action cb)
    {
        // 上一个倒计时还没走完，不再重新走 
        if (cbSnapCountDown != null)
            return;

        cursorSnap.SetActive(true);
        cbSnapCountDown = cb;

        tw3.gameObject.SetActive(true);
        tw3.ResetToBeginning();
        tw3.AddFinishAction(ShowSpanCountDown3, true);
        tw3.Play(true);
    }

    private void ShowSpanCountDown3()
    {
        tw3.gameObject.SetActive(false);

        tw2.gameObject.SetActive(true);
        tw2.ResetToBeginning();
        tw2.AddFinishAction(ShowSpanCountDown2, true);
        tw2.Play(true);

    }

    private void ShowSpanCountDown2()
    {
        tw2.gameObject.SetActive(false);

        tw1.gameObject.SetActive(true);
        tw1.ResetToBeginning();
        tw1.AddFinishAction(ShowSpanCountDown1, true);
        tw1.Play(true);

    }

    private void ShowSpanCountDown1()
    {
        tw1.gameObject.SetActive(false);
        cursorSnap.SetActive(false);

        if (cbSnapCountDown != null)
        {
            cbSnapCountDown();
            cbSnapCountDown = null;
        }
    }

    public void ShowInfo(string s)
    {
        GameObject obj = PrefabUtils.CreateGameObjectToParent(InfoRoot.gameObject, infoPrefab);
        obj.SetActive(true);
        InfoText info = obj.GetComponent<InfoText>();
        info.SetText(s);
        
    }

    public void HideError()
    {
        infoPrefab.SetActive(false);
    }




    public void StartRecoding()
    {
        StartCountdown(3);

    }

    public void StopRecording()
    {
        StopCountdown();
    }

    public void ShowMoveAxis(Vector3 dir)
    {
        moveAxis.SetActive(true);

        moveAxisX.SetActive(false);
        moveAxisY.SetActive(false);
        moveAxisZ.SetActive(false);
        if (dir.x != 0)
        {
            moveAxisX.SetActive(true);
        }
        if (dir.y != 0)
        {
            moveAxisY.SetActive(true);
        }
        if (dir.z != 0)
        {
            moveAxisZ.SetActive(true);
        }
    }

    public void HideMoveAxis()
    {
        moveAxis.SetActive(false);
    }

    public void ShowRotateScene()
    {
        cursorRotateScene.SetActive(true);
    }

    public void HideRotateScene()
    {
        cursorRotateScene.SetActive(false);
    }

}
