using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using DataMesh.AR;
using DataMesh.AR.Storage;
using DataMesh.AR.UI;
using DataMesh.AR.Interactive;

using MEHoloClient.Core.Entities;

using MEHoloClient.Interface.Storage;

public class StorageSample : MonoBehaviour
{
    public Transform booth1;
    public Transform booth2;
    public Transform booth3;

    private StorageManager storageManager;
    private MultiInputManager inputManager;
    private CursorController cursor;

    private bool hasShow = false;
    private int pageAdd = 1;

    private Transform currentSelectBooth;

    private StorageUI storageUI;

    private Asset currentAsset;

    void Start()
    {
        StartCoroutine(WaitForInit());
    }

    private IEnumerator WaitForInit()
    {
        MEHoloEntrance entrance = MEHoloEntrance.Instance;

        while (!entrance.HasInit)
        {
            yield return null;
        }

        storageManager = StorageManager.Instance;
        inputManager = MultiInputManager.Instance;
        cursor = UIManager.Instance.cursorController;

        storageManager.TurnOn();

        inputManager.cbTap = OnTap;

        storageUI = storageManager.GetUI();
    }

    private void OnTap(int count)
    {
        if (cursor.isBusy)
            return;

        if (storageUI.IsShowUI())
            return;

        if (inputManager.FocusedObject != null)
        {
            if (inputManager.FocusedObject == booth1.gameObject)
            {
                currentSelectBooth = booth1;
            }
            else if (inputManager.FocusedObject == booth2.gameObject)
            {
                currentSelectBooth = booth2;
            }
            else if (inputManager.FocusedObject == booth3.gameObject)
            {
                currentSelectBooth = booth3;
            }
            else
            {
                currentSelectBooth = null;
            }

            if (currentSelectBooth != null)
            {
                // 显示UI，并传入两个回调 
                storageUI.ShowUI(OnChangePage, OnSelect);

                // 显示Loading状态
                storageUI.ShowLoading();

                // 开始读取第一页 
                LoadAssetPages(1);
            }
        }
    }

    /// <summary>
    /// 读取一页数据
    /// </summary>
    /// <param name="page"></param>
    private void LoadAssetPages(int page)
    {
        if (!storageManager.IsBusy())
        {
            storageManager.GetStorageResourceList(
                StorageManager.StorageResourceType.Asset,
                page,
                storageUI.CountPerPage,
                OnLoadPageFinish
                );
        }

    }

    /// <summary>
    /// 读取一页数据结束回调
    /// </summary>
    /// <param name="succ"></param>
    /// <param name="error"></param>
    private void OnLoadPageFinish(bool succ, string error)
    {
        storageUI.HideLoading();

        if (!succ)
        {
            cursor.ShowInfo("load storage error!\n" + error);
            return;
        }

        storageUI.ChangeUIData();
    }

    /// <summary>
    /// 点击翻页按钮的回调
    /// </summary>
    /// <param name="page"></param>
    private void OnChangePage(int page)
    {
        Debug.Log("Click page=" + page);
        storageUI.ShowLoading();
        LoadAssetPages(page);
    }

    /// <summary>
    /// 点击一个资源的回调
    /// </summary>
    /// <param name="res"></param>
    private void OnSelect(BaseResource res)
    {
        currentAsset = res as Asset;

        // 显示Loading状态 
        storageUI.ShowLoading();

        // 下载资源，传入回调
        storageManager.DownloadAsset(currentAsset, OnDownloadAssetFinish);
    }

    /// <summary>
    /// 下载资源完成的回调 
    /// </summary>
    /// <param name="filePath"></param>
    private void OnDownloadAssetFinish(bool rs)
    { 
        if (!rs)
        {
            Debug.LogError("Download error");
        }

        string fileName = storageManager.GetFileNameFromAsset(currentAsset);
        string filePath = StorageManager.GetFilePath() + fileName;

        Debug.Log("Asset at " + filePath);

        if (filePath == null)
        {
            Debug.LogWarning("Download file Failed!");
            return;
        }

        storageUI.HideLoading();
        storageUI.HideUI();

        cursor.isBusy = true;

        // 下载完毕，开始加载资源并显示 
        LoadAssetFromFile(fileName);
        //StartCoroutine(LoadAsset(filePath));
    }

    /// <summary>
    /// 从磁盘上加载已经下载过的资源，并显示  
    /// </summary>
    /// <param name="fileName"></param>
    private void LoadAssetFromFile(string fileName)
    {
        StorageAssetData data = StorageAssetManager.Instance.LoadAsset(fileName);

        if (data.bundle == null)
        {
            Debug.LogError("Load Bundle " + fileName + " failed!");
            cursor.isBusy = false;
            return;
        }

        try
        {
            GameObject prefab = data.bundle.LoadAsset<GameObject>(data.assetName);

            PrefabUtils.DestroyAllChild(currentSelectBooth.gameObject);

            GameObject obj = PrefabUtils.CreateGameObjectToParent(currentSelectBooth.gameObject, prefab);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Exception! " + e);
        }

        cursor.isBusy = false;
    }

}
