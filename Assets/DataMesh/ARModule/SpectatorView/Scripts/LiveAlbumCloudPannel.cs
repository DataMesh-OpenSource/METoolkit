using DataMesh.AR;
using DataMesh.AR.Event;
using DataMesh.AR.SpectatorView;
using DataMesh.AR.Utility;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
using MEHoloClient.Interface.Social;
#endif

using MEHoloClient.Interface.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DataMesh.AR.Interactive;
using System.Reflection;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

public class LiveAlbumCloudPannel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    [Serializable]
    public class JsonData
    {
        public int code;
        public string msg;
        public string[] data;
    }

    public class JsonTimeData
    {
        public int[] data;
    }

    public enum UploadFileType
    {
        Image,
        Video,
    }

    [HideInInspector]
    public float recordTime = 10f;
    public Dropdown albumProfileDropdown; //下拉菜单
    public Text IpInfo;
    public Button buttonStartRecord;
    public Button buttonTakePicture;
    public Button buttonStopRecord;
    public LiveMainPanel liveMainPannel;
    public Text statusError;
    public Image loadingImage;
    public GameObject ProfileDropdown;

    private string AlbumURL;
    private int serverPort;
    private List<string> listAlbumProfileName; //相册名称列表
    private string imageOutputPath;//拍摄的图片地址
    private string imageName;//拍摄的图片的名字
    private string videoOutputPath;
    private string videoName; //如果录像时禁止拍照， outputPath和name可以使用同一个
    private float limitTime = 3f; //上传前检查文件是否存在的限制时间
    private BackgroundWorker uploadWorker;
    private Delegate[] delegates;
    public void Awake()
    {
        AppConfig.Instance.LoadConfig(MEHoloConstant.NetworkConfigFile);
        AlbumURL = AppConfig.Instance.GetConfigByFileName(MEHoloConstant.NetworkConfigFile, "Server_Host", "127.0.0.1");
        serverPort = int.Parse(AppConfig.Instance.GetConfigByFileName(MEHoloConstant.NetworkConfigFile, "Server_Port", "8848"));
        listAlbumProfileName = new List<string>();
        IpInfo.text = AlbumURL;
        //注册按钮
        ETListener.Get(buttonStartRecord.gameObject).onClick = RecordVideoStart;
        ETListener.Get(buttonStopRecord.gameObject).onClick = RecordVideoStop;
        ETListener.Get(buttonTakePicture.gameObject).onClick = TakePicture;

        uploadWorker = new BackgroundWorker();
        loadingImage.gameObject.SetActive(false);
    }

    // Use this for initialization
    void Start()
    {

    }

    /// <summary>
    /// 打开并刷新Live端Album窗口
    /// </summary>
    public void OpenAndRefreshAlbumProfileName()
    {
        StartCoroutine(LoadAlbumProfileNameData());
        uploadWorker.StartWorker();
    }

    /// <summary>
    /// 关闭Live端Album窗口
    /// </summary>
    public void CloseAlbumCloudPannelWindow()
    {
        uploadWorker.StopWorker();
        loadingImage.gameObject.SetActive(false);
        albumProfileDropdown.Hide();
        if (ProfileDropdown.transform.Find("Dropdown List") != null)
        {
            Destroy(ProfileDropdown.transform.Find("Dropdown List").gameObject);
        }
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// 从服务器下载Album列表
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadAlbumProfileNameData()
    {
        WWW downloadAlbumProfileData = new WWW("http://" + AlbumURL + ":" + serverPort.ToString() + "/me/social/album/list");
        yield return downloadAlbumProfileData;
        if (downloadAlbumProfileData.error != null)
        {
            Debug.LogWarning("Download AlbumProfileNameData false ，message : " + downloadAlbumProfileData.error);
        }
        else
        {
            string albumListJson = downloadAlbumProfileData.text;
            Debug.Log(albumListJson);
            JsonData jsonData = JsonUtility.FromJson<JsonData>(albumListJson);

            listAlbumProfileName.Clear();
            for (int i = 0; i < jsonData.data.Length; i++)
            {
                listAlbumProfileName.Add(jsonData.data[i]);
            }
            ShowAlbumProfile(listAlbumProfileName);
        }
    }

    /// <summary>
    /// 刷新显示相册列表
    /// </summary>
    /// <param name="profileName"></param>
    private void ShowAlbumProfile(List<string> profileName)
    {
        albumProfileDropdown.options.Clear();
        for (int i = 0; i < profileName.Count; i++)
        {
            albumProfileDropdown.options.Add(new Dropdown.OptionData(profileName[i]));
        }
        albumProfileDropdown.value = 0;
        albumProfileDropdown.RefreshShownValue();
        albumProfileDropdown.gameObject.SetActive(true);
    }

    private void RecordVideoStart(GameObject obj)
    {

        buttonStartRecord.gameObject.SetActive(false);
        buttonStopRecord.gameObject.SetActive(true);
        LiveController.Instance.StartCapture();

        StartCoroutine(WaitAndExcet(RecordVideoStop, recordTime));

    }
    private void RecordVideoStop(GameObject obj)
    {

        //  liveMainPannel.Recording = false;
        buttonStartRecord.gameObject.SetActive(true);
        buttonStopRecord.gameObject.SetActive(false);
        LiveController.Instance.StopCapture(ref videoOutputPath, ref videoName);
        Debug.Log("videoOutputPath : " + videoOutputPath + "  VideoName : " + videoName);
        UploadFileType filetype = UploadFileType.Video;
        StartCoroutine(CheckAndUpload(videoOutputPath, videoName + ".mp4", filetype));

    }
    private void TakePicture(GameObject obj)
    {
        LiveController.Instance.TakeSnap(ref imageOutputPath, ref imageName);
        if (imageOutputPath == null || imageName == null)
        {
            SetStatusErrorText("拍摄照片的保存路径获取失败");
            return;
        }
        UploadFileType fileType = UploadFileType.Image;
        StartCoroutine(CheckAndUpload(imageOutputPath, imageName, fileType));
    }

    /// <summary>
    /// 检查文件是否已保存成功，在3s内检查到保存的文件才会进行上传操作
    /// </summary>
    private IEnumerator CheckAndUpload(string outputPath, string name, UploadFileType fileType)
    {
        Debug.Log("CheckUpload!!!!");
        if (GetCurrentAlbumProfileName() == null)
        {
            Debug.LogWarning("CurrentAlbumProfileName is null");
            StopCoroutine("CheckAndUpload");
        }

        float checkTime = 0;
        while (!File.Exists(outputPath + name) && checkTime < limitTime)
        {
            checkTime += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        if (File.Exists(outputPath + name))
        {
            Debug.Log("开始上传到Server");
            if (GetCurrentAlbumProfileName() != null)
            {
                StartCoroutine(UploadToServer(outputPath, name, GetCurrentAlbumProfileName(), fileType));
            }
            else
            {
                Debug.Log("currentAlbumProfileName is null");
            }
        }
        else
        {
            SetStatusErrorText("检查该文件是否存在,文件地址 : " + outputPath + name);
        }
    }

    /// <summary>
    /// 上传照片或视频到服务器
    /// </summary>
    private IEnumerator UploadToServer(string outputPath, string name, string currentAlbumName, UploadFileType fileType)
    {
        uploadWorker.StartWorker();
        var fileFolder = outputPath;
        var uploadFileName = name;
        string serverUrl = "http://" + AlbumURL + ":" + serverPort.ToString();
        string appId = MEHoloEntrance.Instance.AppID;
        string albumName = currentAlbumName;
        float uploadTime = 0;
        UploadProgressApi uploadProgressApi = null;
        try
        {
            if (fileType == UploadFileType.Image)
            {
                uploadProgressApi = new UploadProgressSocialImageApi(serverUrl,
                 appId, fileFolder, uploadFileName, albumName, 50);
            }
            else
            {
                uploadProgressApi = new UploadProgressSocialVideoApi(serverUrl,
                  appId, fileFolder, uploadFileName, albumName, 50);
            }
            uploadWorker.SubmitWork(uploadProgressApi);
            loadingImage.gameObject.SetActive(true);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        while (uploadWorker.GetProgress(uploadProgressApi) < 1)
        {
            yield return new WaitForSeconds(0.5f);
            uploadTime += 0.5f;
        }

        if (uploadWorker.GetProgress(uploadProgressApi) >= 1)
        {
            SetStatusErrorText("Upload Success");
        }
        else
        {
            SetStatusErrorText("Upload Failed");
        }
        loadingImage.gameObject.SetActive(false);

    }

    /// <summary>
    /// 取得当前下拉列表的名字
    /// </summary>
    public string GetCurrentAlbumProfileName()
    {
        string currentProfileName = albumProfileDropdown.captionText.text;
        if (currentProfileName != null && currentProfileName != "")
            return currentProfileName;
        else
        {
            SetStatusErrorText("Current AlbumProfileName is null");
            return null;
        }
    }

    public void OnPointerEnter(PointerEventData data)
    {
        ButtonEnter(this.gameObject);
    }

    public void OnPointerExit(PointerEventData data)
    {
        ButtonExit(this.gameObject);
    }

    public void OnPointerClick(PointerEventData data)
    {
        albumProfileDropdown.Hide();
    }


    object cbtapAction;
    private void ButtonEnter(GameObject obj)
    {
        if (MultiInputManager.Instance.cbTap != null)
        {
            cbtapAction = MultiInputManager.Instance.cbTap;
        }
        MultiInputManager.Instance.cbTap = null;
    }


    private void ButtonExit(GameObject obj)
    {
        MultiInputManager.Instance.cbTap = (Action<int>)cbtapAction;
    }

    public void SetStatusErrorText(string errorString)
    {
        statusError.text = errorString;
        StartCoroutine(WaitAndClearStatusErrorText());
    }

    private IEnumerator WaitAndClearStatusErrorText()
    {
        yield return new WaitForSeconds(3);
        statusError.text = "";
    }

    public void NoSelectAndCloseDropdown()
    {
        albumProfileDropdown.Hide();
    }

    private IEnumerator WaitAndExcet(Action<GameObject> action, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        if (action != null)
        {
            action(this.gameObject);
        }
    }


}
#endif