using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using DataMesh.AR.Common;
using System;

namespace DataMesh.AR.MRC {
    public class MixedRealityCapture : DataMesh.AR.MEHoloModuleSingleton<MixedRealityCapture>
    {
        [HideInInspector]
        public string primaryDomain= "https://social.datamesh.com";
        [HideInInspector]
        //public string app_id = "3";

        public System.Action cbAftarTakePicture;
        public System.Action cbAftarTakeVideo;

        public static int maxReconnectCount = 3;

        private PhotoCaptureController photoCapture;
        private VideoCaptureController videoCapture;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void _Init()
        {
            photoCapture = new PhotoCaptureController();
            videoCapture = new VideoCaptureController();
        }

        protected override void _TurnOn()
        {
            
        }

        protected override void _TurnOff()
        {
            
        }

        public void TakeAPicture()
        {
            photoCapture.TakeAPictureToDisk();
        }
        public void AfterTakeAPicture()
        {
            //To Do After Take A Picture
            if (cbAftarTakePicture != null)
                cbAftarTakePicture();
        }
        public void StartVideoCapture()
        {
            videoCapture.StartVideoCapture();
        }
        public void StopVideoCapture()
        {
            videoCapture.StopVideoCapture();
        }
        public void AfterStopVideoCapture()
        {
            //To Do After Stop Video Capture
            if (cbAftarTakeVideo != null)
                cbAftarTakeVideo();
        }
        public void UploadImageME(System.Action cbUploadOK, System.Action<string> cbUploadError)
        {
            string token = "3ACE54EFC4B267908AB5210EDFB16A3F";
            string filepath = photoCapture.filePath;
            string timeStamp = Time.time.ToString().Replace(".", "").Replace(":", "");
            string filename = string.Format("DataMesh_Image_{0}.jpg", timeStamp);
            if (ReadWrite.Instance.FileExist(filepath))
            {
                byte[] filebytes = ReadWrite.Instance.GetFileBytes(filepath);
                string url = "/share/upload/image";
                StartCoroutine(UploadFileME(MEHoloEntrance.Instance.AppID, token, filebytes, url, filename, cbUploadOK, cbUploadError));
            }
            else
            {
                Debug.Log("File Is Not Exist" + filepath);
            }
        }
        public void UploadVideoME(System.Action cbUploadOK, System.Action<string> cbUploadError)
        {
            string token = "3ACE54EFC4B267908AB5210EDFB16A3F";
            string filepath = videoCapture.filepath;
            string timeStamp = Time.time.ToString().Replace(".", "").Replace(":", "");
            string filename = string.Format("DataMesh_Video_{0}.mp4", timeStamp);
            if (ReadWrite.Instance.FileExist(filepath))
            {
                byte[] filebytes = ReadWrite.Instance.GetFileBytes(filepath);
                string url = "/share/upload/video";
                StartCoroutine(UploadFileME(MEHoloEntrance.Instance.AppID, token, filebytes, url, filename, cbUploadOK, cbUploadError));
            }
            else
            {
                if (cbUploadError != null)
                    cbUploadError("File Is Not Exist" + filepath);
                Debug.Log("File Is Not Exist" + filepath);
            }

        }
        public void SetPrimaryDomain(string primaryDomain)
        {
            this.primaryDomain = primaryDomain; //"http://182.92.123.14:8123"
        }

        public IEnumerator UploadFileME(string app_id, string token, byte[] filebytes, string url, string filename, System.Action cbUploadOK, System.Action<string> cbUploadError)
        {
            WWWForm form = new WWWForm();
            form.AddField("token", token);
            form.AddField("app_id", app_id);
            form.AddField("filename", filename);
            form.AddBinaryData("file", filebytes);
            string posturl = primaryDomain + url;
            //string posturl = main_url + "/api/holographic/mrc/files";
            Debug.Log("UploadUrl"+posturl);

            using (UnityWebRequest www = UnityWebRequest.Post(posturl, form))
            {
                bool sendSuccess = false;
                for (int i = 0; i < maxReconnectCount; i++)
                {
                    yield return www.Send();
                    sendSuccess = !(www.isNetworkError);
                    if (sendSuccess)
                    {
                        //ReadWrite.Instance.FileDelete(filepath);
                        break;
                        //if (category=="video") {
                        //    VCR.Instance.OnOpenVideoFile();
                        //}                        
                    }
                    else
                    {
                        if (cbUploadError != null)
                        Debug.Log(www.error);
                    }
                }
                if(sendSuccess)
                {
                    Debug.Log(www.downloadHandler.text);
                    Debug.Log("Form upload complete!");

                    if (cbUploadOK != null)
                        cbUploadOK();
                }
                else
                {
                    cbUploadError(www.error);

                }


            }
            

            
        }
        


    }
}
