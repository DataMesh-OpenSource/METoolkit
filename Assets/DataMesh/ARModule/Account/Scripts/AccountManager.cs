using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

using DataMesh.AR.Utility;
using DataMesh.AR.UI;
using MEHoloClient.Interface.Account;
using MEHoloClient.Utils;
using MEHoloClient.Core.Entities;

using System.Net;

namespace DataMesh.AR.Account
{

    public class AccountManager : MEHoloModuleSingleton<AccountManager>
    {
        public const int LOGIN_RESULT_SUCCESS = 0;
        public const int LOGIN_RESULT_UNKNOWN_ERROR = 1;

        public string DCSAddress = "https://portal.datamesh.com";

        /// <summary>
        /// 用户识别ID
        /// </summary>
        [HideInInspector]
        public string clientId = "";

        public LoginUI loginUI;
        public BlockList regionList;
        public GameObject loadingPrefab;

        public System.Action callbackLoginSuccess;

        private AccountApi loginApi;

        private LoadingUI loadingUI;


        private string accountDataSavePath;
        private string loginSaveFile = "LoginData";
        private string regionSaveFile = "RegionData";

        private UserCredential currentUserCredential = null;
        private string tenantId = null;
        private string userName = null;
        private string currentRegionCode = null;

        private bool hasLogin = false;

        public UserCredential UserCredential
        {
            get { return currentUserCredential; }
        }

        public string TenantId
        { 
            get { return tenantId; }
        }

        public string UserName
        {
            get { return userName; }
        }

        public string RegionCode
        {
            get { return currentRegionCode; }
        }

        private void InitFilePath()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            accountDataSavePath = Application.dataPath + "/../";
#else
            accountDataSavePath = Application.persistentDataPath;
#endif
        }


        protected override void Awake()
        {
            base.Awake();

            loginUI.gameObject.SetActive(false);
            regionList.gameObject.SetActive(false);




        }

        protected override void _Init()
        {
            // 读取存储数据 
            InitFilePath();
            LoadLoginData();
            LoadRegionData();

            clientId = SystemInfo.deviceUniqueIdentifier;
            Debug.Log("client=" + clientId);

            clientId = MD5Hash.Hash(Encoding.UTF8.GetBytes(clientId));
            Debug.Log("client=" + clientId);

            loginUI.Init();
            loginUI.callbackLogin = OnLoginInput;

            regionList.AddCallbackChangePage(OnRegionChangePage);
            regionList.AddCallbackClick(OnClickRegion);

            regionList.Init();

            GameObject obj = Instantiate(loadingPrefab) as GameObject;
            obj.transform.SetParent(this.transform);
            obj.transform.localPosition = new Vector3(0, 0, 2) ;
            obj.transform.localRotation = Quaternion.identity;
            loadingUI = obj.GetComponent<LoadingUI>();
            loadingUI.Init();


#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;
#endif
        }

        public void OnRegionChangePage(int page)
        {
            regionList.ChangePage(page);
        }

        public void OnClickRegion(BlockListData data)
        {
            // 选择一个区 
            RegionList.Region region = data.data as RegionList.Region;
            currentRegionCode = region.RegionCode;

            SaveRegionData();

            regionList.Hide();

            LoginFinish();
        }


        protected override void _TurnOn()
        {
            loginApi = new AccountApi(DCSAddress);

            // 检查登录信息
            if (currentUserCredential != null)
            {
                
                loadingUI.ShowLoading("Check your credential...");

                // 如果存在登录信息，则直接验证
                GetUserInfo(currentUserCredential);
            }
            else
            {
                loadingUI.Hide();
                loginUI.Show();
            }
        }

        protected override void _TurnOff()
        {
            loginUI.Hide();
            loadingUI.Hide();
        }

        #region 登录相关

        public bool HasLogin()
        {
            return hasLogin;
        }


        private void OnLoginInput(string name, string pass)
        {
            Debug.Log("Login! name=" + name + " pass=" + pass);
            loginUI.Hide();
            loadingUI.ShowLoading("Now Loading, please wait...");
            Login(name, pass);
        }



        /// <summary>
        /// 用账号密码登录
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pass"></param>
        private async void Login(string name, string pass)
        {
            // 调用登录API

            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;

            int errorCode = LOGIN_RESULT_SUCCESS;
            try
            {
                UserCredential response = await loginApi.Login(name, pass, clientId);
                if (response != null)
                {
                    currentUserCredential = response;
                    SaveLoginData();
                }

            }
            catch (MEHoloClient.MeshException.HoloClientException me)
            {
                Debug.LogWarning("Login Error! errorcode=" + me.ErrorCode);
                errorCode = me.ErrorCode;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Exception! " + e);
                errorCode = LOGIN_RESULT_UNKNOWN_ERROR;
            }

            OnLoginResult(errorCode);
        }

        private void OnLoginResult(int resultCode)
        {
            loadingUI.Hide();

            switch (resultCode)
            {
                case LOGIN_RESULT_SUCCESS:
                    SaveLoginData();
                    GetUserInfo(currentUserCredential);
                    break;
                default:
                    loginUI.ShowError("Login Error! " + resultCode);
                    loginUI.Show();
                    break;
            }
        }

