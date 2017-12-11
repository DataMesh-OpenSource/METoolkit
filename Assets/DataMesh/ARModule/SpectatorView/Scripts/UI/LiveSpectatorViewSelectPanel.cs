using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DataMesh.AR.Event;

namespace DataMesh.AR.SpectatorView
{
    public class LiveSpectatorViewSelectPanel : MonoBehaviour
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

        public Button StartHololensSpectatorView;

        private LiveController liveController;
        private LiveControllerUI controlPanel;

        public void Init(LiveController b, LiveControllerUI panel)
        {
            liveController = b;
            controlPanel = panel;

            ETListener.Get(StartHololensSpectatorView.gameObject).onClick = OnStartHololensConnect;

        }

        private void OnStartHololensConnect(GameObject go)
        {
            liveController.StartHololensServer();

            // 通知主面板 
            controlPanel.OnStartHololensConnect();
        }
#endif
    }
}