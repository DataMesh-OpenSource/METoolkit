using DataMesh.AR.SpectatorView;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DataMesh.AR.SpectatorView
{
    public class UIFormMediaOperation : BaseUIForm
    {

        public Button buttonAutoUpload;
        public Button buttonUnAutoUpload;
        public Button buttonTakeSnake;
        public Button buttonRecordStart;
        public Button buttonRecordStop;
        public Text textSecond;
        public Text textMin;
        public Text textHour;

        private bool isAutoUploadToServer = false;
        private bool isRecording = false;
        private bool canRecordCPU = true;

        private float recordTime = 0;
        private float lastSaveTime = 0;
        private float realTime;
        private int second, min, hour;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        private void Awake()
        {
            base.CurrentUIType.UIFormShowModel = UIFormShowModel.HideOther;
            base.CurrentUIType.UIFormType = UIFormType.Fixed;

            RigisterButtonEvent(buttonAutoUpload, AutoUpload);
            RigisterButtonEvent(buttonUnAutoUpload, UnAutoUpload);
            RigisterButtonEvent(buttonTakeSnake, TakeSnake);
            RigisterButtonEvent(buttonRecordStart, RecordStart);
            RigisterButtonEvent(buttonRecordStop, RecordStop);
        }

        public override void Init()
        {
            canRecordCPU = System.Environment.ProcessorCount >= 4;
            buttonAutoUpload.gameObject.SetActive(false);
            buttonUnAutoUpload.gameObject.SetActive(true);
            buttonRecordStart.gameObject.SetActive(true);
            buttonRecordStop.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (canRecordCPU)
                buttonRecordStart.interactable = true;
            else
                buttonRecordStart.interactable = false;
        }

        private void FixedUpdate()
        {
            if (isRecording)
            {
                FixedUpdateRecordTime();
            }
        }

        public void AutoUpload(GameObject obj)
        {
            isAutoUploadToServer = false;
            buttonAutoUpload.gameObject.SetActive(false);
            buttonUnAutoUpload.gameObject.SetActive(true);
        }

        public void UnAutoUpload(GameObject obj)
        {
            isAutoUploadToServer = true;
            buttonAutoUpload.gameObject.SetActive(true);
            buttonUnAutoUpload.gameObject.SetActive(false);
        }

        public void TakeSnake(GameObject obj)
        {
            LiveController.Instance.TakeSnap(ref SocialAlbumInfo.Instance.imageOutputPath, ref SocialAlbumInfo.Instance.imageFileName);
            SocialAlbumInfo.Instance.uploadFileType = UploadFileType.Image;
            if (isAutoUploadToServer)
            {
                SendMessage(SysDefine.MESSAGE_CheckAndUploadToServer, null, null);
            }
        }

        public void RecordStart(GameObject obj)
        {
            ResetTime();
            LiveController.Instance.StartCapture();
            isRecording = true;
            buttonRecordStart.gameObject.SetActive(false);
            buttonRecordStop.gameObject.SetActive(true);

            if (SocialAlbumInfo.Instance.recordTime != -1f)
             {
                StartCoroutine(WaitAndExcet(RecordStop, SocialAlbumInfo.Instance.recordTime));
             } 

        }

        public void RecordStop(GameObject obj)
        {
            if (isRecording)
            {
                isRecording = false;
                string videoOutputpath = null;
                string videoFileName = null;

                buttonRecordStart.gameObject.SetActive(true);
                buttonRecordStop.gameObject.SetActive(false);

                LiveController.Instance.StopCapture(ref videoOutputpath, ref videoFileName);
                SocialAlbumInfo.Instance.uploadFileType = UploadFileType.Video;
                SocialAlbumInfo.Instance.videoOutputPath = videoOutputpath;
                SocialAlbumInfo.Instance.videoFileName = videoFileName + ".mp4";
                if (isAutoUploadToServer)
                {
                    SendMessage(SysDefine.MESSAGE_CheckAndUploadToServer, null, null);
                }
            }
            ResetTime();
        }
        private IEnumerator WaitAndExcet(Action<GameObject> action, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);

            if (action != null)
            {
                action(gameObject);
            }
        }

        private void FixedUpdateRecordTime()
        {
            realTime += Time.deltaTime;
            second = (int)realTime;
            if (second >= 60)
            {
                second = 0;
                realTime = 0;
                min += 1;
            }
            if (min >= 60)
            {
                min = 0;
                hour += 1;
            }
            if (hour >= 24)
            {
                hour = 0;
            }
            textSecond.text = string.Format("{0:D2}", second);
            textMin.text = string.Format("{0:D2}", min);
            textHour.text = string.Format("{0:D2}", hour);
        }

        private void ResetTime()
        {
            realTime = 0;
            second = 0;
            min = 0;
            hour = 0;
            textSecond.text = string.Format("{0:D2}", second);
            textMin.text = string.Format("{0:D2}", min);
            textHour.text = string.Format("{0:D2}", hour);
        }
#endif
    }
}

