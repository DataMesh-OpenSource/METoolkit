using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEHoloClient.Utils;

public class LiveMessageConstant
{ 
    public const int BEV_MESSAGE_TYPE_START = 1;
    public const int BEV_MESSAGE_TYPE_STOP = 2;
    public const int BEV_MESSAGE_TYPE_SET_ANCHOR = 3;
    public const int BEV_MESSAGE_TYPE_DOWNLOAD_ANCHOR = 4;
    public const int BEV_MESSAGE_TYPE_DOWNLOAD_ANCHOR_FINISH = 5;
    public const int BEV_MESSAGE_TYPE_SET_ANCHOR_FINISH = 6;
    public const int BEV_MESSAGE_TYPE_SAVE_ANCHOR_FINISH = 7;
    public const int BEV_MESSAGE_TYPE_SAVE_ANCHOR = 8;

    public const int BEV_MESSAGE_TYPE_SYNCHRONIZE_CAMERA = 11;
    public const int BEV_MESSAGE_TYPE_SYNCHRONIZE_ANCHOR = 12;
    public const int BEV_MESSAGE_TYPE_SYNCHRONIZE_ALL = 13;

}


public class LiveMessageManager
{

    public static LiveMessage ParseMessage(byte[] bytes)
    {
        LiveMessage msg = null;
        switch (bytes[0])
        {
            case LiveMessageConstant.BEV_MESSAGE_TYPE_START:
                msg = new LiveMessageStart();
                break;
            case LiveMessageConstant.BEV_MESSAGE_TYPE_STOP:
                msg = new LiveMessageStop();
                break;
            case LiveMessageConstant.BEV_MESSAGE_TYPE_DOWNLOAD_ANCHOR:
                msg = new LiveMessageDownload();
                break;
            case LiveMessageConstant.BEV_MESSAGE_TYPE_SET_ANCHOR:
                msg = new LiveMessageSetAnchor();
                break;
            case LiveMessageConstant.BEV_MESSAGE_TYPE_SYNCHRONIZE_ANCHOR:
                msg = new LiveMessageSynchronizeAnchor();
                break;
            case LiveMessageConstant.BEV_MESSAGE_TYPE_SYNCHRONIZE_CAMERA:
                msg = new LiveMessageSynchronizeCamera();
                break;
            case LiveMessageConstant.BEV_MESSAGE_TYPE_SYNCHRONIZE_ALL:
                msg = new LiveMessageSynchronizeAll();
                break;
            case LiveMessageConstant.BEV_MESSAGE_TYPE_DOWNLOAD_ANCHOR_FINISH:
                msg = new LiveMessageDownloadFinish();
                break;
            case LiveMessageConstant.BEV_MESSAGE_TYPE_SET_ANCHOR_FINISH:
                msg = new LiveMessageSetAnchorFinish();
                break;
            case LiveMessageConstant.BEV_MESSAGE_TYPE_SAVE_ANCHOR:
                msg = new LiveMessageSaveAnchor();
                break;
        }

        msg.Deserialize(bytes);
        return msg;
    }

    public static void WriteVectorToBytes(byte[] data, Vector3 vector, ref int index)
    {
        byte[] floatBytes;

        floatBytes = BitConverter.GetBytes(vector.x);
        Array.Copy(floatBytes, 0, data, index, floatBytes.Length);
        index += floatBytes.Length;

        floatBytes = BitConverter.GetBytes(vector.y);
        Array.Copy(floatBytes, 0, data, index, floatBytes.Length);
        index += floatBytes.Length;

        floatBytes = BitConverter.GetBytes(vector.z);
        Array.Copy(floatBytes, 0, data, index, floatBytes.Length);
        index += floatBytes.Length;

    }

    public static void GetVectorFromBytes(byte[] data, out Vector3 vector, ref int index)
    {
        vector = new Vector3();

        vector.x = BitConverter.ToSingle(data, index);
        index += 4;

        vector.y = BitConverter.ToSingle(data, index);
        index += 4;

        vector.z = BitConverter.ToSingle(data, index);
        index += 4;
    }
}

public class LiveMessage
{
    public int type;

    public virtual byte[] Serialize()
    {
        byte[] rs = new byte[1];
        rs[0] = (byte)type;

        return rs;
    }

    public virtual void Deserialize(byte[] data)
    {
        type = data[0];
    }


}

public class LiveMessageStart : LiveMessage
{
    public LiveMessageStart()
    {
        type = LiveMessageConstant.BEV_MESSAGE_TYPE_START;
    }
}

public class LiveMessageStop : LiveMessage
{
    public LiveMessageStop()
    {
        type = LiveMessageConstant.BEV_MESSAGE_TYPE_STOP;
    }
}

public class LiveMessageSetAnchorFinish : LiveMessage
{
    public short version = 0;
    public bool isOld = false;

    public LiveMessageSetAnchorFinish()
    {
        type = LiveMessageConstant.BEV_MESSAGE_TYPE_SET_ANCHOR_FINISH;
    }

