using DataMesh.AR.Event;
using DataMesh.AR.Interactive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace DataMesh.AR.SpectatorView
{
    public class UIFormSetting : BaseUIForm , IPointerEnterHandler, IPointerExitHandler
    {

        public Button buttonClose;
        public Button buttonExit;
        public Button buttonYES;
        public Button buttonNO;
        public Sprite hightLightImage;
        public Sprite normalImage;
        public GameObject exitApplicationUI;

        private Image buttonSprite;
        private object cbTapAction;
        private void Awake()
        {
            buttonSprite = buttonExit.GetComponent<Image>();
            RigisterButtonEvent(buttonClose, CloseThisUIForm);
            RigisterButtonEvent(buttonExit, ShowExitUI);
            RigisterButtonEvent(buttonYES,Exit);
            RigisterButtonEvent(buttonNO, HideExitUI);

            ETListener.Get(buttonExit.gameObject).onEnter = ButtonExitOnEnter;
            ETListener.Get(buttonExit.gameObject).onExit = ButtonExitOnExit;
        }

        public override void Display()
        {
            base.Display();
            LiveUIManager.Instance.ShowUIForms(SysDefine.UI_UIFormSocial);
            LiveUIManager.Instance.ShowUIForms(SysDefine.UI_UIFormHolographic);
            LiveUIManager.Instance.ShowUIForms(SysDefine.UI_UIFormHololensAgent);
            LiveUIManager.Instance.ShowUIForms(SysDefine.UI_UIFormHololensAgent);
        }

        public void CloseThisUIForm(GameObject obj)
        {
            CloseUIForm();
        }

        public void Exit(GameObject obj)
        {
            Application.Quit();
        }

        public void ShowExitUI(GameObject obj)
        {
            exitApplicationUI.gameObject.SetActive(true);
        }

        private void HideExitUI(GameObject obj)
        {
            exitApplicationUI.gameObject.SetActive(false);
        }



        private void ButtonExitOnEnter(GameObject obj)
        {
            buttonSprite.sprite = hightLightImage;
        }

        private void ButtonExitOnExit(GameObject obj)
        {
            buttonSprite.sprite = normalImage;
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
    }

}
