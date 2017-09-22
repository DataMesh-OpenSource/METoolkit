using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.Networking;
using MEHoloClient.Utils;

namespace DataMesh.AR.SpectatorView
{
    public class LiveWDPController : DataMesh.AR.Utility.Singleton<LiveWDPController>
    {
        public string authorId;
        public string authorPass;

        [HideInInspector]
        public LiveController liveController;
        public LiveControllerUI panel;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN


        private class SpectatorViewAppInfo
        {
            public string appid;
            public string package;
        }

        [HideInInspector]
        public const string SpectatorViewAppName = "DataMeshLiveAgent";

        private string networkInfo;

        private string networkID = null;

        private SpectatorViewAppInfo appInfo;

        float checkBatteryTime = 0;
        float checkBatteryInterval = 30;

        private bool hasTurnOn = false;

        private LiveWDPController() { }

        public void Init(LiveController b, LiveControllerUI p)
        {
            liveController = b;
            panel = p;
        }

        public void TurnOn()
        {
            panel.spectatorViewHololensPanel.OnOpenStatusPanel(null);

            hasTurnOn = true;
        }

        public void OnOpenStatusPanel()
        {
            panel.hololensStatusPanel.InitShow();
            if (LiveParam.AuthorId == null)
            {
                panel.hololensStatusPanel.ShowLogin();
            }
            else
            {
                panel.hololensStatusPanel.ShowStatus();
                BeginGetStatus();
            }
        }

        public void Login(string author, string pass)
        {
            LiveParam.AuthorId = author.Trim();
            LiveParam.AuthorPass = pass.Trim();

            panel.hololensStatusPanel.ShowStatus();

            BeginGetStatus();
        }

        private void BeginGetStatus()
        {
            GetNetworkStatus();

            // 继续加载app信息 
            StartCoroutine(GetAppStatus());

            checkBatteryTime = Time.realtimeSinceStartup - checkBatteryInterval + 5;

        }

        /// <summary>
        /// 获取电池电量
        /// </summary>
        public void GetBatteryStatus()
        {
            string url = "http://localhost:10080/api/power/battery";
            Dictionary<string, string> headers = new Dictionary<string, string>();

            string author = LiveParam.AuthorId + ":" + LiveParam.AuthorPass;
            byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(author);
            string authorBase = System.Convert.ToBase64String(bytes);
            Debug.Log("author base64=" + authorBase);

            headers.Add("authorization", "Basic " + authorBase);
            Debug.Log("Try to get [" + url + "]");
            WWW www = GET(url, headers, GetBatteryComplete, GetBatteryStatusError);

        }

        private void GetBatteryComplete(string result)
        {
            Debug.Log("Get Complete!!!");

            Debug.Log(result);

            BatteryState battery = JsonUtil.Deserialize<BatteryState>(result);

            bool isCharging = (battery.Charging == 1);
            int cap = (int)((float)battery.RemainingCapacity / (float)battery.MaximumCapacity * 100);
            panel.hololensStatusPanel.batteryStatus.SetBatteryStatus(isCharging, cap);

        }

        private void GetBatteryStatusError(string error)
        {
            Debug.LogError("Request error! " + error);

            if (!CheckAuthorized(error))
                return;
            //panel.hololensStatusPanel.loginErrorText.text = "Request Error! \n" + error;
        }


        /// <summary>
        /// 获取网络IP等信息
        /// </summary>
        public void GetNetworkStatus()
        {
            networkID = null;

            panel.hololensStatusPanel.ShowNetworkWaiting();

            string url = "http://localhost:10080/api/networking/ipconfig";
            Dictionary<string, string> headers = new Dictionary<string, string>();

            string author = LiveParam.AuthorId + ":" + LiveParam.AuthorPass;
            byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(author);
            string authorBase = System.Convert.ToBase64String(bytes);
            Debug.Log("author base64=" + authorBase);

            headers.Add("authorization", "Basic " + authorBase);
            Debug.Log("Try to get [" + url + "]");
            WWW www = GET(url, headers, GetNetworkStatusComplete, GetNetworkStatusError);

        }

