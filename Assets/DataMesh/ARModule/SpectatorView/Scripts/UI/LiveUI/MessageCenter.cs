using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataMesh.AR.SpectatorView
{
    public static class MessageCenter
    {

        public delegate void DealMessageDelivery(KeyValueUpdate keyValue);
        public static Dictionary<string, DealMessageDelivery> dicMessage = new Dictionary<string, DealMessageDelivery>();
        /// <summary>
        /// 添加消息监听
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="handler"></param>
        public static void AddMsgListener(string messageType, DealMessageDelivery handler)
        {
            if (!dicMessage.ContainsKey(messageType))
            {
                dicMessage.Add(messageType, null);
            }
            dicMessage[messageType] += handler;
        }

        public static void RemoveMsgListener(string messageType, DealMessageDelivery handler)
        {
            if (dicMessage.ContainsKey(messageType))
                dicMessage.Remove(messageType);
        }

        public static void ClearMsgListener()
        {
            if (dicMessage != null)
                dicMessage.Clear();
        }

        public static void SendMessage(string messageType, KeyValueUpdate kv)
        {
            DealMessageDelivery del;
            if (dicMessage.TryGetValue(messageType, out del))
            {
                if (del != null)
                {
                    del(kv);
                }
            }
        }
    }

    public class KeyValueUpdate
    {
        private string _Key;
        private object _Value;

        public string Key
        {
            get { return _Key; }
        }

        public object Value
        {
            get { return _Value; }
        }

        public KeyValueUpdate(string key, object valueObj)
        {
            _Key = key;
            _Value = valueObj;
        }
    }
}


