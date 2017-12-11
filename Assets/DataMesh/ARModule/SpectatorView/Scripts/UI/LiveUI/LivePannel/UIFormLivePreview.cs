using DataMesh.AR.SpectatorView;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DataMesh.AR.SpectatorView
{

    public enum ScreenType
    {
        Normal,
        FullScreen,
        Hide,
    }

    public class UIFormLivePreview : BaseUIForm
    {

        public Button buttonFullScreen;
        public LivePreview livePreview;

        private ScreenType screenType = ScreenType.Normal;
        private bool isFullScreen = false;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        private void Awake()
        {
            RigisterButtonEvent(buttonFullScreen, ButtonScreen);
        }

        public override void Init()
        {

        }


        private void Update()
        {

        }

        public void ButtonScreen(GameObject obj)
        {
            SetScreen();
        }


        private void SetScreen()
        {
            if (screenType == ScreenType.Normal)
            {
                livePreview.SetFullScreen(true);
                screenType = ScreenType.FullScreen;
                LiveUIManager.Instance.CloseUIForm(SysDefine.UI_UIFormFunction);
                LiveUIManager.Instance.CloseUIForm(SysDefine.UI_UIFormLiveInfomation);

            }
            else if (screenType == ScreenType.FullScreen)
            {
                livePreview.gameObject.SetActive(false);
                screenType = ScreenType.Hide;
                LiveUIManager.Instance.ShowUIForms(SysDefine.UI_UIFormFunction);
                LiveUIManager.Instance.ShowUIForms(SysDefine.UI_UIFormLiveInfomation);

            }
            else if (screenType == ScreenType.Hide)
            {
                livePreview.SetFullScreen(false);
                livePreview.gameObject.SetActive(true);
                screenType = ScreenType.Normal;
            }

        }

#endif
    }
}

