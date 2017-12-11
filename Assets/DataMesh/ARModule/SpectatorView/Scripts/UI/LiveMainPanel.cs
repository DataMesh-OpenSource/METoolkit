using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DataMesh.AR.Event;


namespace DataMesh.AR.SpectatorView
{

    public class LiveMainPanel : MonoBehaviour
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

        public Text systemInfoText;

        private bool isRecoding = false;

        private bool canRecordCPU = true;

        private LiveController liveController;
        private LiveControllerUI liveUI;

        // Use this for initialization
        public void Init(LiveController b, LiveControllerUI u)
        {
            liveController = b;
            liveUI = u;
            systemInfoText.text = "";
            canRecordCPU = System.Environment.ProcessorCount >= 4;
        }


        // Update is called once per frame

        private IEnumerator HideSystemInfo()
        {
            yield return new WaitForSeconds(5);
            systemInfoText.text = "";
        }

#endif
    }
}