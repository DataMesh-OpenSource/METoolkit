using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DataMesh.AR.Interactive;

namespace DataMesh.AR.UI
{
    public class BlockListItem : MonoBehaviour
    {
        public const float ITEM_WIDTH = 150;
        public const float ITEM_HEIGHT = 150;


        //public TweenerGroupTransitObject focusTransit;
        public TweenerGroupTransitObject showTransit;
        public TweenerGroupTransitObject clickTransit;

        public Image border;

        protected BlockListData _data;
        public BlockListData data
        {
            get { return _data; }
            set
            {
                _data = value;
                if (hasInit)
                {
                    Refresh();
                }
            }
        }

        protected BlockList parent;

        protected bool hasInit = false;

        protected bool focus = false;

        protected MultiInputManager inputManager;

        protected bool needLoadImage = false;


        public void Init(BlockList list)
        {
            parent = list;

            inputManager = MultiInputManager.Instance;

            border.gameObject.SetActive(false);

            Refresh();
            hasInit = true;

            gameObject.SetActive(false);
        }

        public void Show(System.Action cbFinish)
        {
            showTransit.transit(true, cbFinish);
        }

        public void Hide(System.Action cbFinish)
        {
            showTransit.transit(false, cbFinish);
        }


        void OnTapOnObject()
        {
            if (clickTransit != null)
            {
                parent.IsBusy = true;
                clickTransit.transit(true, () =>
                {
                    parent.IsBusy = false;
                    parent.OnClick(this);
                }
                );
            }
            else
            {
                parent.OnClick(this);
            }
        }

        // Update is called once per frame
        private void Update()
        {
            if (inputManager != null)
            {
                if (!focus && inputManager.FocusedObject == gameObject)
                {
                    border.gameObject.SetActive(true);
                    focus = true;
                }
                else if (focus && inputManager.FocusedObject != gameObject)
                {
                    border.gameObject.SetActive(false);
                    focus = false;
                }
            }

            _Update();
        }


        protected virtual void Refresh()
        {
            // 子类实现 
        }

        protected virtual void _Update()
        {

        }

    }
}