        private void GetNetworkStatusComplete(string result)
        {
            Debug.Log("Get Complete!!!");

            Debug.Log(result);

            NetworkStatus status = JsonUtil.Deserialize<NetworkStatus>(result);


            networkInfo = "";
            for (int i = 0;i < status.Adapters.Count;i ++)
            {
                Adapter adapter = status.Adapters[i];

                // 寻找无线网卡 
                if (adapter.Description.IndexOf("802.11") < 0)
                {
                    continue;
                }

                //networkInfo += adapter.Description + ":\n";
                networkID = adapter.Name;

                for (int j = 0; j < adapter.IpAddresses.Count; j++)
                {
                    networkInfo += "HoloLens IP = [" + adapter.IpAddresses[j].IpAddress + "]\n";
                }

                networkInfo += "\n";
            }


            if (networkID != null)
            {
                networkID = networkID.Trim();
                networkID = networkID.Substring(1, networkID.Length - 2);

                // 如果加载到网络信息，则继续加载wifi信息 
                StartCoroutine(GetConfigFile());
                StartCoroutine(GetWIFIProfile());
                StartCoroutine(GetWIFIStatus());

            }

            panel.hololensStatusPanel.ShowNetworkResult(networkInfo);

        }

        /// <summary>
        /// 获取SpectatorView的配置文件
        /// </summary>
        /// <returns></returns>
        private IEnumerator GetConfigFile()
        {
            yield return new WaitForSeconds(1);

        }

        private void GetNetworkStatusError(string error)
        {
            Debug.LogError("Request error! " + error);

            if (!CheckAuthorized(error))
                return;

            panel.hololensStatusPanel.ShowNetworkError();
        }

        /// <summary>
        /// 获取预存的Wifi Profile信息
        /// </summary>
        /// <param name="id"></param>
        public IEnumerator GetWIFIProfile()
        {
            if (networkID == null)
                yield break;

            yield return new WaitForSeconds(2);

            panel.hololensStatusPanel.ShowWifiProfileWaiting();

            string url = "http://localhost:10080/api/wifi/interfaces";
            Dictionary<string, string> headers = new Dictionary<string, string>();

            string author = LiveParam.AuthorId + ":" + LiveParam.AuthorPass;
            byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(author);
            string authorBase = System.Convert.ToBase64String(bytes);
            Debug.Log("author base64=" + authorBase);

            headers.Add("authorization", "Basic " + authorBase);
            Debug.Log("Try to get [" + url + "]");
            WWW www = GET(url, headers, GetWIFIProfileComplete, GetWIFIProfileError);

        }

        private void GetWIFIProfileComplete(string result)
        {
            Debug.Log(result);
            WifiInterfaces all = JsonUtil.Deserialize<WifiInterfaces>(result);

            List<string> profileName = new List<string>();
            for (int i = 0; i < all.Interfaces.Count; i++)
            {
                WifiInterface inter = all.Interfaces[i];
                string uid = inter.GUID;
                uid = uid.Trim();
                uid = uid.Substring(1, uid.Length - 2);

                Debug.Log("find uid=" + uid);
                Debug.Log("networkId=" + networkID);

                if (uid == networkID)
                {
                    for (int j = 0;j < inter.ProfilesList.Count;j ++)
                    {
                        WifiProfile profile = inter.ProfilesList[j];
                        profileName.Add(profile.Name);
                    }
                    Debug.Log("Find!!! profile count=" + profileName.Count);
                }

            }

            panel.hololensStatusPanel.ShowWifiProfileResult(profileName);


        }

        private void GetWIFIProfileError(string error)
        {
            Debug.LogError(error);

            if (!CheckAuthorized(error))
                return;

            panel.hololensStatusPanel.ShowWifiResult(false);
        }

