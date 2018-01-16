using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DataMesh.AR.Interactive;

namespace DataMesh.AR.UI
{
    public class RegionItem : BlockListItem
    {
        public Image iconSelect;
        public Image iconUnselect;
        public Text regionName;

        protected override void Refresh()
        {

            if (_data != null)
            {
                RegionItemData regionData = _data as RegionItemData;

                // name
                regionName.text = regionData.name;

                if (regionData.selected)
                {
                    iconSelect.gameObject.SetActive(true);
                    iconUnselect.gameObject.SetActive(false);
                }
                else
                {
                    iconSelect.gameObject.SetActive(false);
                    iconUnselect.gameObject.SetActive(false);
                }

            }
        }
    }
}