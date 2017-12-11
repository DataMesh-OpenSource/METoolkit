using System.Collections;
using UnityEngine;
using DataMesh.AR.Network;
using DataMesh.AR;
using MEHoloClient.Entities;
using MEHoloClient.Proto;
 
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

        collaborationManager.AddMessageHandler(this);

        MsgEntry entry = new MsgEntry();
        entry.ShowId = "Test";
        GetTransformFloat(cube.transform, entry);

        ShowObject showObject = new ShowObject(entry);
        SceneObject roomData = new SceneObject();
        roomData.ShowObjectDic.Add(showObject.ShowId, showObject);

        collaborationManager.roomInitData = roomData;

        collaborationManager.TurnOn();
    }

    private void GetTransformFloat(Transform trans, MsgEntry entry)
    {
        entry.Pr.Clear();

        float[] rs = new float[6];
        entry.Pr.Add(trans.position.x);
        entry.Pr.Add(trans.position.y);
        entry.Pr.Add(trans.position.z);
        entry.Pr.Add(trans.eulerAngles.x);
        entry.Pr.Add(trans.eulerAngles.y);
        entry.Pr.Add(trans.eulerAngles.z);
    }

    public void DealMessage(SyncProto proto)
    {
        Google.Protobuf.Collections.RepeatedField<MsgEntry> messages = proto.SyncMsg.MsgEntry;
        if (messages == null)
            return;

        for (int i = 0; i < messages.Count; i++)
        {
            MsgEntry msg = messages[i];
            cube.transform.position = new Vector3(msg.Pr[0], msg.Pr[1], msg.Pr[2]);
            cube.transform.eulerAngles = new Vector3(msg.Pr[3], msg.Pr[4], msg.Pr[5]);

            Debug.Log("Receive Message! " + msg.Pr);
        }
    }

    void Update()
    {
        if (collaborationManager != null)
        {
            if (collaborationManager.enterRoomResult == EnterRoomResult.EnterRoomSuccess)
            {
                MsgEntry entry = new MsgEntry();
                entry.OpType = MsgEntry.Types.OP_TYPE.Upd;
                entry.ShowId = "Test";
                GetTransformFloat(cube.transform, entry);

                SyncMsg msg = new SyncMsg();
                msg.MsgEntry.Add(entry);

                collaborationManager.SendMessage(msg);
            }
        }
    }

}
