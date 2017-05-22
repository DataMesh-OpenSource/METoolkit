using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DataMesh.AR.Common {
    public class WarningContext : MonoBehaviour
    {
        public static WarningContext Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
        }
        public UnityEngine.UI.Text warningText;

        public void Warning(string txt, bool fadeEnable)
        {
            if (fadeEnable)
            {
                StartCoroutine(WaringUI(txt));
            }
            else
            {
                warningText.text = txt;
            }
        }
        IEnumerator WaringUI(string txt)
        {
            if (txt != "")
            {
                warningText.text = "";
                yield return new WaitForSeconds(0.3f);
                warningText.text = txt;
                yield return new WaitForSeconds(3.0f);
            }
        }
        void ResetObjectToFalse(GameObject oneobject)
        {
            oneobject.SetActiveRecursively(true);
            oneobject.active = false;
        }
        void ResetObjectToTrue(GameObject oneobject)
        {
            oneobject.SetActiveRecursively(true);
            oneobject.active = true;
        }
    }

}