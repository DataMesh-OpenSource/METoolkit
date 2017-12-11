using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DataMesh.AR.Utility;

using MEHoloClient.Queue;
using MEHoloClient.Entities;
using MEHoloClient.Core.Entities;
using MEHoloClient.Proto;
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
    public enum CollaborationMessageType
    {
        ProtoBuf,
        JSON
    }

    public enum EnterRoomResult
    {
        Waiting,
        EnterRoomSuccess,
        LicenseMissing,
        LicenseExpired,
        RoomNotExist,
        DeviceReachesLimit,
        GameHasStarted,
        SocketCreateError,
        UnknownError
    }

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
        public int serverPort;

        public CollaborationMessageType messageType = CollaborationMessageType.ProtoBuf;

        public List<IMessageHandler> messageHandlerList = new List<IMessageHandler>();

        protected string socketUrl;
        protected string socketUrlForSyncTime;

        protected SyncApi syncApi;
        protected SyncClient syncClient;
        protected SyncTimeClient syncTimeClient;

        public System.Action cbEnterRoom;

        public SceneObject roomInitData;

        public EnterRoomResult enterRoomResult { get; private set; }
        public string errorString { get; private set; }
        public long roomInitTime { get; private set; }

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
            Debug.Log("client=" + clientId);

            clientId = MD5Hash.Hash(Encoding.UTF8.GetBytes(clientId));
            Debug.Log("client=" + clientId);

            enterRoomResult = EnterRoomResult.Waiting;
        }

        protected override void _TurnOn()
        {
            AppConfig config = AppConfig.Instance;
            config.LoadConfig(MEHoloConstant.NetworkConfigFile);
            serverHost = AppConfig.Instance.GetConfigByFileName(MEHoloConstant.NetworkConfigFile, "Server_Host", "127.0.0.1");
            serverPort = int.Parse(AppConfig.Instance.GetConfigByFileName(MEHoloConstant.NetworkConfigFile, "Server_Port", "8848"));

            roomId = RoomManager.Instance.GetCurrentRoom();

            StartEnterRoom();
        }

        protected override void _TurnOff()
        {
            
        }

        public bool IsValid()
        {
            if (syncClient == null)
                return false;

            return syncClient.Running;
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
            socketUrl = "ws://" + serverHost + ":"+ serverPort.ToString() + "/me/live/register/" + MEHoloEntrance.Instance.AppID + "/" + roomId + "/" + clientId;
            socketUrlForSyncTime = "ws://" + serverHost +":"+ serverPort.ToString() + "/me/live/time";

            Debug.Log("Sync Time Url=" + socketUrlForSyncTime);

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

        /// <summary>
        /// 重新尝试进入房间 
        /// </summary>
        /// <param name="delayTime"></param>
        public void ReEnterRoom(float delayTime)
        {
            if　(enterRoomResult == EnterRoomResult.Waiting || enterRoomResult == EnterRoomResult.EnterRoomSuccess)
            {
                return;
            }

            enterRoomResult = EnterRoomResult.Waiting;

            StartCoroutine(ReconnectRoom(delayTime));
        }

        private IEnumerator ReconnectRoom(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            SendEnterRoom();
        }

        private async void SendEnterRoom()
        {
            enterRoomResult = EnterRoomResult.Waiting;

            // 尝试加入房间
            syncApi = new SyncApi("http://" + serverHost +":"+ serverPort.ToString() + "");
            Debug.Log("ip=" + serverHost);
            Debug.Log("app=" + MEHoloEntrance.Instance.AppID + " room=" + roomId + " init=" + roomInitData);

            QueryRoomResponse enterRoomResponse = null;
            try
            {
                enterRoomResponse = await syncApi.EnterAppRoom(MEHoloEntrance.Instance.AppID, roomId, roomInitData);
            }
            catch (Exception e)
            {
                Debug.LogError("Enter Room Failed! " + e);
                errorString = e.ToString();
            }

            if (enterRoomResponse != null)
            {
                if (enterRoomResponse.CanEnterAppRoom)
                {
                    try
                    {
                        ConnectToRoom();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Connect Room ws Failed! " + e);
                        enterRoomResult = EnterRoomResult.SocketCreateError;
                        errorString = e.ToString();
                    }

                    roomInitTime = enterRoomResponse.InitRoomTime;
                    enterRoomResult = EnterRoomResult.EnterRoomSuccess;

                    if (cbEnterRoom != null)
                        cbEnterRoom();
                }
                else
                {
                    switch (enterRoomResponse.Code)
                    {
                        case 6001:      // Missing License
                            enterRoomResult = EnterRoomResult.LicenseMissing;
                            break;
                        case 6002:      // license expired
                            enterRoomResult = EnterRoomResult.LicenseExpired;
                            break;
                        case 6003:      // Room not exist
                            enterRoomResult = EnterRoomResult.RoomNotExist;
                            break;
                        case 6004:      // Device limit
                            enterRoomResult = EnterRoomResult.DeviceReachesLimit;
                            break;
                        case 6005:      // Game has started
                            enterRoomResult = EnterRoomResult.GameHasStarted;
                            break;
                        default:        // Unknown error
                            Debug.Log("error?? " + enterRoomResponse.Code);
                            enterRoomResult = EnterRoomResult.UnknownError;
                            break;
                    }

                    errorString = enterRoomResponse.Message;

                    Debug.LogWarning("Error String : " + errorString);
                }

            }
            else
            {
                enterRoomResult = EnterRoomResult.UnknownError;
            }
        }

#if UNITY_METRO && !UNITY_EDITOR
#else
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
        public bool SendMessage(SyncMsg msg)
        {
            if (syncClient == null)
                return false;

            if (!syncClient.Running)
                return false;

            SyncProto syncProto = new SyncProto();
            syncProto.SyncMsg = msg;

            if (messageType == CollaborationMessageType.JSON)
            {
                string msgData = JsonUtil.Serialize(syncProto, false, false);
                syncClient.SendMessage(msgData);
            }
            else if (messageType == CollaborationMessageType.ProtoBuf)
            {
                syncClient.SendMessage(syncProto.ToByteArray());
            }
            //Debug.Log("Send Sync: " + syncProto);

            return true;
        }

        public bool SendCommand(BroadcastMsg msg)
        {
            if (syncClient == null)
                return false;

            if (!syncClient.Running)
                return false;

            SyncProto syncProto = new SyncProto();
            syncProto.BrdMsg = msg;

            if (messageType == CollaborationMessageType.JSON)
            {
                string msgData = JsonUtil.Serialize(syncProto, false, false);
                syncClient.SendMessage(msgData);
            }
            else if (messageType == CollaborationMessageType.ProtoBuf)
            {
                syncClient.SendMessage(syncProto.ToByteArray());
            }

            //Debug.Log("Send Command: " + syncProto);

            return true;
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

                        SyncProto proto = null;

                        if (messageType == CollaborationMessageType.JSON)
                        {
                            string json = Encoding.UTF8.GetString(messageBytes);
                            //Debug.Log(json);

                            proto = JsonUtil.Deserialize<SyncProto>(json);
                            //Debug.Log("Msg[" + proto.msg_id + "]  objs:" + proto.sync_msg.msg_entry);
                        }
                        else
                        {
                            proto = SyncProto.Parser.ParseFrom(messageBytes);
                        }

                        //Debug.Log("[Receive] " + proto);

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