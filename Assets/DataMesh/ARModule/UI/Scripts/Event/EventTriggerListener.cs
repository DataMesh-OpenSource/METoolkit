using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DataMesh.AR.Event
{
    public class ETListener : UnityEngine.EventSystems.EventTrigger
    {
        public delegate void VoidDelegate(GameObject go);
        public VoidDelegate onClick;
        public VoidDelegate onDown;
        public VoidDelegate onEnter;
        public VoidDelegate onExit;
        public VoidDelegate onUp;
        public VoidDelegate onSelect;
        public VoidDelegate onUpdateSelect;

        private Selectable selectObj;

        static public ETListener Get(GameObject go)
        {
            ETListener listener = go.GetComponent<ETListener>();
            if (listener == null) listener = go.AddComponent<ETListener>();
            listener.selectObj = go.GetComponent<Selectable>();
            return listener;
        }
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (selectObj != null && !selectObj.interactable)
                return;
            if (onClick != null) onClick(gameObject);
        }
        public override void OnPointerDown(PointerEventData eventData)
        {
            if (selectObj != null && !selectObj.interactable)
                return;
            if (onDown != null) onDown(gameObject);
        }
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (selectObj != null && !selectObj.interactable)
                return;
            if (onEnter != null) onEnter(gameObject);
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            if (selectObj != null && !selectObj.interactable)
                return;
            if (onExit != null) onExit(gameObject);
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            if (selectObj != null && !selectObj.interactable)
                return;
            if (onUp != null) onUp(gameObject);
        }
        public override void OnSelect(BaseEventData eventData)
        {
            if (selectObj != null && !selectObj.interactable)
                return;
            if (onSelect != null) onSelect(gameObject);
        }
        public override void OnUpdateSelected(BaseEventData eventData)
        {
            if (selectObj != null && !selectObj.interactable)
                return;
            if (onUpdateSelect != null) onUpdateSelect(gameObject);
        }
    }
}