    public override byte[] Serialize()
    {
        byte[] rs = new byte[4 * 6 + 3];

        rs[0] = (byte)type;

        int index = 1;

        byte[] floatBytes = BitConverter.GetBytes(version);
        Array.Copy(floatBytes, 0, rs, index, floatBytes.Length);
        index += floatBytes.Length;


        return rs;

    }

    public override void Deserialize(byte[] data)
    {
        type = data[0];

        if (data.Length == 1)
        {
            isOld = true;
        }
        else
        {
            isOld = false;

            int index = 1;

            version = BitConverter.ToInt16(data, index);
            index += 2;
        }

    }

}



/// <summary>
/// hololens端向pc端同步摄像机位置的消息
/// </summary>
public class LiveMessageSynchronizeCamera : LiveMessage
{
    public Vector3 position;
    public Vector3 rotation;

    public LiveMessageSynchronizeCamera()
    {
        type = LiveMessageConstant.BEV_MESSAGE_TYPE_SYNCHRONIZE_CAMERA;
    }

    public override byte[] Serialize()
    {
        byte[] rs = new byte[4 * 6 + 1];

        rs[0] = (byte)type;

        int index = 1;

        LiveMessageManager.WriteVectorToBytes(rs, position, ref index);
        LiveMessageManager.WriteVectorToBytes(rs, rotation, ref index);

        return rs;

    }

    public override void Deserialize(byte[] data)
    {
        type = data[0];

        int index = 1;
        LiveMessageManager.GetVectorFromBytes(data, out position, ref index);
        LiveMessageManager.GetVectorFromBytes(data, out rotation, ref index);
    }

}

/// <summary>
/// hololens端向pc端同步anchor的消息
/// </summary>
public class LiveMessageSynchronizeAnchor : LiveMessage
{
    public int anchorCount;
    public List<Vector3> anchorPositionList = new List<Vector3>();
    public List<Vector3> anchorRotationList = new List<Vector3>();

    public LiveMessageSynchronizeAnchor()
    {
        type = LiveMessageConstant.BEV_MESSAGE_TYPE_SYNCHRONIZE_ANCHOR;
    }

    public override byte[] Serialize()
    {
        byte[] rs = new byte[anchorCount * 4 * 6 + 2];

        rs[0] = (byte)type;
        rs[1] = (byte)anchorCount;

        int index = 2;

        for (int i = 0;i < anchorCount;i ++)
        {
            LiveMessageManager.WriteVectorToBytes(rs, anchorPositionList[i], ref index);
            LiveMessageManager.WriteVectorToBytes(rs, anchorRotationList[i], ref index);
        }

        return rs;

    }

    public override void Deserialize(byte[] data)
    {
        type = data[0];
        anchorCount = data[1];

        int index = 2;
        anchorPositionList.Clear();
        anchorRotationList.Clear();

        for (int i = 0; i < anchorCount; i++)
        {
            Vector3 pos, rot;
            LiveMessageManager.GetVectorFromBytes(data, out pos, ref index);
            LiveMessageManager.GetVectorFromBytes(data, out rot, ref index);

            anchorPositionList.Add(pos);
            anchorRotationList.Add(rot);
        }
    }

}

public class LiveMessageSynchronizeAll : LiveMessage
{
    public Vector3 position;
    public Vector3 rotation;
    public int anchorCount;
    public Vector3[] anchorPositionList;
    public Vector3[] anchorRotationList;
    public bool[] anchorIsLocated;

    public LiveMessageSynchronizeAll()
    {
        type = LiveMessageConstant.BEV_MESSAGE_TYPE_SYNCHRONIZE_ALL;
    }

    public override byte[] Serialize()
    {
        byte[] rs = new byte[2 + 4 * 6 + anchorCount * (4 * 6) + anchorCount];

        rs[0] = (byte)type;
        rs[1] = (byte)anchorCount;

        int index = 2;

        LiveMessageManager.WriteVectorToBytes(rs, position, ref index);
        LiveMessageManager.WriteVectorToBytes(rs, rotation, ref index);

        for (int i = 0; i < anchorCount; i++)
        {
            LiveMessageManager.WriteVectorToBytes(rs, anchorPositionList[i], ref index);
            LiveMessageManager.WriteVectorToBytes(rs, anchorRotationList[i], ref index);

        }

        for (int i = 0; i < anchorCount; i++)
        {
            if (anchorIsLocated[i])
                rs[index] = 1;
            else
                rs[index] = 0;
            index++;

        }

        return rs;

    }

    public override void Deserialize(byte[] data)
    {
        type = data[0];
        anchorCount = data[1];


        int index = 2;
        LiveMessageManager.GetVectorFromBytes(data, out position, ref index);
        LiveMessageManager.GetVectorFromBytes(data, out rotation, ref index);

        anchorPositionList = new Vector3[anchorCount];
        anchorRotationList = new Vector3[anchorCount];
        anchorIsLocated = new bool[anchorCount];

        for (int i = 0; i < anchorCount; i++)
        {
            Vector3 pos, rot;
            LiveMessageManager.GetVectorFromBytes(data, out pos, ref index);
            LiveMessageManager.GetVectorFromBytes(data, out rot, ref index);

            anchorPositionList[i] = pos;
            anchorRotationList[i] = rot;
        }

        // 如果还有内容，则继续读取。兼容老数据 
        if (data.Length > index)
        {
            for (int i = 0; i < anchorCount; i++)
            {
                byte isLocate = data[index];
                index++;

                if (isLocate > 0)
                    anchorIsLocated[i] = true;
                else
                    anchorIsLocated[i] = false;
            }
        }
        else
        {
            for (int i = 0; i < anchorCount; i++)
            {
                anchorIsLocated[i] = true;
            }
        }
    }
}