        /// <summary>
        /// 获取Wifi列表信息
        /// </summary>
        /// <param name="id"></param>
        public IEnumerator GetWIFIStatus()
        {
            if (networkID == null)
                yield break;

            yield return new WaitForSeconds(3);

            panel.hololensStatusPanel.ShowWifiWaiting();

            panel.hololensStatusPanel.ClearWifiItem();

            string url = "http://localhost:10080/api/wifi/networks?interface=" + networkID;
            Dictionary<string, string> headers = new Dictionary<string, string>();

            string author = LiveParam.AuthorId + ":" + LiveParam.AuthorPass;
            byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(author);
            string authorBase = System.Convert.ToBase64String(bytes);
            Debug.Log("author base64=" + authorBase);

            headers.Add("authorization", "Basic " + authorBase);
            Debug.Log("Try to get [" + url + "]");
            WWW www = GET(url, headers, GetWIFIStatusComplete, GetWIFIError);

        }

        private void GetWIFIStatusComplete(string result)
        {
            Debug.Log(result);
            AllAvailableNetwork all = JsonUtil.Deserialize<AllAvailableNetwork>(result);

            Debug.Log(all.AvailableNetworks.Count);

            List<string> nameList = new List<string>();
            int connectedId = -1;
            for (int i = 0; i < all.AvailableNetworks.Count; i++)
            {
                AvailableNetwork net = all.AvailableNetworks[i];

                nameList.Add(net.SSID);
                if (net.AlreadyConnected)
                {
                    connectedId = i;
                }

            }


            panel.hololensStatusPanel.SetWifiItemList(nameList, connectedId);

            panel.hololensStatusPanel.ShowWifiResult(true);
        }

        private void GetWIFIError(string error)
        {
            Debug.LogError(error);

            if (!CheckAuthorized(error))
                return;

            panel.hololensStatusPanel.ShowWifiResult(false);
        }


        /// <summary>
        /// 检查SpectatorView版本
        /// </summary>
        public IEnumerator GetAppStatus()
        {
            yield return new WaitForSeconds(1);

            panel.hololensStatusPanel.ShowAppStatusWaiting();

            string url = "http://localhost:10080/api/app/packagemanager/packages";

            Dictionary<string, string> headers = new Dictionary<string, string>();

            string author = LiveParam.AuthorId + ":" + LiveParam.AuthorPass;
            byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(author);
            string authorBase = System.Convert.ToBase64String(bytes);
            Debug.Log("author base64=" + authorBase);

            headers.Add("authorization", "Basic " + authorBase);
            Debug.Log("Try to get [" + url + "]");
            WWW www = GET(url, headers, GetAppComplete, GetAppError);

        }

        private void GetAppComplete(string result)
        {
            Debug.Log(result);

            string version = null;
            InstalledApps apps = JsonUtil.Deserialize<InstalledApps>(result);
            for (int i = 0;i < apps.InstalledPackages.Count;i ++)
            {
                InstalledApp app = apps.InstalledPackages[i];
                if (app.Name == SpectatorViewAppName)
                {
                    appInfo = new SpectatorViewAppInfo();

                    byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(app.PackageRelativeId);
                    appInfo.appid = System.Convert.ToBase64String(bytes);

                    bytes = Encoding.GetEncoding("utf-8").GetBytes(app.PackageFullName);
                    appInfo.package = System.Convert.ToBase64String(bytes);


                    version = "" + app.Version.Major + "." + app.Version.Minor + "." + app.Version.Build + "." + app.Version.Revision;
                    break;
                }
            }

            panel.hololensStatusPanel.ShowAppStatus(version);
            if (version != null)
            {
                StartCoroutine(GetAppRunningStatus());
            }
        }

        private void GetAppError(string error)
        {
            Debug.LogError(error);
            string err = "Can not get App info! Please check your HoloLens has turned on.";
            //panel.hololensStatusPanel.NetworkErrorText.text = "Request Error! \n" + err;
        }


        /// <summary>
        /// 检查SpectatorView运行状况
        /// </summary>
        private IEnumerator GetAppRunningStatus()
        {
            yield return new WaitForSeconds(1);

            string url = "http://localhost:10080/api/resourcemanager/processes";

            Dictionary<string, string> headers = new Dictionary<string, string>();

            string author = LiveParam.AuthorId + ":" + LiveParam.AuthorPass;
            byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(author);
            string authorBase = System.Convert.ToBase64String(bytes);
            Debug.Log("author base64=" + authorBase);

            headers.Add("authorization", "Basic " + authorBase);
            Debug.Log("Try to get [" + url + "]");
            WWW www = GET(url, headers, GetAppRunningComplete, GetAppRunningError);

        }

