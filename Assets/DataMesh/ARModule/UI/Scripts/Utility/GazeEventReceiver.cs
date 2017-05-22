using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataMesh.AR.UI
{

    public class GazeEventReceiver : MonoBehaviour
    {

        public GameObject receiver;

        void OnTapOnObject()
        {
            if (receiver != null)
            {
                receiver.SendMessage("OnTapOnObject", SendMessageOptions.DontRequireReceiver);
            }
        }

        void OnGazeEnterObject()
        {
            if (receiver != null)
            {
                receiver.SendMessage("OnGazeEnterObject", SendMessageOptions.DontRequireReceiver);
            }
        }

        void OnGazeExitObject()
        {
            if (receiver != null)
            {
                receiver.SendMessage("OnGazeExitObject", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

}