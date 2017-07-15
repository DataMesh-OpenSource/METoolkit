using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEHoloClient.Utils;
using DataMesh.AR.Utility;

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
    public const int BEV_MESSAGE_TYPE_SYNCHRONIZE_ALL_WITH_ROTATION = 14;

    public const int BEV_MESSAGE_TYPE_REQUEST_SPATIAL_MAPPING = 21;
    public const int BEV_MESSAGE_TYPE_RESPONSE_SPATIAL_MAPPING = 22;

}


public class LiveMessageManager
{

    public static LiveMessage ParseMessage(byte[] bytes)
    {
        LiveMessage msg = null;

        if (bytes.Length == 0)
        {
            Debug.LogError("Error: Message length=0!");
            return msg;
        }

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
            case LiveMessageConstant.BEV_MESSAGE_TYPE_SAVE_ANCHOR_FINISH:
                msg = new LiveMessageSaveAnchorFinish();
                break;
            case LiveMessageConstant.BEV_MESSAGE_TYPE_REQUEST_SPATIAL_MAPPING:
                msg = new LiveMessageRequestSpatialMapping();
                break;
            case LiveMessageConstant.BEV_MESSAGE_TYPE_RESPONSE_SPATIAL_MAPPING:
                msg = new LiveMessageResponseSpatialMapping();
                break;
            default:
                Debug.LogError("No such type message!");
                return msg;
        }

        try
        {
            msg.Deserialize(bytes);
        }
        catch (Exception e)
        {
            msg = null;
            Debug.LogError("Parse Message Error! " + e);
        }

        return msg;
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


public class LiveMessageSynchronizeAll : LiveMessage
{
    public int seq;
    public Vector3 position;
    public Quaternion rotation;
    public int anchorCount;
    public Vector3[] anchorPositionList;
    public Quaternion[] anchorRotationList;
    public bool[] anchorIsLocated;

    public float receiveTime;

    public LiveMessageSynchronizeAll()
    {
        type = LiveMessageConstant.BEV_MESSAGE_TYPE_SYNCHRONIZE_ALL;
    }

    public override byte[] Serialize()
    {
        byte[] rs = new byte[6 + 4 * 7 + anchorCount * (4 * 7) + anchorCount];


        int index = 0;

        BytesUtility.WriteByteToTBytes(rs, (byte)type, ref index);

        BytesUtility.WriteIntToBytes(rs, seq, ref index);

        BytesUtility.WriteByteToTBytes(rs, (byte)anchorCount, ref index);

        BytesUtility.WriteVectorToBytes(rs, position, ref index);
        BytesUtility.WriteVectorToBytes(rs, rotation, ref index);

        for (int i = 0; i < anchorCount; i++)
        {
            BytesUtility.WriteVectorToBytes(rs, anchorPositionList[i], ref index);
            BytesUtility.WriteVectorToBytes(rs, anchorRotationList[i], ref index);

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
        int index = 0;
        byte b;

        BytesUtility.GetByteFromBytes(data, out b, ref index);
        type = b;

        BytesUtility.GetIntFromBytes(data, out seq, ref index);

        BytesUtility.GetByteFromBytes(data, out b, ref index);
        anchorCount = b;


        BytesUtility.GetVectorFromBytes(data, out position, ref index);
        BytesUtility.GetVectorFromBytes(data, out rotation, ref index);

        anchorPositionList = new Vector3[anchorCount];
        anchorRotationList = new Quaternion[anchorCount];
        anchorIsLocated = new bool[anchorCount];

        for (int i = 0; i < anchorCount; i++)
        {
            Vector3 pos;
            Quaternion rot;
            BytesUtility.GetVectorFromBytes(data, out pos, ref index);
            BytesUtility.GetVectorFromBytes(data, out rot, ref index);

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

    public string FormatLogString()
    {
        string rs = "";

        AddIntString(ref rs, ref this.seq);

        AddVector3String(ref rs, ref this.position);
        AddQuaternionString(ref rs, ref this.rotation);

        AddIntString(ref rs, ref this.anchorCount);

        for (int i = 0; i < this.anchorCount; i++)
        {
            AddVector3String(ref rs, ref this.anchorPositionList[i]);
            AddQuaternionString(ref rs, ref this.anchorRotationList[i]);
        }

        return rs;
    }
    private void AddFloatString(ref string str, ref float f)
    {
        str += f.ToString("f8") + ",";
    }
    private void AddIntString(ref string str, ref int n)
    {
        str += n.ToString() + ",";
    }
    private void AddVector3String(ref string str, ref Vector3 v)
    {
        AddFloatString(ref str, ref v.x);
        AddFloatString(ref str, ref v.y);
        AddFloatString(ref str, ref v.z);
    }
    private void AddQuaternionString(ref string str, ref Quaternion v)
    {
        AddFloatString(ref str, ref v.x);
        AddFloatString(ref str, ref v.y);
        AddFloatString(ref str, ref v.z);
        AddFloatString(ref str, ref v.w);
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
        public bool useUDP;
        public int serverPortUDP;
        public string appId;
        public string roomId;
        public int logIndex;
        public List<string> anchorNameList = new List<string>();
        public List<Vector3InMessage> anchorPosition = new List<Vector3InMessage>();
        public List<Vector3InMessage> anchorForward = new List<Vector3InMessage>();
        public bool sendRotation = false;
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
        public List<string> anchorNameList = new List<string>();
        public List<Vector3InMessage> anchorPosition = new List<Vector3InMessage>();
        public List<Vector3InMessage> anchorForward = new List<Vector3InMessage>();
        public bool sendRotation = false;
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


public class LiveMessageRequestSpatialMapping : LiveMessage
{
    public LiveMessageRequestSpatialMapping()
    {
        type = LiveMessageConstant.BEV_MESSAGE_TYPE_REQUEST_SPATIAL_MAPPING;
    }
}

public class LiveMessageResponseSpatialMapping : LiveMessage
{
    public byte[] mapData;

    public LiveMessageResponseSpatialMapping()
    {
        type = LiveMessageConstant.BEV_MESSAGE_TYPE_RESPONSE_SPATIAL_MAPPING;
    }

    public override byte[] Serialize()
    {
        byte[] rs = new byte[mapData.Length + 1];

        rs[0] = (byte)type;

        int index = 1;

        Array.Copy(mapData, 0, rs, index, mapData.Length);

        return rs;
    }

    public override void Deserialize(byte[] data)
    {
        type = data[0];

        int index = 1;

        mapData = new byte[data.Length - 1];
        Array.Copy(data, index, mapData, 0, data.Length - 1);
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


