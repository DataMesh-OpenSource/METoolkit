using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DataMesh.AR.Interactive;
using DataMesh.AR.UI;
using MEHoloClient.Core.Entities;
using MEHoloClient.Interface.Storage;

namespace DataMesh.AR.Storage
{
    public class StorageManager : DataMesh.AR.MEHoloModuleSingleton<StorageManager>
    {
        public enum StorageResourceType
        {
            Asset,
            Record
        }

        /// <summary>
        /// 用户识别ID
        /// </summary>
        public string clientId = "";

        [HideInInspector]
        /// <summary>
        /// 服务器地址
        /// </summary>
        public string serverHost = "";

        /// <summary>
        /// 查询一类资源的结果
        /// </summary>
        [HideInInspector]
        public List<BaseResource> listResult;

        private Platform currentPlatform;
        private StorageResourceType currentListResourceType = StorageResourceType.Asset;

        private bool isBusy = false;

        private StorageUI ui;

        /// <summary>
        /// 加载资源列表时，加载完成后会触发此回调
        /// </summary>
        private System.Action<bool, string> cbLoadPageFinish;
        private System.Action<string> cbDownloadResourceFinish;

        private string cacheFilePath;
        private string persistentFilePath;

        [HideInInspector]
        public int curPage = 1;
        [HideInInspector]
        public int totalCount = 0;

        private AssetApi assetAPI;
        private RecordingApi recordingAPI;
        private BackgroundWorker downloadWorker;


        private bool isWaitingList = false;
        private bool isWaitingDownload = false;
        private bool listFinish = false;
        private string err = null;
        //private bool downloadFinish = false;
        private string downloadFilePath;

        public StorageUI GetUI()
        {
            return ui;
        }

        public bool IsBusy()
        {
            return isBusy;
        }

        protected override void _Init()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            persistentFilePath = Application.dataPath + "/../Storage/";
            cacheFilePath = Application.dataPath + "/../CacheFile/Storage/";
#else
            persistentFilePath = Application.persistentDataPath + "/Storage/";
            cacheFilePath = Application.temporaryCachePath + "/Storage/";
#endif

#if UNITY_STANDALONE || UNITY_EDITOR
            currentPlatform = Platform.pc;
#elif UNITY_IOS
            currentPlatform = Platform.ios;
#elif UNITY_ANDROID
            currentPlatform = Platform.android;
#elif UNITY_WSA
            currentPlatform = Platform.uwp;
#endif

            ui = GetComponent<StorageUI>();
            ui.Init(this);


        }

        protected override void _TurnOn()
        {
            Utility.AppConfig config = Utility.AppConfig.Instance;
            config.LoadConfig(MEHoloConstant.NetworkConfigFile);
            serverHost = Utility.AppConfig.Instance.GetConfigByFileName(MEHoloConstant.NetworkConfigFile, "Server_Host", "127.0.0.1");

            ui.TurnOn();

            Debug.Log("App[" + MEHoloEntrance.Instance.AppID + "] Get Storage From " + serverHost);
            assetAPI = new AssetApi("http://" + serverHost, 80);
            recordingAPI = new RecordingApi("http://" + serverHost, 80);

            downloadWorker = new BackgroundWorker();
            downloadWorker.StartWorker();

        }

        protected override void _TurnOff()
        {

        }

        /// <summary>
        /// 获取Asset列表
        /// </summary>
        public void GetStorageResourceList(StorageResourceType type,int currentPage, int countPerPage, System.Action<bool, string> cbFinish)
        {
            if (isBusy)
                return;

            cbLoadPageFinish = cbFinish;
            currentListResourceType = type;

            isWaitingList = true;
            listFinish = false;
            listResult = null;
            isBusy = true;
            err = null;

            curPage = currentPage;

            if (type == StorageResourceType.Asset)
                GetAssetList(curPage, countPerPage);
            else if (type == StorageResourceType.Record)
                GetRecordList(curPage, countPerPage);
        }

#if UNITY_WSA && !UNITY_EDITOR
        private async void GetAssetList(int page, int countPerPage)
#else
        private void GetAssetList(int page, int countPerPage)
#endif
        {
            Page<Asset> pageResult = null;
            // 加载数据 
            try
            {
#if UNITY_WSA && !UNITY_EDITOR
                pageResult = await assetAPI.ListAssets(MEHoloEntrance.Instance.AppID, currentPlatform, page, countPerPage);
#else
                pageResult = assetAPI.ListAssets(MEHoloEntrance.Instance.AppID, currentPlatform, page, countPerPage);

#endif
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                err = e.ToString();
            }


            if (err == null)
            {
                // 处理数据
                totalCount = pageResult.totalHits;
                Debug.Log("Storage find " + pageResult.totalHits + " item, Total Page: " + pageResult.totalPage);

                listResult = new List<BaseResource>();
                if (pageResult.data != null)
                {
                    for (int i = 0; i < pageResult.data.Length; i++)
                    {
                        listResult.Add(pageResult.data[i]);
                    }
                }
            }

            listFinish = true;


        }


#if UNITY_WSA && !UNITY_EDITOR
        private async void GetRecordList(int page, int totalCount)
#else
        private void GetRecordList(int page, int totalCount)
#endif
        {
            Page<Recording> pageResult = null;
            // 加载数据 
            try
            {
#if UNITY_WSA && !UNITY_EDITOR
                pageResult = await recordingAPI.ListRecordings(MEHoloEntrance.Instance.AppID, page, totalCount);
#else
                pageResult = recordingAPI.ListRecordings(MEHoloEntrance.Instance.AppID, page, totalCount);

#endif
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                err = e.ToString();
            }


            if (err == null)
            {
                // 处理数据
                totalCount = pageResult.totalHits;
                Debug.Log("Storage find " + pageResult.totalHits + " item, Total Page: " + pageResult.totalPage);

                listResult = new List<BaseResource>();
                if (pageResult.data != null)
                {
                    for (int i = 0; i < pageResult.data.Length; i++)
                    {
                        listResult.Add(pageResult.data[i]);
                    }
                }
            }

            listFinish = true;
        }

