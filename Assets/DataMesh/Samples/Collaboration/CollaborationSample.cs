using System.Collections;
using UnityEngine;
using DataMesh.AR.Interactive;
using DataMesh.AR.Network;
using MEHoloClient.Entities;

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
        CollaborationManager cm;

        int appId;
        string roomId;
        string serverHost;
        int serverPort;
        ColorType CurrentColor;
        ShowObject showObject;
        SceneObjects roomData;
        void Start()
        {
            appId = int.Parse("1");
            roomId = "test";
            serverHost = "192.168.2.50";
            serverPort = int.Parse("8823");

            StartCoroutine(WaitForInit());
        }

        private IEnumerator WaitForInit()
        {
            MEHoloEntrance entrance = MEHoloEntrance.Instance;

            while (!entrance.HasInit)
            {
                yield return null;
            }


            // Todo: Begin your logic
            inputManager = MultiInputManager.Instance;
            inputManager.cbTap += OnTap;

            cm = CollaborationManager.Instance;
            cm.AddMessageHandler(this);
            cm.appId = appId;
            cm.roomId = roomId;
            cm.serverHost = serverHost;
            cm.serverPort = serverPort;
            cm.cbEnterRoom = cbEnterRoom;

            roomData = new SceneObjects();
            string showId = "showId001";
            bool enabled = false;
            string obj_type = "ColorType";
            float[] pr = new float[7];
            float[] prInit = new float[7];
            showObject = new ShowObject(showId, enabled, pr, prInit);
            showObject.obj_type = obj_type;
            roomData.ShowObjectDic.Add(showObject.show_id, showObject);
            cm.roomInitData = roomData;
            cm.TurnOn();

        }

        private void OnTap(int count)
        {
            ClickCube();
        }
        /// <summary>
        /// Callback function of EnterRoom
        /// </summary>
        private void cbEnterRoom()
        {
            Debug.Log("Enter Room Sucessfully");
        }

        [ContextMenu("ClickCube")]
        private void ClickCube()
        {
            CurrentColor += 1;
            if ((int)CurrentColor > 2)
            {
                CurrentColor = 0;
            }
            //ChangeCubeColor(CurrentColor);
            showObject.pr[0] = (float)CurrentColor;
            roomData.ShowObjectDic[showObject.show_id] = showObject;
            MsgEntry entry = new MsgEntry(OP_TYPE.UPD, showObject.show_id, true, showObject.pr, null, null);
            entry.obj_type = showObject.obj_type;
            cm.SendMessage(new MsgEntry[1] { entry });
            //Debug.Log(showObject.pr[0]);
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
            MsgEntry[] messages = proto.sync_msg.msg_entry;
            //Debug.Log("deal message");
            if (messages == null)
                return;
            for (int i = 0; i < messages.Length; i++)
            {
                MsgEntry msgEntry = messages[i];
                if (msgEntry.show_id == showObject.show_id)
                {
                    ChangeCubeColor((ColorType)((int)msgEntry.pr[0]));
                }

            }
        }
    }

}