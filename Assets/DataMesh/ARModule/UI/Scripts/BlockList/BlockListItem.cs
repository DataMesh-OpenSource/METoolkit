using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DataMesh.AR.Interactive;
using DataMesh.AR.UI;

namespace DataMesh.AR.UI
{
    public class BlockListItem : MonoBehaviour
    {
        public const float ITEM_WIDTH = 110;
        public const float ITEM_HEIGHT = 110;

        public RawImage iconImage;
        public Text nameText;

        public Image border;

        public TweenerGroupTransitObject focusTransit;
        public TweenerGroupTransitObject showTransit;
        public TweenerGroupTransitObject clickTransit;

        private Texture defaultIcon;

        private BlockListData _data;
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

        private BlockList parent;

        private bool hasInit = false;

        private bool focus = false;

        private MultiInputManager inputManager;

        private bool needLoadImage = false;

        private void Refresh()
        {
            if (_data != null)
            {
                // name
                nameText.text = _data.name;

                // icon
                iconImage.texture = defaultIcon;
                if (data.icon != null)
                {
                    needLoadImage = true;
                }

                border.gameObject.SetActive(false);
            }
        }

        private IEnumerator LoadIcon(string iconUrl)
        {
            Debug.Log("load icon [" + iconUrl + "]");
            WWW www = new WWW(iconUrl);
            yield return www;

            if (www.error == null)
            {
                Texture2D iconTex = new Texture2D(2,2);
                iconTex.LoadImage(www.bytes);

                iconImage.texture = iconTex;
            }
        }

        public void Init(BlockList list)
        {
            parent = list;

            defaultIcon = iconImage.texture;

            inputManager = MultiInputManager.Instance;

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

        // Update is called once per frame
        void Update()
        {
            if (inputManager != null)
            {
                if (!focus && inputManager.FocusedObject == gameObject)
                {
                    //focusTransit.transit(true, null);
                    border.gameObject.SetActive(true);
                    focus = true;
                }
                else if (focus && inputManager.FocusedObject != gameObject)
                {
                    //focusTransit.transit(false, null);
                    border.gameObject.SetActive(false);
                    focus = false;
                }
            }

            if (needLoadImage)
            {
                needLoadImage = false;
                StopCoroutine("LoadIcon");
                StartCoroutine(LoadIcon(data.icon));
            }
        }

        void OnTapOnObject()
        {
            parent.IsBusy = true;
            clickTransit.transit(true, ()=>
            {
                parent.IsBusy = false;
                parent.OnClick(this);
            }
            );
        }
    }
}