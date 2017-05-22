using System.Collections;
using UnityEngine;
using DataMesh.AR.Network;
using DataMesh.AR;
using MEHoloClient.Entities;

public class GettingStartedSample : MonoBehaviour, IMessageHandler
{
    public GameObject cube;
    private CollaborationManager collaborationManager;

    void Start()
    {
        StartCoroutine(WaitForInit());
    }

    private IEnumerator WaitForInit()
    {
        MEHoloEntrance entrance = MEHoloEntrance.Instance;
        while (!entrance.HasInit)
        {
            yield return null;
        }

        collaborationManager = CollaborationManager.Instance;
        collaborationManager.appId = 9999;
        collaborationManager.roomId = "Room1";
        collaborationManager.serverHost = "192.168.2.50";

        collaborationManager.AddMessageHandler(this);

        SceneObjects roomInitData = new SceneObjects();
        ShowObject obj = new ShowObject(
                    "Test", 
                    true,
                    GetTransformFloat(cube.transform), 
                    null
                    );
        roomInitData.ShowObjectDic.Add(obj.show_id, obj);
        collaborationManager.roomInitData = roomInitData;

        collaborationManager.TurnOn();
    }

    private float[] GetTransformFloat(Transform trans)
    {
        float[] rs = new float[6];
        rs[0] = trans.position.x;
        rs[1] = trans.position.y;
        rs[2] = trans.position.z;
        rs[3] = trans.eulerAngles.x;
        rs[4] = trans.eulerAngles.y;
        rs[5] = trans.eulerAngles.z;
        return rs;
    }

    public void DealMessage(SyncProto proto)
    {
        MsgEntry[] messages = proto.sync_msg.msg_entry;
        if (messages == null)
            return;

        for (int i = 0; i < messages.Length; i++)
        {
            MsgEntry msg = messages[i];
            cube.transform.position = new Vector3(msg.pr[0], msg.pr[1], msg.pr[2]);
            cube.transform.eulerAngles = new Vector3(msg.pr[3], msg.pr[4], msg.pr[5]);
        }
    }

    void Update()
    {
        if (collaborationManager != null && collaborationManager.hasEnterRoom)
        {
            MsgEntry entry = new MsgEntry(
                    OP_TYPE.UPD,
                    "Test", 
                    true, 
                    GetTransformFloat(cube.transform), 
                    null, 
                    null
                    );
            collaborationManager.SendMessage(new MsgEntry[1] { entry });
        }
    }

}
