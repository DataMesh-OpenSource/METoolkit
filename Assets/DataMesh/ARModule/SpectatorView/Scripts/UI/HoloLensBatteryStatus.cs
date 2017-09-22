using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DataMesh.AR.Utility;

namespace DataMesh.AR.SpectatorView
{
    public class HoloLensBatteryStatus : MonoBehaviour
    {
        public Sprite batteryCharging;
        public Sprite battery4;
        public Sprite battery3;
        public Sprite battery2;
        public Sprite battery1;
        public Sprite batteryLow;

        public Image batteryImage;
        public Text batteryText;

        public UITweener tw;

        public void SetBatteryStatus(bool isCharging, int percent)
        {
            if (isCharging)
            {
                batteryImage.sprite = batteryCharging;
                tw.ResetToBeginning();
                tw.enabled = false;
            }
            else
            {
                if (percent > 75)
                {
                    batteryImage.sprite = battery4;
                }
                else if (percent > 50)
                {
                    batteryImage.sprite = battery3;
                }
                else if (percent > 25)
                {
                    batteryImage.sprite = battery2;
                }
                else if (percent > 5)
                {
                    batteryImage.sprite = battery1;
                }
                else
                {
                    batteryImage.sprite = batteryLow;
                }

                if (batteryImage.sprite == batteryLow)
                {
                    tw.PlayForward();
                }
                else
                {
                    tw.ResetToBeginning();
                    tw.enabled = false;
                }
            }

            batteryText.text = "" + percent + "%";

            gameObject.SetActive(true);
        }
    }
}