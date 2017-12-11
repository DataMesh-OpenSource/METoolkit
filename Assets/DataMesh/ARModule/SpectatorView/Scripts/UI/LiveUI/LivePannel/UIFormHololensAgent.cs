
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DataMesh.AR.SpectatorView
{
    public class UIFormHololensAgent : BaseUIForm
    {

        public Button buttonDownloadAnchor;
        public Button buttonDownloadSpatialMapping;

        //private bool hasInit = false;
        private LiveController liveController;
        private bool hasSync = false;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        private void Awake()
        {
            RigisterButtonEvent(buttonDownloadAnchor, DownloadAnchor);
            RigisterButtonEvent(buttonDownloadSpatialMapping, DownloadSpatialMapping);
        }

        public override void Init()
        {
            base.Init();
            liveController = LiveController.Instance;
        }


        private void Update()
        {
            if (liveController == null)
                return;
            if (liveController.hololensConnected)
            {
                if (liveController.waiting)
                {
                    buttonDownloadAnchor.interactable = false;
                    buttonDownloadSpatialMapping.interactable = false;
                }
                else
                {
                    if (liveController.hololensStartSynchronize)
                    {
                        hasSync = true;
                        buttonDownloadAnchor.interactable = false;
                        buttonDownloadSpatialMapping.interactable = false;
                    }
                    else
                    {
                        hasSync = false;
                        buttonDownloadAnchor.interactable = true;
                        buttonDownloadSpatialMapping.interactable = true;
                    }
                }
            }
        }

        private void DownloadAnchor(GameObject obj)
        {
            liveController.DownloadAnchor();
        }

        private void DownloadSpatialMapping(GameObject obj)
        {
            liveController.DownloadSpatial();
        }
#endif
    }
}


