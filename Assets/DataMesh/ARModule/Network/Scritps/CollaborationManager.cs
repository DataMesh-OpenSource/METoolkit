using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using MEHoloClient.Queue;
using MEHoloClient.Entities;
using MEHoloClient.Interface.Sync;
using MEHoloClient.Sync;
using MEHoloClient.Sync.Time;
using MEHoloClient.Utils;

#if UNITY_METRO && !UNITY_EDITOR
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
#endif

namespace DataMesh.AR.Network
{
    /// <summary>
    /// 与服务器房间连接的控制类 
    /// </summary>
    public class CollaborationManager : DataMesh.AR.MEHoloModuleSingleton<CollaborationManager>
    {
        /// <summary>
        /// 房间名称 
        /// </summary>
        public string roomId = "";

        /// <summary>
        /// 所使用的应用名称
        /// </summary>
        public int appId = 1;

        /// <summary>
        /// 用户识别ID
        /// </summary>
        [HideInInspector]
        public string clientId = "";

        /// <summary>
        /// 服务器地址
        /// </summary>
        public string serverHost = "";

        /// <summary>
        /// 服务器端口
        /// </summary>
        [HideInInspector]
        public int serverPort = 80;

        public List<IMessageHandler> messageHandlerList = new List<IMessageHandler>();

        protected string socketUrl;
        protected string socketUrlForSyncTime;

        protected SyncApi syncApi;
        protected SyncClient syncClient;
        protected SyncTimeClient syncTimeClient;

        public System.Action cbEnterRoom;

        public SceneObjects roomInitData;

        public long roomInitTime { get; private set; }

        [HideInInspector]
        public bool hasEnterRoom = false;

        protected override void Awake()
        {
            base.Awake();

        }

        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            CheckMessageQueue();
        }


        /// <summary>
        /// 初始化
        /// </summary>
        protected override void _Init()
        {
            clientId = SystemInfo.deviceUniqueIdentifier;

            clientId = MD5Hash.Hash(Encoding.UTF8.GetBytes(clientId));
            Debug.Log("client=" + clientId);
        }

        protected override void _TurnOn()
        {
            StartEnterRoom();
        }

        protected override void _TurnOff()
        {
            
        }

        /// <summary>
        /// 添加一个消息处理器
        /// </summary>
        /// <param name="handler"></param>
        public void AddMessageHandler(IMessageHandler handler)
        {
            messageHandlerList.Add(handler);
        }

        /// <summary>
        /// 开始进入房间
        /// </summary>
        /// <param name="cb"></param>
        private void StartEnterRoom()
        {

            socketUrl = "ws://" + serverHost + "/holocenter/sync/register/" + appId + "/" + roomId + "/" + clientId;
            socketUrlForSyncTime = "ws://" + serverHost + "/holocenter/sync/time";

            Debug.Log("Prepare To EnterRoom");

            // 先启动对时，之后才进入房间 
            StartCoroutine(SyncTime());
        }

        /// <summary>
        /// 开始同步时间
        /// </summary>
        /// <returns></returns>
        private IEnumerator SyncTime()
        {
            Debug.Log("Start To Sync Time!");
            
            try
            {
                syncTimeClient = new SyncTimeClient(socketUrlForSyncTime);
                syncTimeClient.StartSyncTime();
            }
            catch (Exception e)
            {
                Debug.Log("Sync Time Exception! " + e);
                yield break;
            }

            while (syncTimeClient.Delay == 0)
            {
                Debug.Log("Delay=" + syncTimeClient.Delay);
                yield return new WaitForSecondsRealtime(1);

            }

            Debug.Log("Delay=" + syncTimeClient.Delay);

            //yield return null;

            // 对时完成，开始进入房间 
            SendEnterRoom();
        }

