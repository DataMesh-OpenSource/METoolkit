using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DataMesh.AR.Account;
using DataMesh.AR.Interactive;
using DataMesh.AR.UI;
using MEHoloClient.Core.Entities;
using MEHoloClient.Interface.Library;
using MEHoloClient.Interface.Storage;
using MEHoloClient.Interface.Director   ;

namespace DataMesh.AR.Library
{
    public class LibraryManager : DataMesh.AR.MEHoloModuleSingleton<LibraryManager>
    {
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

        public enum ResourceType
        {
            DirectorScript
        }

        /// <summary>
        /// 查询一类资源的结果
        /// </summary>
        [HideInInspector]
        public List<BaseDcsResource> listResult;

        [HideInInspector]
        public int curPage = 1;
        [HideInInspector]

        /// <summary>
        /// 加载资源列表时，加载完成后会触发此回调
        /// </summary>
        private System.Action<bool> cbDownloadResourceFinish;

        private ResourceType currentListResourceType;

        private bool isBusy = false;

        private BackgroundWorker downloadWorker;

        private LibraryUI ui;

        public int totalCount = 0;


        private bool isWaitingList = false;
        private bool isWaitingDownload = false;
        private bool listFinish = false;
        private string err = null;
        //private bool downloadFinish = false;
        private string downloadFilePath;

        private static string persistentFilePath;
        private static string cacheFilePath;

        public LibraryUI GetUI()
        {
            return ui;
        }

        public bool IsBusy()
        {
            return isBusy;
        }

        /*
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
        */

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
            InitFilePath();

            ui = GetComponent<LibraryUI>();
            ui.Init(this);
        }