        private void GetAppRunningComplete(string result)
        {
            Debug.Log(result);

            ProcessesData processes = JsonUtil.Deserialize<ProcessesData>(result);

            bool running = false;
            for (int i = 0;i < processes.Processes.Count;i ++)
            {
                ProcessData data = processes.Processes[i];
                if (data.AppName != null && (data.AppName == SpectatorViewAppName))
                {
                    running = true;
                    break;
                }
            }

            panel.hololensStatusPanel.ShowAppRunning(running);

        }

        private void GetAppRunningError(string error)
        {
            Debug.LogError(error);
            string err = "Can not get App info! Please check your HoloLens has turned on.";
            //panel.hololensStatusPanel.NetworkErrorText.text = "Request Error! \n" + err;
        }


        /// <summary>
        /// 检查SpectatorView配置文件
        /// </summary>
        private IEnumerator GetAppConfigFile()
        {
            yield return new WaitForSeconds(1);

            string url = "http://localhost:10080/api/resourcemanager/processes";

            Dictionary<string, string> headers = new Dictionary<string, string>();

            string author = LiveParam.AuthorId + ":" + LiveParam.AuthorPass;
            byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(author);
            string authorBase = System.Convert.ToBase64String(bytes);
            Debug.Log("author base64=" + authorBase);

            headers.Add("authorization", "Basic " + authorBase);
            Debug.Log("Try to get [" + url + "]");
            WWW www = GET(url, headers, GetAppConfigFileComplete, GetAppConfigFileError);

        }

        private void GetAppConfigFileComplete(string result)
        {
            Debug.Log(result);

            ProcessesData processes = JsonUtil.Deserialize<ProcessesData>(result);

            bool running = false;
            for (int i = 0; i < processes.Processes.Count; i++)
            {
                ProcessData data = processes.Processes[i];
                if (data.AppName != null && (data.AppName == SpectatorViewAppName))
                {
                    running = true;
                    break;
                }
            }

            panel.hololensStatusPanel.ShowAppRunning(running);

        }

        private void GetAppConfigFileError(string error)
        {
            Debug.LogError(error);
            string err = "Can not get App info! Please check your HoloLens has turned on.";
            //panel.hololensStatusPanel.NetworkErrorText.text = "Request Error! \n" + err;
        }



        ////////////////////////////////////////////




        /// <summary>
        /// 连接一个wifi的profile
        /// </summary>
        /// <param name="profileName"></param>
        public void ConnectWifiProfile(string profileName)
        {
            //string url = "http://localhost:10080/api/wifi/network?interface=" + networkID + "&profile=" + profileName + "&op=connect";
            string url = "http://localhost:10080/api/wifi/network";

            Dictionary<string, string> headers = new Dictionary<string, string>();

            string author = LiveParam.AuthorId + ":" + LiveParam.AuthorPass;
            byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(author);
            string authorBase = System.Convert.ToBase64String(bytes);
            Debug.Log("author base64=" + authorBase);

            headers.Add("authorization", "Basic " + authorBase);
            Debug.Log("Try to get [" + url + "]");

            Dictionary<string, string> postParam = new Dictionary<string, string>();
            postParam.Add("interface", networkID);
            postParam.Add("profile", profileName);
            postParam.Add("op", "connect");

            //WWW www = GET(url, headers, ConnectWifiProfileComplete, ConnectWifiProfileError);
            WWW www = POST(url, postParam, headers, ConnectWifiProfileComplete, ConnectWifiProfileError);
        }

        private void ConnectWifiProfileComplete(string result)
        {
            Debug.Log(result);
        }

        private void ConnectWifiProfileError(string error)
        {
            Debug.Log(error);
        }