        /// <summary>
        /// 返回同步时间延迟
        /// </summary>
        /// <returns></returns>
        public long GetSyncDelay()
        {
            if(syncTimeClient==null)
            {
                return 0;
            }
            return syncTimeClient.Delay;
        }

#if UNITY_METRO && !UNITY_EDITOR
        private async void SendEnterRoom()
        {
            // 尝试加入房间
            syncApi = new SyncApi("http://" + serverHost + "/holocenter");
            Debug.Log("ip=" + serverHost);
            Debug.Log("app=" + appId + " room=" + roomId + " init=" + roomInitData);

            try
            {
                roomInitTime = await syncApi.EnterAppRoom(appId, roomId, roomInitData);
                ConnectToRoom();
                if (cbEnterRoom != null)
                    cbEnterRoom();
                hasEnterRoom = true;
            }
            catch (Exception e)
            {
                // 失败了！可能需要重连！ 
                Debug.LogError("Enter Room Failed!");
            }
        }
#else
        private void SendEnterRoom()
        {
            // 尝试加入房间
            string url = "http://" + serverHost + "/holocenter";
            Debug.Log("SyncApi url=" + url);
            syncApi = new SyncApi(url);
            Debug.Log("ip=" + serverHost);
            Debug.Log("app=" + appId + " room=" + roomId + " init=" + roomInitData);

            try
            {
                roomInitTime = syncApi.EnterAppRoom(appId, roomId, roomInitData);
                ConnectToRoom();
                if (cbEnterRoom != null)
                    cbEnterRoom();
                hasEnterRoom = true;
            }
            catch (Exception e)
            {
                // 失败了！可能需要重连！ 
                Debug.LogError("Enter Room Failed! " + e);
            }
        }

        void OnApplicationQuit()
        {
            if (syncClient != null)
            {
                syncClient.Stop(true);
                syncClient.Terminate();
            }

            if (syncTimeClient != null)
            {
                syncTimeClient.Terminate();
            }
        }

#endif

        /// <summary>
        /// 建立长连接
        /// </summary>
        private void ConnectToRoom()
        {
            syncClient = new SyncClient(socketUrl);
            syncClient.StartClient();

            Debug.Log("Start to connect [" + socketUrl + "]");
        }

        /// <summary>
        /// 发送房间同步消息
        /// </summary>
        /// <param name="msgEntries"></param>
        public void SendMessage(MsgEntry[] msgEntries)
        {
            if (syncClient == null)
                return;

            if (!syncClient.Running)
                return;

            SyncMsg msg = new SyncMsg(false, 0L) { msg_entry = msgEntries };
            SyncProto syncProto = new SyncProto(TimeUtil.ConvertToUnixTime(DateTime.Now), msg);

            string msgData = JsonUtil.Serialize(syncProto, false, false);

#if UNITY_METRO && !UNITY_EDITOR
            syncClient.SendMessage(msgData);
#else
            syncClient.SendMessage(msgData);
#endif
        }

        /// <summary>
        /// 检查消息队列，将消息分发
        /// </summary>
        protected void CheckMessageQueue()
        {
            if (syncClient != null)
            {
                if (syncClient.Running)
                {
                    while (syncClient.SyncQueue.GetCount() > 0)
                    {
                        byte[] messageBytes = syncClient.SyncQueue.Dequeue();

                        string json = Encoding.UTF8.GetString(messageBytes);
                        //Debug.Log(json);

                        SyncProto proto = JsonUtil.Deserialize<SyncProto>(json);
                        //Debug.Log("Msg[" + proto.msg_id + "]  objs:" + proto.sync_msg.msg_entry);

                        // 处理消息 
                        DealMessage(proto);
                    }
                }
            }
        }

        /// <summary>
        /// 将消息转发给所有的Handler
        /// </summary>
        /// <param name="proto"></param>
        protected virtual void DealMessage(SyncProto proto)
        {
            for (int i = 0; i < messageHandlerList.Count; i++)
            {
                messageHandlerList[i].DealMessage(proto);
            }
        }
    }

}