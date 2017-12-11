using DataMesh.AR;
using DataMesh.AR.SpectatorView;
using DataMesh.AR.Utility;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
using MEHoloClient.Interface.Social;
using MEHoloClient.Interface.Storage;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace DataMesh.AR.SpectatorView
{
    [Serializable]
    public class JsonData
    {
        public int code;
        public string msg;
        public string[] data;
    }
    [Serializable]
    public class JsonTimeData
    {
        public string[] TimeData;
    }
    public enum UploadFileType
    {
        Image,
        Video,
    }
    public class SocialControl : MonoBehaviour
    {

#if UNITY_EDITOR || UNITY_STANDALONE_WIN

        public TextAsset textRecordTime;
        public TextAsset textRecordTimeUnUpload;

        private string albumURL;
        private int serverPort;
        private BackgroundWorker uploadWorker;

        private static SocialControl instance;
        public static SocialControl Instance
        {
            get { return instance; }
        }

        private void Awake()
        {
            instance = this;
            StartCoroutine(SaveAlbumToInfo());
            SaveTimeListToInfo();
            MessageCenter.AddMsgListener(SysDefine.MESSAGE_CheckAndUploadToServer, UploadFile);
        }

        public void Init()
        {
            AppConfig.Instance.LoadConfig(MEHoloConstant.NetworkConfigFile);
            albumURL = AppConfig.Instance.GetConfigByFileName(MEHoloConstant.NetworkConfigFile, "Server_Host", "127.0.0.1");
            serverPort = int.Parse(AppConfig.Instance.GetConfigByFileName(MEHoloConstant.NetworkConfigFile, "Server_Port", "8848"));
            uploadWorker = new BackgroundWorker();
            uploadWorker.StartWorker();
        }

        /// <summary>
        /// 加载RecordTime列表到SocialInfo中
        /// </summary>
        private void SaveTimeListToInfo()
        {
            TextAsset ProfileTimeData = textRecordTime;
            string jsonStr = ProfileTimeData.text;
            Debug.Log("----------------" + jsonStr);
            JsonTimeData jsonTimeData = JsonUtility.FromJson<JsonTimeData>(jsonStr);
            List<string> listAutoRecordTime = new List<string>();
            for (int i = 0; i < jsonTimeData.TimeData.Length; i++)
            {
                listAutoRecordTime.Add(jsonTimeData.TimeData[i]);
            }
            SocialAlbumInfo.Instance.listAutoRecordTime = listAutoRecordTime;
            SocialAlbumInfo.Instance.recordTime = int.Parse(listAutoRecordTime[0]);
        }

        /// <summary>
        /// 加载AlbumName列表到SocialInfo中
        /// </summary>
        /// <returns></returns>
        private IEnumerator SaveAlbumToInfo()
        {
            WWW downloadAlbumProfileData = new WWW("http://" + albumURL + ":" + serverPort.ToString() + "/me/social/album/list");
            yield return downloadAlbumProfileData;
            if (downloadAlbumProfileData.error != null)
            {
                Debug.LogWarning("Download AlbumProfileNameData false ，message : " + downloadAlbumProfileData.error);
            }
            else
            {
                List<string> listAlbumProfileName = new List<string>();
                string albumListJson = downloadAlbumProfileData.text;
                JsonData jsonData = JsonUtility.FromJson<JsonData>(albumListJson);
                
                if (jsonData == null || jsonData.code == 10500)
                {
                    KeyValueUpdate kv = new KeyValueUpdate(SysDefine.MESSAGE_InfomationTypeError, "获取云相册列表失败");
                    MessageCenter.SendMessage(SysDefine.MESSAGE_Infomation, kv);
                }
                else
                {
                    listAlbumProfileName.Clear();
                    for (int i = 0; i < jsonData.data.Length; i++)
                    {
                        listAlbumProfileName.Add(jsonData.data[i]);
                    }
                    SocialAlbumInfo.Instance.listAlbumProfileName = listAlbumProfileName;
                    SocialAlbumInfo.Instance.currentAlbumName = listAlbumProfileName[0];
                }
            }
        }


        public List<string> GetTimeList()
        {
            return SocialAlbumInfo.Instance.listAutoRecordTime;
        }

        public List<string> GetAlbumList()
        {
            return SocialAlbumInfo.Instance.listAlbumProfileName;
        }




        private void UploadFile(KeyValueUpdate kv)
        {
            KeyValueUpdate kvs = new KeyValueUpdate(SysDefine.MESSAGE_InfomationTypeNormal, "Upload···");
            MessageCenter.SendMessage(SysDefine.MESSAGE_Infomation, kvs);

            // MessageCenter.SendMessage();

            UploadFileType fileType;
            string outputPath;
            string fileName;
            fileType = SocialAlbumInfo.Instance.uploadFileType;
            if (fileType == UploadFileType.Image)
            {
                outputPath = SocialAlbumInfo.Instance.imageOutputPath;
                fileName = SocialAlbumInfo.Instance.imageFileName;
            }
            else
            {
                outputPath = SocialAlbumInfo.Instance.videoOutputPath;
                fileName = SocialAlbumInfo.Instance.videoFileName;
            }
            StartCoroutine(CheckAndUpload(outputPath, fileName, fileType));
        }

        /// <summary>
        /// 检查文件是否已保存成功，在3s内检查到保存的文件才会进行上传操作
        /// </summary>
        private IEnumerator CheckAndUpload(string outputPath, string name, UploadFileType fileType)
        {
            float checkTime = 0;
            while (!File.Exists(outputPath + name) && checkTime < 3f)
            {
                checkTime += 0.1f;
                yield return new WaitForSeconds(0.1f);
            }
            if (File.Exists(outputPath + name))
            {
                Debug.Log("开始上传到Server");
                if (CurrentAlbumName != null)
                {
                    StartCoroutine(UploadToServer(outputPath, name, CurrentAlbumName, fileType));
                }
                else
                {
                    KeyValueUpdate kvs = new KeyValueUpdate(SysDefine.MESSAGE_InfomationTypeNormal, "currentAlbumProfileName is null");
                    MessageCenter.SendMessage(SysDefine.MESSAGE_Infomation, kvs);
                }
            }
            else
            {
                Debug.Log("检查该文件是否存在,文件地址 : " + outputPath + name);
            }
        }

        /// <summary>
        /// 上传照片或视频到服务器
        /// </summary>
        private IEnumerator UploadToServer(string outputPath, string name, string currentAlbumName, UploadFileType fileType)
        {
            var fileFolder = outputPath;
            var uploadFileName = name;
            string serverUrl = "http://" + albumURL + ":" + serverPort.ToString();
            string appId = MEHoloEntrance.Instance.AppID;
            float uploadTime = 0;
            UploadProgressApi uploadProgressApi = null;

            try
            {
                if (fileType == UploadFileType.Image)
                {
                    uploadProgressApi = new UploadProgressSocialImageApi(serverUrl,
                     appId, fileFolder, uploadFileName, currentAlbumName, 50);
                }
                else
                {
                    uploadProgressApi = new UploadProgressSocialVideoApi(serverUrl,
                      appId, fileFolder, uploadFileName, currentAlbumName, 50);
                }
                uploadWorker.SubmitWork(uploadProgressApi);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            while (uploadWorker.GetProgress(uploadProgressApi) < 1)
            {
                yield return new WaitForSeconds(0.5f);
                uploadTime += 0.5f;
                if (uploadWorker.GetProgress(uploadProgressApi) >= 1)
                {
                    KeyValueUpdate kvs = new KeyValueUpdate(SysDefine.MESSAGE_InfomationTypeNormal, "Upload Success!!!");
                    MessageCenter.SendMessage(SysDefine.MESSAGE_Infomation, kvs);
                }
            }
        }

        public string CurrentAlbumName
        {
            get { return SocialAlbumInfo.Instance.currentAlbumName; }
            set { SocialAlbumInfo.Instance.currentAlbumName = value; }
        }

        public float RecordTime
        {
            get { return SocialAlbumInfo.Instance.recordTime; }
            set { SocialAlbumInfo.Instance.recordTime = value; }
        }
#endif
    }
}


