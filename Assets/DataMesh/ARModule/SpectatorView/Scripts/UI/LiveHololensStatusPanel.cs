using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DataMesh.AR.Utility;
using DataMesh.AR.Interactive;
using DataMesh.AR.Event;

namespace DataMesh.AR.SpectatorView
{

    public class LiveHololensStatusPanel : MonoBehaviour
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        public GameObject loginPanel;
        public GameObject statusPanel;

        public InputField accountInput;
        public InputField passwordInput;

        public Text NetworkStatusText;

        public Button buttonSubmit;

        public GameObject wifiInfo;
        public GameObject wifiInfoContent;
        public GameObject wifiInfoItemPrefab;

        public Text waitingWifiText;
        public Text wifiErrorText;
        public Button buttonWifiRefresh;

        public Text AppStatusText;
        public Text AppRunningText;
        public Button buttonStartApp;
        public Button buttonStopApp;

        public HoloLensBatteryStatus batteryStatus;

        public GameObject wifiProfileArea;
        public Dropdown wifiProfileDropdown;
        public Button buttonWifiProfileConnect;

        private LiveController liveController;

        private List<LiveHoloLensWifiItem> wifiItemList = new List<LiveHoloLensWifiItem>();

        public void Init(LiveController b)
        {
            liveController = b;

            SelectedEventDispatcher select = accountInput.gameObject.AddComponent<SelectedEventDispatcher>();
            select.cbSelect = OnSelect;
            select.cbDeselect = OnDeselect;

            select = passwordInput.gameObject.AddComponent<SelectedEventDispatcher>();
            select.cbSelect = OnSelect;
            select.cbDeselect = OnDeselect;

            ETListener.Get(buttonSubmit.gameObject).onClick = OnSubmit;

            InitShow();

            ETListener.Get(buttonWifiRefresh.gameObject).onClick = OnWifiRefresh;

            ETListener.Get(buttonStartApp.gameObject).onClick = OnStartApp;
            ETListener.Get(buttonStopApp.gameObject).onClick = OnStopApp;

            ETListener.Get(buttonWifiProfileConnect.gameObject).onClick = OnConnectWifiProfile;

            wifiProfileArea.SetActive(false);



        }

        private void OnSelect(GameObject obj)
        {
            MultiInputManager.Instance.StopCapture();
        }

        private void OnDeselect(GameObject obj)
        {
            MultiInputManager.Instance.StartCapture();
        }

        private void OnSubmit(GameObject obj)
        {
            liveController.wdpController.Login(accountInput.text, passwordInput.text);
        }

        private void OnWifiRefresh(GameObject obj)
        {
            liveController.wdpController.GetWIFIStatus();
        }

        private void OnStartApp(GameObject obj)
        {
            liveController.wdpController.StartApp();
        }

        private void OnStopApp(GameObject obj)
        {

        }


        private void OnConnectWifiProfile(GameObject obj)
        {
            if (wifiProfileDropdown.options.Count == 0)
                return;

            string profileName = wifiProfileDropdown.captionText.text;

            liveController.wdpController.ConnectWifiProfile(profileName);
        }

        public void ShowLogin()
        {
            accountInput.text = LiveParam.AuthorId;
            passwordInput.text = LiveParam.AuthorPass;

            loginPanel.gameObject.SetActive(true);
            statusPanel.gameObject.SetActive(false);
        }

        public void ShowStatus()
        {
            loginPanel.gameObject.SetActive(false);
            statusPanel.gameObject.SetActive(true);
        }

