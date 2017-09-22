using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DataMesh.AR.Utility;


namespace DataMesh.AR.UI
{
    public class CursorCountDown : MonoBehaviour
    {
        public TweenUGUIAlpha tw3;
        public TweenUGUIAlpha tw2;
        public TweenUGUIAlpha tw1;

        public System.Action cbSnapCountDown;


        // Use this for initialization
        void Awake()
        {
            tw3.gameObject.SetActive(false);
            tw2.gameObject.SetActive(false);
            tw1.gameObject.SetActive(false);
        }

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }



        public void ShowSnapCountDown(System.Action cb)
        {
            // 上一个倒计时还没走完，不再重新走 
            if (cbSnapCountDown != null)
                return;

            cbSnapCountDown = cb;

            tw3.gameObject.SetActive(true);
            tw3.ResetToBeginning();
            tw3.AddFinishAction(ShowSpanCountDown3, true);
            tw3.Play(true);
        }

        private void ShowSpanCountDown3()
        {
            tw3.gameObject.SetActive(false);

            tw2.gameObject.SetActive(true);
            tw2.ResetToBeginning();
            tw2.AddFinishAction(ShowSpanCountDown2, true);
            tw2.Play(true);

        }

        private void ShowSpanCountDown2()
        {
            tw2.gameObject.SetActive(false);

            tw1.gameObject.SetActive(true);
            tw1.ResetToBeginning();
            tw1.AddFinishAction(ShowSpanCountDown1, true);
            tw1.Play(true);

        }

        private void ShowSpanCountDown1()
        {
            tw1.gameObject.SetActive(false);

            if (cbSnapCountDown != null)
            {
                cbSnapCountDown();
                cbSnapCountDown = null;
            }
        }
    }

}