        /// <summary>
        /// 启动指定应用 
        /// </summary>
        /// <param name="profileName"></param>
        public void StartApp()
        {
            if (appInfo == null)
                return;

            string url = "http://localhost:10080/api/taskmanager/app";

            Dictionary<string, string> headers = new Dictionary<string, string>();

            string author = LiveParam.AuthorId + ":" + LiveParam.AuthorPass;
            byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(author);
            string authorBase = System.Convert.ToBase64String(bytes);
            Debug.Log("author base64=" + authorBase);

            headers.Add("authorization", "Basic " + authorBase);
            Debug.Log("Try to get [" + url + "]");

            Dictionary<string, string> postParam = new Dictionary<string, string>();
            postParam.Add("appid", appInfo.appid);
            postParam.Add("package", appInfo.package);

            //WWW www = GET(url, headers, ConnectWifiProfileComplete, ConnectWifiProfileError);
            WWW www = POST(url, postParam, headers, StartAppComplete, StartAppError);
        }

        private void StartAppComplete(string result)
        {
            Debug.Log(result);
        }

        private void StartAppError(string error)
        {
            Debug.Log(error);
        }



        private bool CheckAuthorized(string error)
        {
            if (error.IndexOf("Unauthorized") >= 0)
            {
                panel.hololensStatusPanel.ShowLogin();
                return false;
            }
            return true;
        }

        public WWW GET(string url, Dictionary<string, string> headers, System.Action<string> onComplete, System.Action<string> onError)
        {

            WWW www = new WWW(url, null, headers);
            StartCoroutine(WaitForRequest(www, onComplete, onError));
            return www;
        }

        public WWW POST(string url, Dictionary<string, string> post, Dictionary<string, string> headers, System.Action<string> onComplete, System.Action<string> onError)
        {
            WWWForm form = new WWWForm();

            foreach (KeyValuePair<string, string> post_arg in post)
            {
                form.AddField(post_arg.Key, post_arg.Value);
            }

            WWW www = new WWW(url, form.data, headers);

            StartCoroutine(WaitForRequest(www, onComplete, onError));
            return www;
        }

        private IEnumerator WaitForRequest(WWW www, System.Action<string> onComplete, System.Action<string> onError)
        {
            yield return www;
            // check for errors
            if (www.error == null)
            {
                onComplete(www.text);
            }
            else
            {
                onError(www.error);
            }
        }


        public class NetworkStatus
        {
            public List<Adapter> Adapters;
        }

        public class Adapter
        {
            public string Description;
            public string HardwareAddress;
            public int Index;
            public string Name;
            public string Type;
            public DHCPData DHCP;
            public List<AddressData> Gateways;
            public List<AddressData> IpAddresses;
        }

        public class DHCPData
        {
            public int LeaseExpires;
            public int LeaseObtained;
            public AddressData Address;
        }

        public class AddressData
        {
            public string IpAddress;
            public string Mask;
        }

        public class AllAvailableNetwork
        {
            public List<AvailableNetwork> AvailableNetworks;
        }

        public class AvailableNetwork
        {
            public bool AlreadyConnected;
            public string ProfileName;
            public string SSID;
        }

        public class InstalledApps
        {
            public List<InstalledApp> InstalledPackages;
        }

        public class InstalledApp
        {
            public string Name;
            public string PackageFullName;
            public string PackageRelativeId;
            public AppVersion Version;
        }

        public class AppVersion
        {
            public int Build;
            public int Major;
            public int Minor;
            public int Revision;
        }

        public class BatteryState
        {
            public int Charging;
            public int MaximumCapacity;
            public int RemainingCapacity;
        }

        public class ProcessesData
        {
            public List<ProcessData> Processes;
        }

        public class ProcessData
        {
            public string AppName;
            //public string PackageFullName;
        }

        public class WifiInterfaces
        {
            public List<WifiInterface> Interfaces;
        }

        public class WifiInterface
        {
            public string GUID;
            public List<WifiProfile> ProfilesList;
        }

        public class WifiProfile
        {
            public string Name;
        }



        /// <summary>
        /// 定时更新
        /// </summary>
        void Update()
        {
            if (!hasTurnOn)
                return;

            float curTime = Time.realtimeSinceStartup;


            if (curTime - checkBatteryTime > checkBatteryInterval)
            {
                GetBatteryStatus();
                checkBatteryTime = curTime;
            }
        }

#endif
    }


}
