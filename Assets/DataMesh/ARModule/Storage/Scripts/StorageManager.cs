using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DataMesh.AR.Interactive;
using DataMesh.AR.UI;
using MEHoloClient.Core.Entities;
using MEHoloClient.Interface.Storage;
using MEHoloClient.Interface.Director;

namespace DataMesh.AR.Storage
{
    public class StorageManager : DataMesh.AR.MEHoloModuleSingleton<StorageManager>
    {
        public enum StorageResourceType
        {
            Asset,
            Record,
            DirectorScript
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
        /// 服务器端口
        /// </summary>
        public int serverPort = 8848;

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
        //private System.Action<bool, string> cbLoadPageFinish;
        private System.Action<bool> cbDownloadResourceFinish;

        [HideInInspector]
        public int curPage = 1;
        [HideInInspector]
        public int totalCount = 0;

        private AssetApi assetAPI;
        private RecordingApi recordingAPI;
        private DirectorApi directorAPI;
        private BackgroundWorker downloadWorker;


        private bool isWaitingList = false;
        private bool isWaitingDownload = false;
        private bool listFinish = false;
        private string err = null;
        //private bool downloadFinish = false;
        private string downloadFilePath;

        private static string persistentFilePath;
        private static string cacheFilePath;


        public StorageUI GetUI()
        {
            return ui;
        }

        public bool IsBusy()
        {
            return isBusy;
        }

        public string GetFileNameFromAsset(Asset asset)
        {
            string fileName = null;
            if (asset.contents != null)
            {
                for (int i = 0; i < asset.contents.Length; i++)
                {
                    AssetContent content = asset.contents[i];
                    if (content.platform == currentPlatform)
                    {
                        fileName = content.bundleHash;
                    }
                }
            }

            return fileName;
        }

        public static string GetFilePath()
        {
            return persistentFilePath + MEHoloEntrance.Instance.AppID + "/";
        }
        public static string GetCacheFilePath()
        {
            return cacheFilePath + MEHoloEntrance.Instance.AppID + "/";
        }

        protected override void _Init()
        {

#if UNITY_STANDALONE || UNITY_EDITOR
            currentPlatform = Platform.pc;
#elif UNITY_IOS
            currentPlatform = Platform.ios;
#elif UNITY_ANDROID
            currentPlatform = Platform.android;
#elif UNITY_WSA
            currentPlatform = Platform.uwp;
#endif

            InitFilePath();

            ui = GetComponent<StorageUI>();
            ui.Init(this);
        }

        private void InitFilePath()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            persistentFilePath = Application.dataPath + "/../Storage/";
#else
            persistentFilePath = Application.persistentDataPath + "/Storage/";
#endif
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            cacheFilePath = Application.dataPath + "/../CacheFile/Storage/";
#else
            cacheFilePath = Application.temporaryCachePath + "/Storage/";
#endif

        }

        protected override void _TurnOn()
        {
            Utility.AppConfig config = Utility.AppConfig.Instance;
            config.LoadConfig(MEHoloConstant.NetworkConfigFile);
            serverHost = Utility.AppConfig.Instance.GetConfigByFileName(MEHoloConstant.NetworkConfigFile, "Server_Host", "127.0.0.1:8848");
            int.TryParse(Utility.AppConfig.Instance.GetConfigByFileName(MEHoloConstant.NetworkConfigFile, "Server_Port", "8848"), out serverPort);

            ui.TurnOn();

            Debug.Log("App[" + MEHoloEntrance.Instance.AppID + "] Get Storage From " + serverHost);
            //下面的暂时只能使用https:// 无法去掉
            assetAPI = new AssetApi(serverHost, serverPort);
            recordingAPI = new RecordingApi(serverHost, serverPort);
            directorAPI = new DirectorApi(serverHost, serverPort); 

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

            //cbLoadPageFinish = cbFinish;
            currentListResourceType = type;

            /*
            isWaitingList = true;
            listFinish = false;
            listResult = null;
            isBusy = true;
            err = null;
            */
            curPage = currentPage;

            if (type == StorageResourceType.Asset)
                GetAssetList(curPage, countPerPage, cbFinish);
            else if (type == StorageResourceType.Record)
                GetRecordList(curPage, countPerPage, cbFinish);
            else if (type == StorageResourceType.DirectorScript)
                GetDirectorScriptList(curPage, countPerPage, cbFinish);
        }