/// <summary>
/// json类型消息的基类
/// </summary>
public class LiveMessageByJson : LiveMessage
{
    protected byte[] SerializeJson<T>(T jsonData)
    {
        string json = JsonUtil.Serialize(jsonData, false, false);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

        byte[] rs = new byte[jsonBytes.Length + 1];
        rs[0] = (byte)type;
        Array.Copy(jsonBytes, 0, rs, 1, jsonBytes.Length);

        return rs;
    }

    protected T DeserializeJson<T>(byte[] data)
    {
        type = data[0];
        if (data.Length <= 1)
            return default(T);

        byte[] jsonBytes = new byte[data.Length - 1];
        Array.Copy(data, 1, jsonBytes, 0, data.Length - 1);

        string json = Encoding.UTF8.GetString(jsonBytes);

        T rs = JsonUtil.Deserialize<T>(json);

        return rs;
    }
}



/// <summary>
/// 在hololens端设置初始化Anchor信息
/// </summary>
public class LiveMessageSetAnchor : LiveMessageByJson
{
    public class LiveMessageSetAnchorData
    {
        public string serverHost;
        public int serverPort;
        public int appId;
        public string roomId;
        public bool isInit;
        public List<string> anchorNameList = new List<string>();
        public List<Vector3InMessage> anchorPosition = new List<Vector3InMessage>();
        public List<Vector3InMessage> anchorForward = new List<Vector3InMessage>();
    }

    public LiveMessageSetAnchorData anchorData = new LiveMessageSetAnchorData();

    public LiveMessageSetAnchor()
    {
        type = LiveMessageConstant.BEV_MESSAGE_TYPE_SET_ANCHOR;
    }

    public override byte[] Serialize()
    {
        return SerializeJson(anchorData);
    }

    public override void Deserialize(byte[] data)
    {
        anchorData = DeserializeJson<LiveMessageSetAnchorData>(data);
    }
}

/// <summary>
/// 在hololens端设置初始化Anchor信息
/// </summary>
public class LiveMessageSaveAnchor : LiveMessageByJson
{
    public class LiveMessageSetAnchorData
    {
        public string serverHost;
        public int serverPort;
        public int appId;
        public string roomId;
        public List<string> anchorNameList = new List<string>();
        public List<Vector3InMessage> anchorPosition = new List<Vector3InMessage>();
        public List<Vector3InMessage> anchorForward = new List<Vector3InMessage>();
    }

    public LiveMessageSetAnchorData anchorData = new LiveMessageSetAnchorData();

    public LiveMessageSaveAnchor()
    {
        type = LiveMessageConstant.BEV_MESSAGE_TYPE_SAVE_ANCHOR;
    }

    public override byte[] Serialize()
    {
        return SerializeJson(anchorData);
    }

    public override void Deserialize(byte[] data)
    {
        anchorData = DeserializeJson<LiveMessageSetAnchorData>(data);
    }
}

public class LiveMessageDownload : LiveMessage
{
    public LiveMessageDownload()
    {
        type = LiveMessageConstant.BEV_MESSAGE_TYPE_DOWNLOAD_ANCHOR;
    }
}

public class LiveMessageResult : LiveMessageByJson
{
    public class ResultData
    {
        public bool success;
        public string errorString;
    }

    public ResultData result = new ResultData();


    public override byte[] Serialize()
    {
        return SerializeJson(result);
    }

    public override void Deserialize(byte[] data)
    {
        result = DeserializeJson<ResultData>(data);
    }
}

/// <summary>
/// 下载Anchor的结果
/// </summary>
public class LiveMessageDownloadFinish : LiveMessageResult
{
    public LiveMessageDownloadFinish()
    {
        type = LiveMessageConstant.BEV_MESSAGE_TYPE_DOWNLOAD_ANCHOR_FINISH;
    }
}

/// <summary>
/// 存储Anchor的结果 
/// </summary>
public class LiveMessageSaveAnchorFinish : LiveMessageResult
{
    public LiveMessageSaveAnchorFinish()
    {
        type = LiveMessageConstant.BEV_MESSAGE_TYPE_SAVE_ANCHOR_FINISH;
    }
}



//========================


public class Vector3InMessage
{
    public float x;
    public float y;
    public float z;


    static public implicit operator Vector3InMessage(Vector3 v)
    {
        Vector3InMessage rs = new Vector3InMessage();
        rs.x = v.x;
        rs.y = v.y;
        rs.z = v.z;
        return rs;
    }

    public Vector3 ToVector3()
    {
        Vector3 rs = new Vector3();
        rs.x = this.x;
        rs.y = this.y;
        rs.z = this.z;
        return rs;
    }
}


