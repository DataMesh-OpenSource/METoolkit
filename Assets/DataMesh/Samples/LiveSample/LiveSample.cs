using System.Collections;
using UnityEngine;
using DataMesh.AR;
using DataMesh.AR.Anchor;
using DataMesh.AR.Interactive;
using DataMesh.AR.SpectatorView;

namespace DataMesh.AR.Samples.SpectatorView
{

    public class LiveSample : MonoBehaviour
    {
        private SceneAnchorController anchorController;
        private MultiInputManager inputManager;
        private LiveController bevController;

        void Start()
        {
            bevController = LiveController.Instance;

            StartCoroutine(WaitForInit());
        }

        private IEnumerator WaitForInit()
        {
            MEHoloEntrance entrance = MEHoloEntrance.Instance;

            while (!entrance.HasInit)
            {
                yield return null;
            }
            anchorController = SceneAnchorController.Instance;

            anchorController.serverHost = "192.168.8.250";
            anchorController.serverPort = int.Parse("8823");
            anchorController.appId = int.Parse("1");
            anchorController.roomId = "test";

            // Todo: Begin your logic
            inputManager = MultiInputManager.Instance;
            inputManager.cbTap += OnTap;
            
            bevController.outputPath = "C:\\HologramCapture\\LiveSample\\";
            bevController.cbStartMoveAnchor = OnStartMoveAnchor;
            bevController.cbEndMoveAnchor = OnEndMoveAnchor;
            bevController.listenPort = int.Parse("8099");
            bevController.TurnOn();
        }
        /// <summary>
        /// Callback function before moveing anchor
        /// </summary>
        private void OnStartMoveAnchor()
        {
            //Todo Before Moveing Anchor
        }
        /// <summary>
        /// Callback function after moveing anchor
        /// </summary>
        private void OnEndMoveAnchor()
        {
            //Todo After Moveing Anchor
        }
        private void OnTap(int count)
        {
            inputManager.cbTap -= OnTap;

            SceneAnchorController.Instance.cbAnchorControlFinish = ModifyAnchorFinish;
            SceneAnchorController.Instance.TurnOn();
        }

        private void OnTapUpload(int count)
        {
            CursorController.Instance.isBusy = true;
            SceneAnchorController.Instance.UploadAnchor((bool success, string error) =>
            {
                CursorController.Instance.isBusy = false;
                if (success)
                {
                    CursorController.Instance.ShowInfo("Upload Anchor Success!");
                }
                else
                {
                    CursorController.Instance.ShowInfo("Upload Error! reason is: " + error);
                }
            });
        }

        private void OnTapDownload(int count)
        {
            CursorController.Instance.isBusy = true;
            SceneAnchorController.Instance.DownloadAnchor((bool success, string error) =>
            {
                CursorController.Instance.isBusy = false;
                if (success)
                {
                    CursorController.Instance.ShowInfo("Download Anchor Success!");
                }
                else
                {
                    CursorController.Instance.ShowInfo("Download Error! reason is: " + error);
                }
            });
        }

        private void ModifyAnchorFinish()
        {
            SceneAnchorController.Instance.TurnOff();
            inputManager.cbTap += OnTap;
        }
    }

}