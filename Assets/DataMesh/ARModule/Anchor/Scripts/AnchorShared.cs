using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataMesh.AR.Common;
using UnityEngine.Networking;
using MEHoloClient.Interface.Anchor;
using MEHoloClient.Sync;

#if UNITY_METRO && !UNITY_EDITOR
using UnityEngine.VR.WSA;
using UnityEngine.VR.WSA.Sharing;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.Networking;
using Windows.Foundation;
using System.Threading.Tasks;
#endif

namespace DataMesh.AR.Anchor {

    public class UploadMessage
    {
        public int status;
        public string message;
        public string app;
        public string handle;
    }

    public class AnchorShared : MonoBehaviour
    {
        [HideInInspector]
        public int appId;

        [HideInInspector]
        public string roomId = "";

        [HideInInspector]

        public string serverHost = "";
        public int serverPort = 0;


        private string tempFileName = "TempAnchorFile.anchor";

        private string Path;
        private byte[] Buffer;


        void Awake()
        {
            Path = Application.temporaryCachePath.ToString() + "\\" + tempFileName;
            Buffer = new byte[0];
        }

#if UNITY_METRO && !UNITY_EDITOR

        private System.Action<bool, string> cbExportFinish;
        private System.Action<bool, string, WorldAnchorTransferBatch> cbImportFinish;


        //#if true

        private bool uploadFinish = true;
        private string uploadErrorString;

        private bool downloadFinish = true;
        private string downloadErrorString;

        private byte[] importedData;
        private int retryCount = 3;

        void Update()
        {
            if (cbExportFinish != null)
            {
                if (uploadFinish)
                {
                    cbExportFinish(uploadErrorString == null ? true : false, uploadErrorString);
                    uploadFinish = false;
                    cbExportFinish = null;
                }
            }
            if (cbImportFinish != null)
            {
                if (downloadFinish)
                {
                    if (downloadErrorString != null)
                    {
                        cbImportFinish(false, downloadErrorString, null);
                        cbImportFinish = null;
                    }
                    else
                    {
                        ContinueDownload();
                    }
                    downloadFinish = false;
                }
            }
        }

        /// <summary>
        /// 导出anchor资料并上传
        /// 注意，上传期间应当阻止应用活动，以避免上传完成之前再次调用 
        /// </summary>
        /// <param name="anchors"></param>
        /// <param name="cbFinish"></param>
        public void ExportGameRootAnchor(List<AnchorObjectInfo> anchors, System.Action<bool, string> cbFinish)
        {

            cbExportFinish = cbFinish;
            uploadFinish = false;
            uploadErrorString = null;

            WorldAnchorTransferBatch transferBatch = new WorldAnchorTransferBatch();

            for (int i = 0; i < anchors.Count; i++)
            {
                AnchorObjectInfo info = anchors[i];
                if (info.anchor != null)
                {
                    bool flag = transferBatch.AddWorldAnchor(info.anchorName, info.anchor);
                    Debug.Log(flag);
                }
            }

            Buffer = new byte[0];
            WorldAnchorTransferBatch.ExportAsync(transferBatch, OnExportDataAvailable, OnExportComplete);
        }

        private void OnExportDataAvailable(byte[] data)
        {
            Debug.Log(data.Length);
            Debug.Log("TransferDataToClient");

            byte[] tmpBuffer = new byte[Buffer.Length + data.Length];
            Buffer.CopyTo(tmpBuffer, 0);
            data.CopyTo(tmpBuffer, Buffer.Length);
            Buffer = tmpBuffer;
        }

        private void OnExportComplete(SerializationCompletionReason completionReason)
        {
            if (completionReason != SerializationCompletionReason.Succeeded)
            {
                uploadFinish = true;
                uploadErrorString = "Excport Anchor Failed";
            }
            else
            {
                Debug.Log("Make data success, Begin Upload");
                //ReadWrite.Instance.WriteFile(name, Buffer);
                ToUploadAnchor(Buffer);
                Debug.Log(Buffer.Length);
            }
        }

        private async void ToUploadAnchor(byte[] anchor)
        {
            AnchorApi api;
            try
            {
                api = new AnchorApi("http://" + serverHost, serverPort);

                Debug.Log("Upload Anchor! app=" + appId + " room=" + roomId + " ip=" + api.host + " port=" + api.port);

                var responseUpload = await api.UploadAnchor(appId, roomId, tempFileName, anchor, 300);
            }
            catch (Exception e)
            {
                uploadErrorString = e.ToString();
            }

            //Debug.Log(responseUpload, "Response [Upload]");

            uploadFinish = true;
            //cbExportFinish(true, null);

        }



        public void ImportRootGameObject(System.Action<bool, string, WorldAnchorTransferBatch> cbFinish = null)
        {
            cbImportFinish = cbFinish;
            downloadFinish = false;
            downloadErrorString = null;

            downloadAnchor();
        }

        private async void downloadAnchor()
        {
            AnchorApi api;
            try
            {
                api = new AnchorApi("http://" + serverHost, serverPort);

                Debug.Log("Download Anchor! app=" + appId + " room=" + roomId + " ip=" + api.host + " port=" + api.port);

                importedData = await api.DownloadAnchor(appId, roomId);
            }
            catch (Exception e)
            {
                downloadErrorString = e.ToString();
            }

            downloadFinish = true;
        }

        private void ContinueDownload()
        {
            Debug.Log("Download, data size=" + importedData.Length + " , Begin to Import!");
            if (importedData.Length != 0)
            {
                retryCount = 3;
                WorldAnchorTransferBatch.ImportAsync(importedData, OnImportComplete);
            }
            else
            {
                cbImportFinish(false, "Anchor data is wrong!", null);
                cbImportFinish = null;
            }

        }

        private void OnImportComplete(SerializationCompletionReason completionReason, WorldAnchorTransferBatch deserializedTransferBatch)
        {
            if (completionReason != SerializationCompletionReason.Succeeded)
            {
                Debug.Log("Failed to import: " + completionReason.ToString());
                if (retryCount > 0)
                {
                    retryCount--;
                    WorldAnchorTransferBatch.ImportAsync(importedData, OnImportComplete);
                }
                else
                {
                    cbImportFinish(false, "Import Anchor Failed after retry times.", null);
                    cbImportFinish = null;
                }
                return;
            }

            Debug.Log("Import success!");

            cbImportFinish(true, null, deserializedTransferBatch);
            cbImportFinish = null;
        }

        /*
        IEnumerator GetAnchor()
        {
            string getUrl = serverHost + "v1/hololens/sharing/" + appId + "/anchor?user_id=anchor";
            Debug.Log(getUrl);
            using (WWW www = new WWW(getUrl))
            {
                while (!www.isDone)
                    yield return www;

                if (!string.IsNullOrEmpty(www.error))
                {
                    Debug.Log("error:" + www.error);
                    if (cbImportFinish != null)
                        cbImportFinish(false, www.error, null);
                    cbImportFinish = null;
                }

                else
                {
                    importedData = www.bytes;
                    Debug.Log(importedData.Length);
                    if (importedData.Length != 0)
                    {
                        retryCount = 3;
                        WorldAnchorTransferBatch.ImportAsync(importedData, OnImportComplete);
                    }
                    else {
                        if (cbImportFinish != null)
                            cbImportFinish(false, "Anchor data is wrong!", null);
                        cbImportFinish = null;
                    }

                }
            }

        }
        */
#endif

    }

}
