using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataMesh.AR.UI
{

    public class GazeEventReceiver : MonoBehaviour
    {

        public GameObject receiver;

        public System.Action<GameObject> cbTap;
        public System.Action<GameObject> cbEnter;
        public System.Action<GameObject> cbExit;

        void OnTapOnObject()
        {
            if (receiver != null)
            {
                receiver.SendMessage("OnTapOnObject", SendMessageOptions.DontRequireReceiver);
            }
            if (cbTap != null)
                cbTap(gameObject);
        }

        void OnGazeEnterObject()
        {
            if (receiver != null)
            {
                receiver.SendMessage("OnGazeEnterObject", SendMessageOptions.DontRequireReceiver);
            }
            if (cbEnter != null)
                cbEnter(gameObject);
        }

        void OnGazeExitObject()
        {
            if (receiver != null)
            {
                receiver.SendMessage("OnGazeExitObject", SendMessageOptions.DontRequireReceiver);
            }
            if (cbExit != null)
                cbExit(gameObject);
        }
    }

}