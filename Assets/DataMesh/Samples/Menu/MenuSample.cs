using System.Collections;
using UnityEngine;
using DataMesh.AR;
using DataMesh.AR.UI;
using DataMesh.AR.Interactive;

public class MenuSample : MonoBehaviour
{
    private MultiInputManager inputManager;
    private BlockMenuManager menuManager;
    private BlockMenu mainMenu;

    // Use this for initialization
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

        // Todo: Begin your logic
        menuManager = BlockMenuManager.Instance;
        mainMenu = menuManager.GetMenu("MainMenu");
        mainMenu.RegistButtonClick("button1", OnClickButton1);

        inputManager = MultiInputManager.Instance;
        inputManager.cbTap = OnTap;
    }

    private void OnTap(int count)
    {
        menuManager.OpenMenu(mainMenu);
    }

    private void OnClickButton1()
    {
        Debug.Log("Click Button!");
        CursorController.Instance.ShowInfo("Click Button!");
    }
}
