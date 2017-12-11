using DataMesh.AR.Event;
using DataMesh.AR.SpectatorView;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DataMesh.AR.SpectatorView
{
    public class UIFormFunction : BaseUIForm
    {

        public Button buttonSetting;
        public Button buttonMoveAnchor;

        public Button buttonStartFollow;
        public Button buttonConnect;
        public Button buttonStopFollow;

        public Sprite spriteStartFollow;
        public Sprite spriteStartFollowUnClick;
        public Sprite spriteStartFollowHightLight;

        private LiveController liveController;
        private bool hasConnected = false;
        private bool hasSync = false;
        private bool hasInit = false;
        private Image startFollowImage;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        private void Awake()
        {
            RigisterButtonEvent(buttonSetting, OpenSettingPannel);
            RigisterButtonEvent(buttonMoveAnchor, OpenMoveAnchorPannel);
            RigisterButtonEvent(buttonStartFollow, StartFollow);
            RigisterButtonEvent(buttonConnect, Connect);
            RigisterButtonEvent(buttonStopFollow, StopFollow);

            ETListener.Get(buttonStartFollow.gameObject).onEnter = ButtonStartFollowOnEnter;
            ETListener.Get(buttonStartFollow.gameObject).onExit = ButtonStartFollowOnExit;

            OpenUIForm(SysDefine.UI_UIFormLiveInfomation);
            startFollowImage = buttonStartFollow.GetComponent<Image>();
        }

        public override void Init(LiveController b, LiveControllerUI pannel)
        {
            liveController = b;
            OnStartHololensConnect();
            hasInit = true;
        }

        private void Update()
        {
            if (!hasInit)
                return;
            if (liveController.hololensStartSynchronize)
            {
                buttonStartFollow.gameObject.SetActive(false);
                buttonConnect.gameObject.SetActive(false);
                buttonStopFollow.gameObject.SetActive(true);
            }
            else
            {
                buttonStartFollow.gameObject.SetActive(true);
                buttonConnect.gameObject.SetActive(false);
                buttonStopFollow.gameObject.SetActive(false);
            }

            if (liveController.hololensConnected)
            {
                if (!hasConnected)
                {
                    hasConnected = true;
                }
                if (liveController.waiting)
                {
                    buttonStartFollow.interactable = false;
                    buttonStopFollow.interactable = false;

                    buttonStartFollow.gameObject.SetActive(false);
                    buttonStopFollow.gameObject.SetActive(false);
                    buttonConnect.gameObject.SetActive(true);
                }
                else
                {
                    if (liveController.hololensStartSynchronize)
                    {
                        hasSync = true;
                        buttonStartFollow.interactable = false;
                        buttonStartFollow.gameObject.SetActive(false);
                        buttonStopFollow.interactable = true;
                        buttonStopFollow.gameObject.SetActive(true);
                        buttonConnect.gameObject.SetActive(false);
                    }
                    else
                    {
                        hasSync = false;
                        buttonStartFollow.gameObject.SetActive(true);
                        buttonStartFollow.interactable = true;
                        buttonConnect.gameObject.SetActive(false);
                        buttonStopFollow.gameObject.SetActive(false);
                        buttonStopFollow.interactable = false;
                    }
                    if (liveController.waitingString != null)
                        SendMessage(SysDefine.MESSAGE_Infomation, SysDefine.MESSAGE_InfomationTypeNormal, liveController.waitingString);
                }
            }
            else
            {
                if (hasConnected)
                {
                    hasConnected = false;
                }
                buttonStartFollow.gameObject.SetActive(true);
                buttonConnect.gameObject.SetActive(false);
                buttonStartFollow.interactable = false;
                buttonStopFollow.interactable = false;
                buttonStopFollow.gameObject.SetActive(false);
            }

        }

        private void OpenSettingPannel(GameObject obj)
        {
            OpenUIForm(SysDefine.UI_UIFormSetting);
        }

        private void OpenMoveAnchorPannel(GameObject obj)
        {
            OpenUIForm(SysDefine.UI_UIFormAnchorControl);
        }

        private void OnStartHololensConnect()
        {
            LiveController.Instance.StartHololensServer();
        }

        private void StartFollow(GameObject obj)
        {
            liveController.SetHololensSynchronize(true);
        }

        private void Connect(GameObject obj)
        {

        }

        private void StopFollow(GameObject obj)
        {
            liveController.SetHololensSynchronize(false);
        }

        private void ButtonStartFollowOnEnter(GameObject obj)
        {
            //startFollowImage.sprite = spriteStartFollowHightLight;
        }

        private void ButtonStartFollowOnExit(GameObject obj)
        {
            //startFollowImage.sprite = spriteStartFollow;
        }
#endif
    }
}


