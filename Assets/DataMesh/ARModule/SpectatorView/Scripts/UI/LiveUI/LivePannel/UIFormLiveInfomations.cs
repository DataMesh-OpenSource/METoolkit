using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DataMesh.AR.SpectatorView
{
    public class UIFormLiveInfomations : BaseUIForm
    {

        public GameObject infomationWindow;
        public Text textInfomationsOutputWindow;
        public Text textInfomationTitle;
        public Button buttonCloseInformation;

        private float delayTime = 10f;

        private void Awake()
        {
            RigisterButtonEvent(buttonCloseInformation, ShowOrHideWindow);
            ReceiveMessage(SysDefine.MESSAGE_Infomation, SetInfomation);
            if (infomationWindow.activeSelf)
            {
                ShowOrHideWindow(this.gameObject);
            }
        }

        private IEnumerator WaitAndClear()
        {
            yield return new WaitForSeconds(delayTime);
            textInfomationsOutputWindow.text = "";
            textInfomationTitle.color = Color.white;
            textInfomationsOutputWindow.color = Color.white;
        }

        public void ShowOrHideWindow(GameObject obj)
        {
            if (infomationWindow.activeSelf)
            {
                infomationWindow.gameObject.SetActive(false);
                buttonCloseInformation.transform.localPosition = new Vector3(-132,-80, 0);
            }
            else
            {
                infomationWindow.gameObject.SetActive(true);
                buttonCloseInformation.transform.localPosition = new Vector3(26, 24, 0);
            }
        }

        public void SetInfomation(KeyValueUpdate kv)
        {
            string infomationType = kv.Key;
            string information = (string)kv.Value;
            SetInfomationText(infomationType, information);
        }

        private void SetInfomationText(string infomationType, string infomationText)
        {
            if (infomationType == SysDefine.MESSAGE_InfomationTypeNormal)
            {
                textInfomationTitle.color = Color.white;
                textInfomationsOutputWindow.color = Color.white;
                textInfomationsOutputWindow.text = infomationText;

            }
            else if (infomationType == SysDefine.MESSAGE_InfomationTypeError)
            {
                textInfomationTitle.color = Color.red;
                textInfomationsOutputWindow.color = Color.red;
                textInfomationsOutputWindow.text = infomationText;
            }
            StartCoroutine(WaitAndClear());
        }

    }
}

