using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloLensXboxController;

namespace DataMesh.AR.Interactive
{

    public class XBoxControllerInputManager : MonoBehaviour
    {

#if UNITY_METRO && !UNITY_EDITOR
        private ControllerInput controllerInput;
#endif

        private bool _hasController = false;
        public bool hasContoller
        {
            get
            {
                return _hasController;
            }
        }

        void Awake()
        {
#if UNITY_METRO && !UNITY_EDITOR
            controllerInput = new ControllerInput(0, 0.19f);
#endif
        }


        private void Update()
        {

            // 检测手柄是否存在 
            _hasController = false;
            string[] joysticks = Input.GetJoystickNames();
            for (int i = 0;i < joysticks.Length;i ++)
            {
                string n = joysticks[i].Trim();
                if (!string.IsNullOrEmpty(n))
                {
                    _hasController = true;
                    break;
                }
            }

#if UNITY_METRO && !UNITY_EDITOR

            if (hasContoller)
            {
                try
                {
                    controllerInput.Update();

                }
                catch (System.Exception e)
                {
                    //Debug.Log("Exception! " + e);
                    return;
                }
            }
#endif


        }

        /// <summary>
        /// 获取左侧摇杆的横向位移
        /// </summary>
        /// <returns></returns>
        public float GetAxisLeftThumbstickX()
        {
#if UNITY_METRO && !UNITY_EDITOR
            return controllerInput.GetAxisLeftThumbstickX();
#else
            return Input.GetAxis("Horizontal");
#endif
        }

        /// <summary>
        /// 获得左侧摇杆的纵向位移
        /// </summary>
        /// <returns></returns>
        public float GetAxisLeftThumbstickY()
        {
#if UNITY_METRO && !UNITY_EDITOR
            return controllerInput.GetAxisLeftThumbstickY();
#else
            return Input.GetAxis("Vertical");
#endif
        }

        /// <summary>
        /// 获得LT按钮的位移（力度）
        /// </summary>
        /// <returns></returns>
        public float GetAxisLeftTrigger()
        {
#if UNITY_METRO && !UNITY_EDITOR
            return controllerInput.GetAxisLeftTrigger();
#else
            return Input.GetAxis("JoystickLT");
#endif
        }

            /// <summary>
            /// 获得右侧摇杆的横向位移 
            /// </summary>
            /// <returns></returns>
        public float GetAxisRightThumbstickX()
        {
#if UNITY_METRO && !UNITY_EDITOR
            return controllerInput.GetAxisRightThumbstickX();
#else
            return Input.GetAxis("RightHorizontal");
#endif
        }

        /// <summary>
        /// 获得右侧摇杆的纵向位移 
        /// </summary>
        /// <returns></returns>
        public float GetAxisRightThumbstickY()
        {
#if UNITY_METRO && !UNITY_EDITOR
            return controllerInput.GetAxisRightThumbstickY();
#else
            return Input.GetAxis("RightVertical");
#endif
        }

        /// <summary>
        /// 获得RT按钮的位移（力度）
        /// </summary>
        /// <returns></returns>
        public float GetAxisRightTrigger()
        {
#if UNITY_METRO && !UNITY_EDITOR
            return controllerInput.GetAxisRightTrigger();
#else
            return Input.GetAxis("JoystickRT");
#endif
        }

        /// <summary>
        /// 获得指定按键的按下状态 
        /// </summary>
        /// <param name="controllerButton"></param>
        /// <returns></returns>
        public bool GetButton(ControllerButton controllerButton)
        {
#if UNITY_METRO && !UNITY_EDITOR
            return controllerInput.GetButton(controllerButton);
#else
            return Input.GetButton(TransformButtonId(controllerButton));
#endif
        }

        /// <summary>
        /// 获得指定按键是否在这一帧按下 
        /// </summary>
        /// <param name="controllerButton"></param>
        /// <returns></returns>
        public bool GetButtonDown(ControllerButton controllerButton)
        {
#if UNITY_METRO && !UNITY_EDITOR
            return controllerInput.GetButtonDown(controllerButton);
#else
            return Input.GetButtonDown(TransformButtonId(controllerButton));
#endif
        }

        /// <summary>
        /// 获得指定按键是否在这一帧抬起
        /// </summary>
        /// <param name="controllerButton"></param>
        /// <returns></returns>
        public bool GetButtonUp(ControllerButton controllerButton)
        {
#if UNITY_METRO && !UNITY_EDITOR
            return controllerInput.GetButtonUp(controllerButton);
#else
            return Input.GetButtonUp(TransformButtonId(controllerButton));
#endif
        }

        private string TransformButtonId(ControllerButton controllerButton)
        {
            string rs = null;
            switch (controllerButton)
            {
                case ControllerButton.A:
                    rs = "JoystickA";
                    break;
                case ControllerButton.B:
                    rs = "JoystickB";
                    break;
                case ControllerButton.X:
                    rs = "JoystickX";
                    break;
                case ControllerButton.Y:
                    rs = "JoystickY";
                    break;
                case ControllerButton.LeftShoulder:
                    rs = "JoystickLB";
                    break;
                case ControllerButton.RightShoulder:
                    rs = "JoystickRB";
                    break;
                case ControllerButton.LeftThumbstick:
                    rs = "LeftAnalog";
                    break;
                case ControllerButton.RightThumbstick:
                    rs = "RightAnalog";
                    break;
                case ControllerButton.View:
                    rs = "JoystickView";
                    break;
                case ControllerButton.Menu:
                    rs = "JoystickMenu";
                    break;

            }

            return rs;
        }
    }
}