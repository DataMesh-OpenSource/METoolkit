using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DataMesh.AR.Interactive;

namespace DataMesh.AR.UI
{
    public class BlockListItemDefault : BlockListItem
    {
        public RawImage iconImage;
        public Text nameText;

        private Texture defaultIcon;

        protected override void Refresh()
        {
            defaultIcon = iconImage.texture;

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

            }
        }

        private IEnumerator LoadIcon(string iconUrl)
        {
            Debug.Log("load icon [" + iconUrl + "]");
            WWW www = new WWW(iconUrl);
            yield return www;

            if (www.error == null)
            {
                Texture2D iconTex = new Texture2D(2, 2);
                iconTex.LoadImage(www.bytes);

                iconImage.texture = iconTex;
            }
        }

        // Update is called once per frame
        protected override void _Update()
        {
            if (needLoadImage)
            {
                needLoadImage = false;
                StopCoroutine("LoadIcon");
                StartCoroutine(LoadIcon(data.icon));
            }
        }
    }
}