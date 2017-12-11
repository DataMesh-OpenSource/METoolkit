using System.Collections;
using UnityEngine;
using DataMesh.AR;
using DataMesh.AR.Anchor;
using DataMesh.AR.Interactive;
using DataMesh.AR.UI;

public class VuforiaAnchorSample : MonoBehaviour
{
    private MultiInputManager inputManager;

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
        // Todo: Begin your logic
        inputManager = MultiInputManager.Instance;
        inputManager.cbTap += OnTap;
    }

    private void OnTap(int count)
    {
        inputManager.cbTap -= OnTap;

        SceneAnchorController.Instance.AddCallbackFinish(ModifyAnchorFinish);
        SceneAnchorController.Instance.TurnOn();
        //VuforiaManager.Instance.TurnOn();
    }

    private void OnTapUpload(int count)
    {
        UIManager.Instance.cursorController.isBusy = true;
        SceneAnchorController.Instance.UploadAnchor((bool success, string error) =>
        {
            UIManager.Instance.cursorController.isBusy = false;
            if (success)
            {
                UIManager.Instance.cursorController.ShowInfo("Upload Anchor Success!");
            }
            else
            {
                Debug.Log(error);
                UIManager.Instance.cursorController.ShowInfo("Upload Error! reason is: " + error);
            }
        });
    }

    private void OnTapDownload(int count)
    {
        UIManager.Instance.cursorController.isBusy = true;
        SceneAnchorController.Instance.DownloadAnchor((bool success, string error) =>
        {
            UIManager.Instance.cursorController.isBusy = false;
            if (success)
            {
                UIManager.Instance.cursorController.ShowInfo("Download Anchor Success!");
            }
            else
            {
                UIManager.Instance.cursorController.ShowInfo("Download Error! reason is: " + error);
            }
        });
    }

    private void ModifyAnchorFinish()
    {
        //VuforiaManager.Instance.TurnOff();
        SceneAnchorController.Instance.RemoveCallbackFinish(ModifyAnchorFinish);
        SceneAnchorController.Instance.TurnOff();
        inputManager.cbTap += OnTap;
    }
}
