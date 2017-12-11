using DataMesh.AR.Anchor;
using DataMesh.AR.Event;
using DataMesh.AR.Interactive;
using DataMesh.AR.SpectatorView;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
public class LiveAnchorControlPannel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject buttonsMoveObj;
    public GameObject buttonsRotateObj;
    public Slider anchorSpeedSlider;

    public Button buttonResetAnchor;
    //按钮按照上下左右前后的顺序
    [HideInInspector]
    public GameObject[] buttonMove;
    [HideInInspector]
    public GameObject[] buttonRotate;
    public static LiveAnchorControlPannel instance;

    private bool isMove;
    private bool isRotate;
    private string currentButtionName;
    private MultiInputManager inputManager;
    private Vector3 maniDelta;
    private Vector3 navRotate;
    private Camera mainCamera;
    private float t;
    private object cbtapAction;
    private float speedNum = 0.5f;

    private void Awake()
    {
        instance = this;
        int buttonsMove = buttonsMoveObj.transform.childCount;
        int buttonsRotate = buttonsRotateObj.transform.childCount;
        inputManager = MultiInputManager.Instance;
        buttonMove = new GameObject[buttonsMove];
        buttonRotate = new GameObject[buttonsRotate];
        for (int i = 0; i < buttonsRotate; i++)
        {
            buttonMove[i] = buttonsMoveObj.transform.GetChild(i).gameObject;
            buttonRotate[i] = buttonsRotateObj.transform.GetChild(i).gameObject;
            ETListener.Get(buttonMove[i]).onDown = ButtonMoveClickDown;
            ETListener.Get(buttonMove[i]).onUp = ButtonMoveClickUp;

            ETListener.Get(buttonRotate[i]).onDown = ButtonRotateClickDown;
            ETListener.Get(buttonRotate[i]).onUp = ButtonRotateClickUp;

            ETListener.Get(buttonResetAnchor.gameObject).onClick = ResetAnchor;
        }
        t = Time.realtimeSinceStartup;
        mainCamera = Camera.main;

        anchorSpeedSlider.onValueChanged.AddListener(ChangeSliderValue);

        //this.gameObject.SetActive(false);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (isMove && !isRotate)
        {
            AnchorMove(currentButtionName);
        }
        else if (isRotate && !isMove)
        {
            AnchorRotate(currentButtionName);
        }
        t = Time.realtimeSinceStartup;
    }

    private void ButtonMoveClickDown(GameObject obj)
    {
        isMove = true;
        currentButtionName = obj.name;
        SceneAnchorController.Instance.spatialAdjustType = SpatialAdjustType.Move;
        inputManager.cbManipulationStart(new Vector3(0,0,0));
        maniDelta = Vector3.zero;
    }

    private void ButtonMoveClickUp(GameObject obj)
    {
        isMove = false;
        currentButtionName = null;
        SceneAnchorController.Instance.spatialAdjustType = SpatialAdjustType.None;
    }

    private void ButtonRotateClickDown(GameObject obj)
    {
        isRotate = true;
        currentButtionName = obj.name;
        SceneAnchorController.Instance.spatialAdjustType = SpatialAdjustType.Rotate;
        inputManager.cbNavigationStart(new Vector3(0, 0, 0));
        navRotate = Vector3.zero;
    }

    private void ButtonRotateClickUp(GameObject obj)
    {
        isRotate = false;
        currentButtionName = obj.name;
        SceneAnchorController.Instance.spatialAdjustType = SpatialAdjustType.None;
    }

    private void AnchorMove(string curName)
    {
        float dT = Time.realtimeSinceStartup - t;
        switch (curName)
        {
            case "Move_Up":
                maniDelta += mainCamera.transform.TransformDirection(new Vector3(0, 1, 0) * 4 * dT * speedNum);
                inputManager.cbManipulationUpdate(maniDelta);
                break;
            case "Move_Down":
                maniDelta += mainCamera.transform.TransformDirection(new Vector3(0, -1, 0) * 4 * dT * speedNum);
                inputManager.cbManipulationUpdate(maniDelta);
                break;
            case "Move_Left":
                maniDelta += mainCamera.transform.TransformDirection(new Vector3(-1, 0, 0) * 4 * dT * speedNum);
                inputManager.cbManipulationUpdate(maniDelta);
                break;
            case "Move_Right":
                maniDelta += mainCamera.transform.TransformDirection(new Vector3(1, 0, 0) * 4 * dT * speedNum);
                inputManager.cbManipulationUpdate(maniDelta);
                break;
            case "Move_Front":
                maniDelta += mainCamera.transform.TransformDirection(new Vector3(0, 0, 1) * 4 * dT * speedNum);
                inputManager.cbManipulationUpdate(maniDelta);
                break;
            case "Move_Rear":
                maniDelta += mainCamera.transform.TransformDirection(new Vector3(0, 0, -1) * 4 * dT * speedNum);
                inputManager.cbManipulationUpdate(maniDelta);
                break;
            default:
                break;
        }
    }

    private void AnchorRotate(string curName)
    {
        float dT = Time.realtimeSinceStartup - t;
        switch (curName)
        {
            case "Rotate_Up":
                navRotate += new Vector3(0, 0, 1) * 4 * dT * speedNum;
                inputManager.cbNavigationUpdate(navRotate);
                break;
            case "Rotate_Down":
                navRotate += new Vector3(0, 0, -1) * 4 * dT * speedNum;
                inputManager.cbNavigationUpdate(navRotate);
                break;
            case "Rotate_Left":
                navRotate += new Vector3(0, -1, 0) * 4 * dT * speedNum;
                inputManager.cbNavigationUpdate(navRotate);
                break;
            case "Rotate_Right":
                navRotate += new Vector3(0, 1, 0) * 4 * dT * speedNum;
                inputManager.cbNavigationUpdate(navRotate);
                break;
            case "Rotate_Front":
                navRotate += new Vector3(-1, 0, 0) * 4 * dT * speedNum;
                inputManager.cbNavigationUpdate(navRotate);
                break;
            case "Rotate_Rear":
                navRotate += new Vector3(1, 0, 0) * 4 * dT * speedNum;
                inputManager.cbNavigationUpdate(navRotate);
                break;
            default:
                break;
        }
    }

    public void OnPointerEnter(PointerEventData data)
    {
        ButtonEnter(this.gameObject);
    }

    public void OnPointerExit(PointerEventData data)
    {
        ButtonExit(this.gameObject);
    }

    private void ButtonEnter(GameObject obj)
    {
        if (MultiInputManager.Instance.cbTap != null)
        {
            cbtapAction = MultiInputManager.Instance.cbTap;
        }
        MultiInputManager.Instance.cbTap = null;
    }

    private void ButtonExit(GameObject obj)
    {
        MultiInputManager.Instance.cbTap = (Action<int>)cbtapAction;
    }

    public void ChangeSliderValue(float sliderValue)
    {
        speedNum = sliderValue;
    }

    public void ResetAnchor(GameObject obj)
    {
        string FolderName = Application.dataPath + "/../SaveData/";
        if (IsFolderExists(FolderName))
        {
            Directory.Delete(FolderName,true);
        }
    }

    /// 检测是否存在文件夹
    public static bool IsFolderExists(string folderPath)
    {
        if (folderPath.Equals(string.Empty))
        {
            return false;
        }

        return Directory.Exists(folderPath);
    }
}
#endif
