using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using DataMesh.AR.Interactive;
using DataMesh.AR.UI;
using DataMesh.AR.Utility;

namespace DataMesh.AR.Account
{
    public class LoginUI : MonoBehaviour
    {
        public InputField nameText;
        public InputField passText;
        public Text errorText;

        public CommonButton loginButton;
        public System.Action<string, string> callbackLogin;

        private MultiInputManager inputManager;

        public static string keyboardText = "";
        private bool isInputing;
        private InputField currentInput;

        private FloatKeyboard keyboard;

        public void Init()
        {
            loginButton.callbackClick = OnClickLogin;

            SelectedEventDispatcher select = nameText.gameObject.AddComponent<SelectedEventDispatcher>();
            select.cbSelect = OnSelectText;
            select.cbDeselect = OnDeselectText;

            select = passText.gameObject.AddComponent<SelectedEventDispatcher>();
            select.cbSelect = OnSelectText;
            select.cbDeselect = OnDeselectText;

            ShowError("");

            keyboard = UIManager.Instance.keyboard;

            inputManager = MultiInputManager.Instance;
            inputManager.layerMask = LayerMask.GetMask("Default") | LayerMask.GetMask("UI");

            //nameText.OnSelect = OnSelectText;
            //nameText.OnDeselect = OnDeselectText;
        }

        private void OnSelectText(GameObject obj)
        {
            currentInput = obj.GetComponent<InputField>();

#if !UNITY_EDITOR && UNITY_UWP
            Debug.Log("Show keyboard!!!!!");

            nameText.interactable = false;
            passText.interactable = false;

            keyboard.TurnOn();
            keyboard.callbackInputFinish += OnInputFinish;
            keyboard.callbackExit += OnInputExit;
#else
            MultiInputManager.Instance.StopCapture();
#endif
        }
        private void OnDeselectText(GameObject obj)
        {
#if !UNITY_EDITOR && UNITY_UWP
#else
            MultiInputManager.Instance.StartCapture();
#endif
        }

        private void OnInputFinish(string s)
        {
            nameText.interactable = true;
            passText.interactable = true;

            currentInput.text = s;
            currentInput = null;
            keyboard.callbackInputFinish -= OnInputFinish;
            keyboard.callbackExit -= OnInputExit;
            keyboard.TurnOff();
        }

        private void OnInputExit()
        {
            nameText.interactable = true;
            passText.interactable = true;

            currentInput = null;
            keyboard.callbackInputFinish -= OnInputFinish;
            keyboard.callbackExit -= OnInputExit;
            keyboard.TurnOff();
        }

        public void ShowError(string error)
        {
            errorText.text = error;
        }

        /// <summary>
        /// 单纯的显示出来，保留之前的状态
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnClickLogin(CommonButton btn)
        {
            string name = nameText.text.Trim();
            string pass = passText.text.Trim();

            if (callbackLogin != null)
                callbackLogin(name, pass);
        }

        void Update()
        {
        }
    }
}