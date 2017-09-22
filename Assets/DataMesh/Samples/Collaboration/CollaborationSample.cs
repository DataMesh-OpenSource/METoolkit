using System.Collections;
using UnityEngine;
using DataMesh.AR.Interactive;
using DataMesh.AR.Network;
using DataMesh.AR.UI;
using MEHoloClient.Entities;
using MEHoloClient.Proto;

namespace DataMesh.AR.Samples.Collaboration
{
    public enum ColorType
    {
        red = 0,
        blue = 1,
        green = 2,
    }
    public class CollaborationSample : MonoBehaviour, IMessageHandler
    {
        private MultiInputManager inputManager;
        private CollaborationManager cm;
        private CursorController cursor;

        private ColorType CurrentColor;
        private ShowObject showObject;
        private SceneObject roomData;

        void Awake()
        {
            MEHoloEntrance.Instance.AppID = "XYR_Demo";
        }

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

            cursor = UIManager.Instance.cursorController;


            // Todo: Begin your logic
            inputManager = MultiInputManager.Instance;
            inputManager.cbTap += OnTap;

            cm = CollaborationManager.Instance;
            cm.AddMessageHandler(this);
            cm.cbEnterRoom = cbEnterRoom;

            string showId = "showId001";
            string obj_type = "ColorType";

            MsgEntry msg = new MsgEntry();
            msg.ShowId = showId;    

            ObjectInfo info = new ObjectInfo();
            info.ObjType = obj_type;
            msg.Info = info;

            msg.Vec.Add((long)CurrentColor);

            showObject = new ShowObject(msg);
            roomData = new SceneObject();
            roomData.ShowObjectDic.Add(showObject.ShowId, showObject);

            cm.roomInitData = roomData;
            cm.TurnOn();

        }

        private void OnTap(int count)
        {
            if (cm.enterRoomResult == EnterRoomResult.EnterRoomSuccess)
            {
                ClickCube();
            }
            else
            {
                if (cm.enterRoomResult == EnterRoomResult.Waiting)
                {
                    cursor.ShowInfo("waiting....");
                }
                else
                {
                    cursor.ShowInfo("Error! " + cm.enterRoomResult);
                }
            }
        }
        /// <summary>
        /// Callback function of EnterRoom
        /// </summary>
        private void cbEnterRoom()
        {
            Debug.Log("Enter Room Sucessfully");
        }

        private void ClickCube()
        {
            CurrentColor += 1;
            if ((int)CurrentColor > 2)
            {
                CurrentColor = 0;
            }

            MsgEntry entry = new MsgEntry();
            entry.OpType = MsgEntry.Types.OP_TYPE.Upd;
            entry.ShowId = showObject.ShowId;
            entry.Vec.Add((long)CurrentColor);

            SyncMsg msg = new SyncMsg();
            msg.MsgEntry.Add(entry);

            cm.SendMessage(msg);
        }

        void ChangeCubeColor(ColorType CurrentColor)
        {
            GameObject cube = GameObject.Find("Cube");

            switch (CurrentColor)
            {
                case ColorType.red:
                    cube.GetComponent<Renderer>().material.color = Color.red;
                    break;
                case ColorType.blue:
                    cube.GetComponent<Renderer>().material.color = Color.blue;
                    break;
                case ColorType.green:
                    cube.GetComponent<Renderer>().material.color = Color.green;
                    break;
            }
        }
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.name == "Cube")
                    {
                        ClickCube();
                    }

                }
            }
        }
        void IMessageHandler.DealMessage(SyncProto proto)
        {

            this.DealMessage(proto);
        }
        void DealMessage(SyncProto proto)
        {
            Google.Protobuf.Collections.RepeatedField<MsgEntry> messages = proto.SyncMsg.MsgEntry;
            //Debug.Log("deal message");
            if (messages == null)
                return;
            for (int i = 0; i < messages.Count; i++)
            {
                MsgEntry msgEntry = messages[i];
                if (msgEntry.ShowId == showObject.ShowId)
                {
                    ChangeCubeColor((ColorType)((int)msgEntry.Vec[0]));
                }

            }
        }
    }

}