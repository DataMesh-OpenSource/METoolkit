using DataMesh.AR.Event;
using DataMesh.AR.SpectatorView;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DataMesh.AR.SpectatorView
{
    public class BaseUIForm : MonoBehaviour
    {
        
        private LiveUIType liveUIType = new LiveUIType();

        public LiveUIType CurrentUIType
        {
            get { return liveUIType; }
            set { liveUIType = value; }
        }

        #region 窗体的声明周期

        public virtual void Init()
        {
        }
        public virtual void Init(LiveController b, LiveControllerUI ui)
        {
        }

        public virtual void Display()
        {
            this.gameObject.SetActive(true);
        }

        public virtual void Hiding()
        {
            this.gameObject.SetActive(false);
        }

        public virtual void Freeze()
        {
            Debug.Log("冻结");
        }

        #endregion

        #region 子类常用方法

        protected void RigisterButtonEvent(Button button, ETListener.VoidDelegate delHandle)
        {
            if (button != null)
                ETListener.Get(button.gameObject).onClick = delHandle;
        }

        protected void SendMessage(string MESSAGE_Type, string key, object value)
        {
            KeyValueUpdate kvs = new KeyValueUpdate(key, value);
            MessageCenter.SendMessage(MESSAGE_Type, kvs);
        }
        /// <summary>
        /// 注册，接收消息
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="handler"></param>
        protected void ReceiveMessage(string messageType, MessageCenter.DealMessageDelivery handler)
        {
            MessageCenter.AddMsgListener(messageType, handler);
        }

        protected void OpenUIForm(string uiFormName)
        {
            LiveUIManager.Instance.ShowUIForms(uiFormName);
        }

        public void CloseUIForm()
        {
            string strUIFormName = string.Empty;
            int intPosition = -1;
            strUIFormName = GetType().ToString();
            intPosition = strUIFormName.IndexOf('.');
            if (intPosition != -1)
            {
                strUIFormName = strUIFormName.Substring(intPosition + 1);
                strUIFormName = strUIFormName.Replace("AR.SpectatorView.","");
            }
            LiveUIManager.Instance.CloseUIForm(strUIFormName);
        }

        #endregion

    }
}

