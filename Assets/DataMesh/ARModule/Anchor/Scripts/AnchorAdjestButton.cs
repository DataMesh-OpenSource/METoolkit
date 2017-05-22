using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DataMesh.AR.Anchor
{
    public class AnchorAdjestButton : MonoBehaviour
    {
        private AnchorMark mark;

        private Image image;

        public SpatialAdjustType type;

        private Color active = new Color(1f, 0.7f, 0.7f, 1f);
        private Color disactive = new Color(1f, 1f, 1f, 1f);

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Init(AnchorMark m)
        {
            mark = m;
            image = GetComponent<Image>();
        }

        public void SetActive(bool b)
        {
            if (b)
            {
                image.color = active;
            }
            else
            {
                image.color = disactive;
            }
        }

        /*
        /// <summary>
        /// 焦点进入时的处理
        /// </summary>
        /// <param name="eventData"></param>
        public void OnTapOnObject()
        {
            mark.SetAdjustType(type);
        }
        */
    }
}