        private async void GetAssetList(int page, int countPerPage, System.Action<bool, string> cbFinish)
        {
            Page<Asset> pageResult = null;
            // 加载数据 
            try
            {
                pageResult = await assetAPI.ListAssets(MEHoloEntrance.Instance.AppID, currentPlatform, page, countPerPage);
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

            //listFinish = true;
            MEHoloEntrance.Instance.Dispatch((param) =>
            {
                isBusy = false;
                bool succ = (err == null);

                if (cbFinish != null)
                {
                    cbFinish(succ, err);
                }
            });

        }


        private async void GetRecordList(int page, int countPerPage, System.Action<bool, string> cbFinish)
        {
            Page<Recording> pageResult = null;
            // 加载数据 
            try
            {
                pageResult = await recordingAPI.ListRecordings(MEHoloEntrance.Instance.AppID, page, countPerPage);
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

            //listFinish = true;
            MEHoloEntrance.Instance.Dispatch((param) =>
            {
                isBusy = false;
                bool succ = (err == null);

                if (cbFinish != null)
                {
                    cbFinish(succ, err);
                }
            });
        }

        private async void GetDirectorScriptList(int page, int countPerPage, System.Action<bool, string> cbFinish)
        {
            Page<DirectorScript> pageResult = null;
            // 加载数据 
            try
            {
                pageResult = await directorAPI.ListScripts(MEHoloEntrance.Instance.AppID, null, page, countPerPage);
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

            //listFinish = true;
                        MEHoloEntrance.Instance.Dispatch((param) =>
            {
                isBusy = false;
                bool succ = (err == null);

                if (cbFinish != null)
                {
                    cbFinish(succ, err);
                }
            });
        }

        /// <summary>
        /// 根据一个Asset信息，下载实际的Asset内容
        /// </summary>
        /// <param name="asset"></param>
        public void DownloadAsset(Asset asset, System.Action<bool> cbFinish)
        {
            string fileName = GetFileNameFromAsset(asset);

            cbDownloadResourceFinish = cbFinish;
            if (fileName == null)
            {
                // 没找到文件，直接返回null; 
                StorageReady(false);
            }
            else
            {
                string filePath = GetFilePath();
                if (!CheckStorageFileExist(asset.id, fileName))
                {
                    isWaitingDownload = true;
                    //downloadFinish = false;
                    downloadFilePath = filePath;
                    isBusy = true;

                    DownloadAssetFile(asset.id, fileName);
                }
                else
                {
                    // 文件存在，直接返回 
                    StorageReady(true);
                }
            }
        }

        public class DownloadAssetInfo
        {
            /// <summary>
            /// 申请下载的Asset的ID
            /// </summary>
            public string id;
            
            /// <summary>
            /// 申请下载的Asset名字
            /// </summary>
            public string name;

            /// <summary>
            /// 申请下载的Asset，对应当前平台的Hash。
            /// 传入时无需填写，在下载后会回填此值。
            /// </summary>
            public string hash;
        }

        private async void QueryAssetHash(Dictionary<string, StorageManager.DownloadAssetInfo> infoList, System.Action<Dictionary<string, StorageManager.DownloadAssetInfo>> OnQueryFinish)
        {
            // 先连接server，检查所有Asset对应当前平台的文件Hash，并回填数据 
            string[] assetIds = new string[infoList.Count];
            int index = 0;
            foreach (DownloadAssetInfo info in infoList.Values)
            {
                assetIds[index] = info.id;
                index++;
            }
            try
            {
                List<Asset> queryRs = await assetAPI.ListAssetsByIds(assetIds, currentPlatform);
                Debug.Log("Query find [" + queryRs.Count + "] assets!");
                for (int i = 0;i < queryRs.Count;i ++)
                {
                    Asset asset = queryRs[i];
                    if (infoList.ContainsKey(asset.id))
                    {
                        for (int j = 0;j < asset.contents.Length;j ++)
                        {
                            AssetContent content = asset.contents[j];
                            if (content.platform == currentPlatform)
                            {
                                DownloadAssetInfo info = infoList[asset.id];
                                info.hash = content.bundleHash;
                                Debug.Log("Fill Hash for asset [" + asset.id + "]");
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                err = e.ToString();
            }

            MEHoloEntrance.Instance.Dispatch((param)=>{
                if (OnQueryFinish != null)
                {
                    OnQueryFinish(infoList);
                }
            });
        }

        private void OnQueryFinishDelegate(object param)
        {

        }

        /// <summary>
        /// 下载一系列Asset资源
        /// 因为Asset资源对应不同平台会有不同的文件，所以需要先查询这些资源所对应平台的Hash
        /// </summary>
        /// <param name="infoList"></param>
        /// <param name="cbFinish"></param>
        public void DownloadAssets(Dictionary<string, DownloadAssetInfo> infoList, System.Action<bool> cbFinish)
        {
            cbDownloadResourceFinish = cbFinish;

            QueryAssetHash(infoList, (returnList) =>
            {
                int count = 0;
                foreach (DownloadAssetInfo info in returnList.Values)
                {
                    if (info.hash == null)
                    {
                        Debug.Log("Hash of asset[" + info.id + "] not found!");
                        StorageReady(false);
                        return;
                    }

                    if (!CheckStorageFileExist(info.id, info.hash))
                    {
                        Debug.Log("---> [" + info.id + "] need download");
                        DownloadAssetFile(info.id, info.hash);
                        count++;
                    }
                }

                if (count > 0)
                {
                    Debug.Log("Need Download " + count + " res, Waiting....");
                    isWaitingDownload = true;
                    //downloadFinish = false;
                    //downloadFilePath = filePath;
                    isBusy = true;
                }
                else
                {
                    Debug.Log("Don't need Donwload!");
                    StorageReady(true);
                }
            }
            );



        }

        /// <summary>
        /// 根据一个Recording信息，下载其中包含的Model资源
        /// </summary>
        /// <param name="record"></param>
        /// <param name="cbFinish"></param>
        public void DownloadRecordingModel(Recording record, System.Action<bool> cbFinish)
        {
            string fileName = record.modelHash;
            long fileSize = record.modelSize;

            cbDownloadResourceFinish = cbFinish;
            if (fileName == null)
            {
                // 没找到文件，直接返回null; 
                StorageReady(false);
            }
            else
            {
                string filePath = GetFilePath();
                if (!CheckStorageFileExist(record.id, fileName))
                {
                    isWaitingDownload = true;
                    //downloadFinish = false;
                    downloadFilePath = filePath;
                    isBusy = true;

                    DownloadRecordingModelFile(record.id, fileName);
                }
                else
                {
                    // 文件存在，直接返回 
                    StorageReady(true);
                }
            }
        }

        /// <summary>
        /// 根据一个Recording信息，下载其中包含的Model资源
        /// </summary>
        /// <param name="record"></param>
        /// <param name="cbFinish"></param>
        public void DownloadDirectorScript(string id, string fileName, System.Action<bool> cbFinish)
        {

            cbDownloadResourceFinish = cbFinish;
            if (fileName == null)
            {
                // 没找到文件，直接返回null; 
                StorageReady(false);
            }
            else
            {
                string filePath = GetFilePath();
                if (!CheckStorageFileExist(id, fileName))
                {
                    isWaitingDownload = true;
                    //downloadFinish = false;
                    downloadFilePath = filePath;
                    isBusy = true;

                    DownloadDirectorScriptFile(id, fileName);
                }
                else
                {
                    // 文件存在，直接返回 
                    StorageReady(true);
                }
            }
        }

        /// <summary>
        /// 检查一个资源是否已经下载过，
        /// </summary>
        /// <param name="resId">资源的ID</param>
        /// <param name="fileName">存储的目标文件</param>
        private bool CheckStorageFileExist(string resId, string fileName)
        {
            string savePath = GetFilePath();
            string filePath = savePath + fileName;
            bool isExist = false;
            if (File.Exists(filePath))
            {
                // 文件不存在，标记为需要下载 
                isExist = true;
            }
            return isExist;
        }

        /// <summary>
        /// 下载一个Asset的内容
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="fileName"></param>
        private void DownloadAssetFile(string assetId, string fileName)
        {
            Debug.Log("server=" + serverHost + " assetId=" + assetId + " platform=" + currentPlatform);
            DownloadProgressAssetApi downloadApi = new DownloadProgressAssetApi(
                serverHost,
                serverPort,
                assetId,
                currentPlatform,
                GetFilePath(),
                fileName,
                GetCacheFilePath()
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
            DownloadProgressRecordingModelApi downloadApi = new DownloadProgressRecordingModelApi(
                serverHost,
                serverPort,
                recordId,
                GetFilePath(),
                fileName,
                GetCacheFilePath()
                );

            downloadWorker.SubmitWork(downloadApi);
        }

        /// <summary>
        /// 下载一个Director的内容
        /// </summary>
        /// <param name="scriptId"></param>
        /// <param name="fileName"></param>
        private void DownloadDirectorScriptFile(string scriptId, string fileName)
        {
            DownloadProgressScriptApi downloadApi = new DownloadProgressScriptApi(
                serverHost,
                serverPort,
                scriptId,
                GetFilePath(),
                fileName,
                GetCacheFilePath()
                );

            downloadWorker.SubmitWork(downloadApi);
        }

        /// <summary>
        /// 加载完成，调用回调
        /// </summary>
        /// <param name="filePath"></param>
        private void StorageReady(bool success)
        {
            Debug.Log("Download Result=[" + success + "]");

            // 判断文件存在之后才能调用接口 
            if (cbDownloadResourceFinish != null)
                cbDownloadResourceFinish(success);



        }


        // Update is called once per frame
        void Update()
        {
            /*
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
            */

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
                    StorageReady(true);
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