        /// <summary>
        /// 用已经记录的身份信息登录
        /// </summary>
        /// <param name="cred"></param>
        private async void GetUserInfo(UserCredential credential)
        {
            UserInfo info = null;
            try
            {
                info = await loginApi.GetUserInfo(credential.AccessKey, credential.AccessSecret);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Exception! " + e);
            }

            if (info != null && !string.IsNullOrEmpty(info.Uid))
            {
                // 存储用户信息 
                tenantId = info.TenantId;
                userName = info.Email;

                // 信息有效，开始检查区域 
                CheckRegionSelect();
            }
            else
            {
                // 信息无效，传入错误 
                loadingUI.Hide();
                loginUI.ShowError("Login Error!");
                loginUI.Show();
            }
        }






        #endregion

        #region 选区相关

        private void CheckRegionSelect()
        {
            if (currentRegionCode == null)
            {
                // 如果没有选过区，则开启选区界面 
                GetRegionList();
            }
            else
            {
                // 如果选过区，则直接进入 
                LoginFinish();
            }
        }

        private async void GetRegionList()
        {
            RegionList list = null;

            loadingUI.ShowLoading("Getting region list...");

            try
            {
                list = await loginApi.ListRegions();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Exception! " + e);
            }

            OnRegionDownload(list);
        }

        private void OnRegionDownload(RegionList list)
        {
            loadingUI.Hide();

            if (list == null)
            {
                loginUI.ShowError("Login Error!");
                loginUI.Show();

                return;
            }

            List<BlockListData> uiDataList = new List<BlockListData>();
            for (int i = 0;i < list.Regions.Count;i ++)
            {
                RegionList.Region region = list.Regions[i];

                RegionItemData data = new RegionItemData();
                data.name = region.RegionName;
                data.data = region;

                uiDataList.Add(data);
            }
            regionList.SetData(uiDataList, 1, uiDataList.Count);

            regionList.Show();
        }


        private void LoginFinish()
        {
            hasLogin = true;
            if (callbackLoginSuccess != null)
                callbackLoginSuccess();
        }

        #endregion

        #region 文件操作
        ///////////////////// 文件操作 /////////////////

        /// <summary>
        /// 从存盘文件中读取登录信息
        /// </summary>
        private void LoadLoginData()
        {
            Dictionary<string, string> data = AppConfig.AnalyseConfigFile(accountDataSavePath + loginSaveFile);
            if (data != null)
            {
                currentUserCredential = new UserCredential();
                if (data.ContainsKey("access_key"))
                {
                    currentUserCredential.AccessKey = data["access_key"];
                }
                if (data.ContainsKey("access_secret"))
                {
                    currentUserCredential.AccessSecret = data["access_secret"];
                }
                if (data.ContainsKey("expire_at"))
                {
                    long rs = 0;
                    long.TryParse(data["expire_at"], out rs);
                    currentUserCredential.ExpireAt = rs;
                }

                Debug.Log("Load Login data!");
                Debug.Log("key=" + currentUserCredential.AccessKey);
                Debug.Log("Secret=" + currentUserCredential.AccessSecret);
                Debug.Log("Expire=" + currentUserCredential.ExpireAt);
            }
            else
            {
                Debug.Log("There are no login data. Need relogin");
            }
        }

        /// <summary>
        /// 将登录信息写入存盘文件
        /// </summary>
        private void SaveLoginData()
        {
            if (currentUserCredential == null)
                return;

            Dictionary<string, string> data = new Dictionary<string, string>();

            data.Add("access_key", currentUserCredential.AccessKey);
            data.Add("access_secret", currentUserCredential.AccessSecret);
            data.Add("expire_at", currentUserCredential.ExpireAt.ToString());

            Debug.Log("Save login file [" + accountDataSavePath + loginSaveFile + "]");
            AppConfig.SaveConfigFile(accountDataSavePath + loginSaveFile, data);

            Debug.Log("Login Data Saved!");
        }

        private void LoadRegionData()
        {
            Dictionary<string, string> data = AppConfig.AnalyseConfigFile(accountDataSavePath + regionSaveFile);
            if (data != null)
            {
                if (data.ContainsKey("region_code"))
                {
                    currentRegionCode = data["region_code"];
                }

                Debug.Log("Load Region data!");
                Debug.Log("code=" + currentRegionCode);
            }
            else
            {
                Debug.Log("There are no region data. Need select region");
            }
        }

        /// <summary>
        /// 将登录信息写入存盘文件
        /// </summary>
        private void SaveRegionData()
        {
            if (currentRegionCode == null)
                return;

            Dictionary<string, string> data = new Dictionary<string, string>();

            data.Add("region_code", currentRegionCode);

            Debug.Log("Save region file [" + accountDataSavePath + regionSaveFile + "]");
            AppConfig.SaveConfigFile(accountDataSavePath + regionSaveFile, data);

            Debug.Log("Region Data Saved!");
        }

        #endregion
    }

}