        private void InitFilePath()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            persistentFilePath = Application.dataPath + "/../DCSLibrary/";
#else
            persistentFilePath = Application.persistentDataPath + "/Storage/";
#endif
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            cacheFilePath = Application.dataPath + "/../CacheFile/DCSLibrary/";
#else
            cacheFilePath = Application.temporaryCachePath + "/Storage/";
#endif

        }

        protected override void _TurnOn()
        {
            ui.TurnOn();

            downloadWorker = new BackgroundWorker();
            downloadWorker.StartWorker();

        }

        protected override void _TurnOff()
        {

        }



        /// <summary>
        /// 获取Asset列表
        /// </summary>
        public void GetStorageResourceList(ResourceType type, int currentPage, int countPerPage, System.Action<bool, string> cbFinish)
        {
            if (isBusy)
                return;

            currentListResourceType = type;

            curPage = currentPage;

            if (type == ResourceType.DirectorScript)
                GetDirectorScriptList(curPage, countPerPage, cbFinish);
        }

        private async void GetDirectorScriptList(int page, int countPerPage, System.Action<bool, string> cbFinish)
        {
            Pagination<DcsDirecorScript> pageResult = null;
            DirectorApi api = new DirectorApi("https://Director." + AccountManager.Instance.RegionCode + ".datamesh.com");
            // 加载数据 
            try
            {
                pageResult = await api.ListDcsScripts(
                    AccountManager.Instance.UserCredential.AccessKey,
                    AccountManager.Instance.UserCredential.AccessSecret,
                    "",
                    null,
                    null,
                    page,
                    countPerPage
                    );
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                err = e.ToString();
            }


            if (err == null)
            {
                // 处理数据
                totalCount = pageResult.TotalHits;
                Debug.Log("Storage find " + pageResult.TotalHits + " item, Total Page: " + pageResult.TotalPage);

                listResult = new List<BaseDcsResource>();
                if (pageResult.Data != null)
                {
                    for (int i = 0; i < pageResult.Data.Length; i++)
                    {
                        listResult.Add(pageResult.Data[i]);
                    }
                }
            }

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



        private async void QueryAssetHash(Dictionary<string, LibraryManager.DownloadAssetInfo> infoList, System.Action<Dictionary<string, LibraryManager.DownloadAssetInfo>> OnQueryFinish)
        {
            // 先连接server，检查所有Asset对应当前平台的文件Hash，并回填数据 
            List<string> assetIds = new List<string>();
            int index = 0;
            foreach (DownloadAssetInfo info in infoList.Values)
            {
                assetIds.Add(info.id);
                index++;
            }

            DcsLibraryEntityType currentPlatform;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            currentPlatform = DcsLibraryEntityType.EntityTypeAssetPC;
#elif UNITY_IOS
            currentPlatform = DcsLibraryEntityType.EntityTypeAssetIOS;
#elif UNITY_ANDROID
            currentPlatform = DcsLibraryEntityType.EntityTypeAssetAndroid;
#elif UNITY_WSA
            currentPlatform = DcsLibraryEntityType.EntityTypeAssetUWP;
#endif

            LibraryApi libraryApi = new LibraryApi("https://library." + AccountManager.Instance.RegionCode + ".datamesh.com");
            try
            {
                List<DcsLibraryResource> queryRs = await libraryApi.ListResourcesByIds(
                    AccountManager.Instance.UserCredential.AccessKey,
                    AccountManager.Instance.UserCredential.AccessSecret,
                    assetIds,
                    currentPlatform);
                Debug.Log("Query find [" + queryRs.Count + "] assets!");
                for (int i = 0; i < queryRs.Count; i++)
                {
                    DcsLibraryResource resource = queryRs[i];
                    Debug.Log("entitis length=" + resource.Entities.Length);
                    for (int j = 0;j < resource.Entities.Length;j ++)
                    {
                        DcsLibraryEntity asset = resource.Entities[j];
                        if (asset.EntityType == currentPlatform)
                        {
                            if (infoList.ContainsKey(resource.Id))
                            {
                                DownloadAssetInfo info = infoList[resource.Id];
                                info.hash = asset.Id;
                                Debug.Log("Fill Hash for asset [" + resource.Id + "]=[" + asset.Id + "]");
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
            

            MEHoloEntrance.Instance.Dispatch((param) => {
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
                        DownloadFinish(false);
                        return;
                    }

                    if (!CheckStorageFileExist(info.id, info.hash))
                    {
                        Debug.Log("---> [" + info.id + "] need download");
                        DownloadAssetFile(info.hash, info.hash);
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
                    DownloadFinish(true);
                }
            }
            );



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
        private void DownloadAssetFile(string entityId, string fileName)
        {
            var downloadProgressScriptApi = new LibraryDownloadWithProgressApi(
                "https://library." + AccountManager.Instance.RegionCode + ".datamesh.com",
                false,
                AccountManager.Instance.UserCredential.AccessKey,
                AccountManager.Instance.UserCredential.AccessSecret,
                entityId,
                GetFilePath(),
                fileName,
                GetCacheFilePath()
                );
            downloadWorker.SubmitWork(downloadProgressScriptApi);
            
        }



        /// <summary>
        /// 下载一个director脚本
        /// </summary>
        /// <param name="record"></param>
        /// <param name="cbFinish"></param>
        public void DownloadDirectorScript(string id, string fileName, System.Action<bool> cbFinish)
        {
            Debug.Log("Script id=" + id);
            Debug.Log("Script file=" + fileName);

            cbDownloadResourceFinish = cbFinish;
            if (fileName == null)
            {
                // 没找到文件，直接返回null; 
                DownloadFinish(false);
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

                    Debug.Log("File not exist, Need download!");

                    DownloadDirectorScriptFile(id, fileName);
                }
                else
                {
                    Debug.Log("File exist!");

                    // 文件存在，直接返回 
                    DownloadFinish(true);
                }
            }
        }

        /// <summary>
        /// 下载一个Director的内容
        /// </summary>
        /// <param name="scriptId"></param>
        /// <param name="fileName"></param>
        private void DownloadDirectorScriptFile(string scriptId, string fileName)
        {
            Debug.Log("Start download....");

            DcsDownloadProgressScriptApi downloadApi = new DcsDownloadProgressScriptApi(
                "https://director." + AccountManager.Instance.RegionCode + ".datamesh.com",
                AccountManager.Instance.UserCredential.AccessKey,
                AccountManager.Instance.UserCredential.AccessSecret,
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
        private void DownloadFinish(bool success)
        {
            // 判断文件存在之后才能调用接口 
            if (cbDownloadResourceFinish != null)
                cbDownloadResourceFinish(success);
        }


        // Update is called once per frame
        void Update()
        {

            if (isWaitingDownload)
            {
                double progress = 0;
                try
                {
                    progress = downloadWorker.GetTotalProgress();
                    Debug.Log("download...." + progress);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Download Error! " + e);
                }
                if (progress >= 1)
                {
                    isWaitingDownload = false;
                    isBusy = false;
                    DownloadFinish(true);
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