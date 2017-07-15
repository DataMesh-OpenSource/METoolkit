using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DataMesh.AR.Utility
{
    public class SelectedEventDispatcher : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        public System.Action<GameObject> cbSelect;
        public System.Action<GameObject> cbDeselect;

        public void OnSelect(BaseEventData eventData)
        {
            if (cbSelect != null)
            {
                cbSelect(this.gameObject);
            }
        }

        public void OnDeselect(BaseEventData data)
        {
            if (cbDeselect != null)
                cbDeselect(this.gameObject);
        }
    }


}