        /// <summary>
        /// 根据一个Asset信息，下载实际的Asset内容
        /// </summary>
        /// <param name="asset"></param>
        public void DownloadAsset(Asset asset, System.Action<string> cbFinish)
        { 
            string fileName = null;
            long fileSize = 0;
            // 寻找文件名
            if (asset.contents != null)
            {
                for (int i = 0;i < asset.contents.Length;i ++)
                {
                    AssetContent content = asset.contents[i];
                    if (content.platform == currentPlatform)
                    {
                        fileName = content.bundleHash;
                        fileSize = content.bundleSize;
                    }
                }
            }

            cbDownloadResourceFinish = cbFinish;
            if (fileName == null)
            {
                // 没找到文件，直接返回null; 
                StorageReady(null);
            }
            else
            {
                CheckStorageFile(asset.id, fileName, fileSize, DownloadAssetFile);
            }
        }

        /// <summary>
        /// 根据一个Recording信息，下载其中包含的Model资源
        /// </summary>
        /// <param name="record"></param>
        /// <param name="cbFinish"></param>
        public void DownloadRecordingModel(Recording record, System.Action<string> cbFinish)
        {
            string fileName = record.modelHash;
            long fileSize = record.modelSize;

            cbDownloadResourceFinish = cbFinish;
            if (fileName == null)
            {
                // 没找到文件，直接返回null; 
                StorageReady(null);
            }
            else
            {
                CheckStorageFile(record.id, fileName, fileSize, DownloadRecordingModelFile);
            }
        }

        /// <summary>
        /// 检查一个资源是否已经下载过
        /// </summary>
        /// <param name="resId">资源的ID</param>
        /// <param name="fileName">存储的目标文件</param>
        /// <param name="fileSize">文件尺寸</param>
        /// <param name="todoDownload">需要实际调用的加载方法</param>
        private void CheckStorageFile(string resId, string fileName, long fileSize, System.Action<string,string> todoDownload)
        {
            string savePath = persistentFilePath + MEHoloEntrance.Instance.AppID + "/";
            string filePath = savePath + fileName;
            bool needDownload = false;
            if (!File.Exists(filePath))
            {
                // 文件不存在，标记为需要下载 
                needDownload = true;
            }

            if (needDownload)
            {
                Debug.Log("id:" + resId);
                Debug.Log("path:" + savePath);
                Debug.Log("file=" + fileName);

                isWaitingDownload = true;
                //downloadFinish = false;
                downloadFilePath = filePath;
                isBusy = true;

                todoDownload(resId, fileName);
            }
            else
            {
                // 直接返回文件地址 
                StorageReady(filePath);
            }
        }

        /// <summary>
        /// 下载一个Asset的内容
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="fileName"></param>
        private void DownloadAssetFile(string assetId, string fileName)
        {
            DownloadProgressAssetApi downloadApi = new DownloadProgressAssetApi("http://" + serverHost,
                assetId,
                currentPlatform,
                persistentFilePath + MEHoloEntrance.Instance.AppID + "/",
                fileName,
                cacheFilePath + MEHoloEntrance.Instance.AppID + "/"
                );

            downloadWorker.SubmitWork(downloadApi);
        }

        /// <summary>
        /// 下载一个Recording中的Model文件 
        /// </summary>
        /// <param name="recordId"></param>
        /// <param name="fileName"></param>
        private void DownloadRecordingModelFile(string recordId, string fileName)
        {
            DownloadProgressRecordingModelApi downloadApi = new DownloadProgressRecordingModelApi("http://" + serverHost,
                recordId,
                persistentFilePath + MEHoloEntrance.Instance.AppID + "/",
                fileName,
                cacheFilePath + MEHoloEntrance.Instance.AppID + "/"
                );

            downloadWorker.SubmitWork(downloadApi);
        }

        /// <summary>
        /// 加载完成，调用回调
        /// </summary>
        /// <param name="filePath"></param>
        private void StorageReady(string filePath)
        {
            Debug.Log("File Ready [" + filePath + "]");

            // 判断文件存在之后才能调用接口 
            if (cbDownloadResourceFinish != null)
                cbDownloadResourceFinish(filePath);
        }


        // Update is called once per frame
        void Update()
        {
            if (isWaitingList)
            {
                if (listFinish)
                {
                    isWaitingList = false;
                    isBusy = false;
                    bool succ = (err == null);

                    if (cbLoadPageFinish != null)
                    {
                        cbLoadPageFinish(succ, err);
                        cbLoadPageFinish = null;
                    }
                }
            }

            if (isWaitingDownload)
            {
                double progress = 0;
                try
                {
                    progress = downloadWorker.GetTotalProgress();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Download Error! " + e);
                }
                if (progress >= 1)
                {
                    isWaitingDownload = false;
                    isBusy = false;
                    StorageReady(downloadFilePath);
                }
            }
        }


        void OnApplicationQuit()
        {
            if (downloadWorker != null)
                downloadWorker.StopWorker();
        }


    }

}