        public void SetWifiItemList(List<string> nameList, int connectedId)
        {
            for (int i = 0; i < nameList.Count; i++)
            {
                GameObject obj = PrefabUtils.CreateGameObjectToParent(wifiInfoContent, wifiInfoItemPrefab);
                LiveHoloLensWifiItem item = obj.GetComponent<LiveHoloLensWifiItem>();

                item.wifiName.text = nameList[i];
                if (i == connectedId)
                {
                    item.buttonConnect.gameObject.SetActive(false);
                    item.connected.gameObject.SetActive(true);
                }
                else
                {
                    item.buttonConnect.gameObject.SetActive(false);
                    item.connected.gameObject.SetActive(false);
                }

                int deltaY = -wifiItemList.Count * 28;
                RectTransform trans = item.transform as RectTransform;
                trans.anchoredPosition = new Vector2(0, deltaY);
                trans.sizeDelta = new Vector2(0, 30);

                wifiItemList.Add(item);
            }

            RectTransform transContent = wifiInfoContent.transform as RectTransform;
            transContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, wifiItemList.Count * 28);
        }

        public void ClearWifiItem()
        {
            for (int i = 0; i < wifiItemList.Count; i++)
            {
                Destroy(wifiItemList[i].gameObject);
            }
            wifiItemList.Clear();
        }

        public void InitShow()
        {
            wifiInfo.SetActive(false);
            waitingWifiText.gameObject.SetActive(false);
            NetworkStatusText.text = "";
            wifiErrorText.gameObject.SetActive(false);

            AppStatusText.gameObject.SetActive(false);

            batteryStatus.gameObject.SetActive(false);
        }

        public void ShowNetworkWaiting()
        {
            NetworkStatusText.text = "Loading network information, please wait...";
            NetworkStatusText.color = Color.white;
        }

        public void ShowNetworkResult(string rs)
        {
            NetworkStatusText.text = rs;
            NetworkStatusText.color = Color.white;
        }

        public void ShowNetworkError()
        {
            NetworkStatusText.text = "Can not get network information, \n Please make sure your HoloLens has turned on.";
            NetworkStatusText.color = Color.red;
        }

        public void ShowWifiWaiting()
        {
            wifiInfo.SetActive(false);
            waitingWifiText.gameObject.SetActive(true);
            buttonWifiRefresh.gameObject.SetActive(false);
            wifiErrorText.gameObject.SetActive(false);
        }

        public void ShowWifiResult(bool success)
        {
            wifiInfo.SetActive(true);
            waitingWifiText.gameObject.SetActive(false);
            buttonWifiRefresh.gameObject.SetActive(true);
            if (success)
            {
                wifiErrorText.gameObject.SetActive(false);
            }
            else
            {
                wifiErrorText.gameObject.SetActive(true);
            }
        }

        void Update()
        {
        }

        public void ShowAppStatusWaiting()
        {
            AppStatusText.gameObject.SetActive(true);
            AppStatusText.text = "Loading SpectatorView status, please waiting...";
            AppRunningText.gameObject.SetActive(false);
        }

        public void ShowAppStatus(string version)
        {
            AppStatusText.gameObject.SetActive(true);
            if (version != null)
            {
                AppStatusText.text = "[" + LiveWDPController.SpectatorViewAppName + "] has installed, version=" + version;

                AppRunningText.gameObject.SetActive(true);
                AppRunningText.text = " --> Check app running status, please wait...";
                AppRunningText.color = Color.white;
                buttonStartApp.gameObject.SetActive(false);
                buttonStopApp.gameObject.SetActive(false);
            }
            else
            {
                AppStatusText.text = "Can not find App [SpectatorView], please install app.";
                AppRunningText.gameObject.SetActive(false);
            }
        }

        public void ShowAppRunning(bool running)
        {
            if (running)
            {
                AppRunningText.text = "Is Running";
                AppRunningText.color = Color.green;
                buttonStartApp.gameObject.SetActive(false);
                buttonStopApp.gameObject.SetActive(false);
            }
            else
            {
                AppRunningText.text = "Not Running";
                AppRunningText.color = Color.red;
                buttonStartApp.gameObject.SetActive(false);
                buttonStopApp.gameObject.SetActive(false);
            }
        }


        public void ShowWifiProfileWaiting()
        {
            wifiProfileArea.SetActive(true);
            wifiProfileDropdown.gameObject.SetActive(false);
            buttonWifiProfileConnect.gameObject.SetActive(false);
        }

        public void ShowWifiProfileResult(List<string> profileName)
        {
            wifiProfileDropdown.options.Clear();

            for (int i = 0;i < profileName.Count;i ++)
            {
                wifiProfileDropdown.options.Add(new Dropdown.OptionData(profileName[i]));
            }

            wifiProfileDropdown.value = 0;
            wifiProfileDropdown.RefreshShownValue();

            wifiProfileDropdown.gameObject.SetActive(true);
            //buttonWifiProfileConnect.gameObject.SetActive(true);
        }

#endif
    }
}