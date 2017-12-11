using DataMesh.AR.Anchor;
using DataMesh.AR.Event;
using DataMesh.AR.Interactive;
using DataMesh.AR.SpectatorView;
using DataMesh.AR.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DataMesh.AR.SpectatorView
{
    public class UIFormAnchorControl : BaseUIForm, IPointerEnterHandler, IPointerExitHandler
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        public Button buttonAnchorReset;
        public Button buttonCameraReset;
        public Button buttonClosePannel;
        public Slider sliderAnchorSpeed;
        public Text textAnchorSpeed;
        public List<Button> buttonMove;
        public List<Button> buttonRotate;

        private bool isMove;
        private bool isRotate;
        private string currentButtonName;
        private MultiInputManager inputManager;
        private Camera mainCamera;
        private Vector3 maniDelta;
        private Vector3 maniRotate;
        private float currentAnchorSpeed = 1f;
        private TransData resetCameraData;
        private List<AnchorObjectInfo> resetAnchorObjectInfoList;
        private object cbTapAction;

        private Dictionary<string, string> dicConfig;

        public struct TransData
        {
            public Vector3 Position { get; set; }
            public Quaternion Rotation { get; set; }
        }

        private void Awake()
        {
            for (int i = 0; i < buttonMove.Count; i++)
            {
                ETListener.Get(buttonMove[i].gameObject).onDown = ButtonMoveClickDown;
                ETListener.Get(buttonMove[i].gameObject).onUp = ButtonMoveClickUp;
            }

            for (int i = 0; i < buttonRotate.Count; i++)
            {
                ETListener.Get(buttonRotate[i].gameObject).onDown = ButtonRotateClickDown;
                ETListener.Get(buttonRotate[i].gameObject).onUp = ButtonRotateClickUp;
            }

            RigisterButtonEvent(buttonAnchorReset, AnchorReset);
            RigisterButtonEvent(buttonCameraReset, CameraReset);

            mainCamera = Camera.main;
            sliderAnchorSpeed.onValueChanged.AddListener(ChangeSliderValue);

            RigisterButtonEvent(buttonClosePannel, ButtonClosePannel);

        }

        public override void Init()
        {
            inputManager = MultiInputManager.Instance; //或者在Display时候初始化
            ChangeSliderValue(0.1f);
            SceneAnchorController.Instance.AddCallbackTurnOff(HideAnchorPannelEvent);

            string path = LiveController.Instance.GetSavePath();
            //dicConfig = new Dictionary<string, string>();
            //dicConfig = AppConfig.AnalyseConfigFile(path);
            if (!File.Exists(path))
            {
                LiveController.Instance.SaveTransformToFile();
            }
        }

        public override void Display()
        {
            base.Display();
            LiveController.Instance.StartMoveAnchor();
            resetAnchorObjectInfoList = new List<AnchorObjectInfo>();
            resetAnchorObjectInfoList = SceneAnchorController.Instance.anchorObjectList;

            resetCameraData.Position = mainCamera.transform.position;
            resetCameraData.Rotation = mainCamera.transform.rotation;
        }

        public override void Hiding()
        {
            base.Hiding();
            MultiInputManager.Instance.cbTap = (Action<int>)cbTapAction;
        }

        private void Update()
        {
            if (isMove && !isRotate)
            {
                AnchorMove(currentButtonName);
            }
            else if (isRotate && !isMove)
            {
                AnchorRotate(currentButtonName);
            }
        }

        private void ButtonClosePannel(GameObject obj)
        {
            CloseUIForm();
        }

        private void ButtonMoveClickDown(GameObject obj)
        {
            isMove = true;
            currentButtonName = obj.name;
            inputManager.ChangeToManipulationRecognizer();
            SceneAnchorController.Instance.spatialAdjustType = SpatialAdjustType.Move;
            inputManager.SetMoveData(0, 0, 0);
        }

        private void ButtonMoveClickUp(GameObject obj)
        {
            isMove = false;
            currentButtonName = null;
            SceneAnchorController.Instance.spatialAdjustType = SpatialAdjustType.None;
            inputManager.SetMoveData(0, 0, 0);
        }

        private void ButtonRotateClickDown(GameObject obj)
        {
            isRotate = true;
            currentButtonName = obj.name;
            inputManager.ChangeToNavigationRecognizer();
            SceneAnchorController.Instance.spatialAdjustType = SpatialAdjustType.Rotate;
            inputManager.SetRotateData(0, 0, 0);
        }

        private void ButtonRotateClickUp(GameObject obj)
        {
            isRotate = false;
            currentButtonName = obj.name;
            SceneAnchorController.Instance.spatialAdjustType = SpatialAdjustType.None;
            inputManager.SetRotateData(0, 0, 0);
        }
        private void AnchorMove(string curName)
        {
            switch (curName)
            {
                case "buttonMoveUp":
                    inputManager.SetMoveData(0, 0, 1);
                    break;
                case "buttonMoveDown":
                    inputManager.SetMoveData(0, 0, -1);
                    break;
                case "buttonMoveLeft":

                    inputManager.SetMoveData(0, -1, 0);
                    break;
                case "buttonMoveRight":

                    inputManager.SetMoveData(0, 1, 0);
                    break;
                case "buttonMoveFront":

                    inputManager.SetMoveData(1, 0, 0);
                    break;
                case "buttonMoveRear":
                    inputManager.SetMoveData(-1, 0, 0);
                    break;
                default:
                    break;
            }
        }
        private void AnchorRotate(string curName)
        {
            switch (curName)
            {
                case "buttonRotateUp":
                    inputManager.SetRotateData(0, 0, 1);
                    break;
                case "buttonRotateDown":
                    inputManager.SetRotateData(0, 0, -1);
                    break;
                case "buttonRotateLeft":
                    inputManager.SetRotateData(0, 1, 0);
                    break;
                case "buttonRotateRight":
                    inputManager.SetRotateData(0, -1, 0);
                    break;
                case "buttonRotateFront":
                    inputManager.SetRotateData(1, 0, 0);
                    break;
                case "buttonRotateRear":
                    inputManager.SetRotateData(-1, 0, 0);
                    break;
                default:
                    break;
            }
        }

        public void ChangeSliderValue(float sliderValue)
        {
            if (sliderValue <= 0.5f && sliderValue > 0)
            {
                currentAnchorSpeed = 0.1f + sliderValue * 1.8f;
            }
            else if (sliderValue > 0.5f && sliderValue <= 1)
            {
                currentAnchorSpeed = 1.0f + (sliderValue - 0.5f) * 8.0f;
            }
            textAnchorSpeed.text = currentAnchorSpeed.ToString();
            inputManager.SetAnchorSpeed(currentAnchorSpeed);
        }

        public void OnPointerEnter(PointerEventData data)
        {
            if (MultiInputManager.Instance.cbTap != null)
            {
                cbTapAction = MultiInputManager.Instance.cbTap;
            }
            MultiInputManager.Instance.cbTap = null;
        }

        public void OnPointerExit(PointerEventData data)
        {
            MultiInputManager.Instance.cbTap = (Action<int>)cbTapAction;
        }


        private void HideAnchorPannelEvent()
        {
            LiveUIManager.Instance.CloseUIForm(SysDefine.UI_UIFormAnchorControl);
        }

        private void AnchorReset(GameObject obj)
        {
            LiveController.Instance.LoadTransformByFile(LoadTransType.Anchor);
        }

        private void CameraReset(GameObject obj)
        {
            LiveController.Instance.LoadTransformByFile(LoadTransType.Camera);
        }
#else
      public void OnPointerEnter(PointerEventData data)
    {
    }

    public void OnPointerExit(PointerEventData data)
    {
    }
#endif
    }
}


