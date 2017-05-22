using System.Collections;
using UnityEngine;
using DataMesh.AR;
using DataMesh.AR.Anchor;
using DataMesh.AR.Interactive;

public class SceneAnchorSample : MonoBehaviour
{
    private MultiInputManager inputManager;

    void Start ()
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

        SceneAnchorController.Instance.serverHost = "192.168.2.50";
        SceneAnchorController.Instance.roomId = "testtest";
        // Todo: Begin your logic
        inputManager = MultiInputManager.Instance;
        inputManager.cbTap += OnTap;
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
