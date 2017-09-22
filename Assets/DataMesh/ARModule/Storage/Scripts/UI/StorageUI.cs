using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataMesh.AR.UI;
using DataMesh.AR.Interactive;
using MEHoloClient.Core.Entities;
using MEHoloClient.Core.Utils;

namespace DataMesh.AR.Storage
{
    public class StorageUI : MonoBehaviour
    {
        private int uiSizeX = 3;
        private int uiSizeY = 4;


        private bool isShowUI = false;

        private BlockList ui;

        private MultiInputManager inputManager;
        private LayerMask oldInputLayer;

        private System.Action<int> cbClickChangePage;
        private System.Action<BaseResource> cbClickResource;
        private System.Action cbClickClose;

        private StorageManager manager;

        public int CountPerPage
        {
            get
            {
                if (ui == null)
                    return 12;

                return ui.CountPerPage;
            }
        }

        public void Init(StorageManager sm)
        {
            manager = sm;

            inputManager = MultiInputManager.Instance;
        }

        public void TurnOn()
        {
            ui = UIManager.Instance.listManager.CreateBlockList();

            ui.AddCallbackChangePage(OnChangePage);
            ui.AddCallbackClick(OnClickResource);
            ui.CallbackClose = OnUIClose;

        }

        public void ShowLoading()
        {
            ui.ShowLoading();
        }

        public void HideLoading()
        {
            ui.HideLoading();
        }

        public void OnChangePage(int page)
        {
            if (manager.IsBusy())
                return;

            if (cbClickChangePage != null)
            {
                cbClickChangePage(page);
            }
        }

        public void OnClickResource(BlockListData data)
        {
            BaseResource res = data.data as BaseResource;
            if (cbClickResource != null)
                cbClickResource(res);
        }

        public void ShowUI(System.Action<int> cbChangePage, System.Action<BaseResource> cbResource, System.Action cbClose = null)
        {
            if (isShowUI)
                return;

            isShowUI = true;


            cbClickChangePage = cbChangePage;
            cbClickResource = cbResource;
            cbClickClose = cbClose;

            // 放在视线位置 
            Vector3 headPosition = Camera.main.transform.position;
            Vector3 gazeDirection = Camera.main.transform.forward;
            float scale = 1;

            Vector3 pos = Vector3.zero;
            if (inputManager.FocusedObject != null)
            {
                float dis = Vector3.Distance(headPosition, inputManager.hitPoint);
                if (dis > 4)
                    dis = 4;

                pos = headPosition + gazeDirection * (dis - 0.3f);

                if (dis < 4)
                {
                    scale = dis / 4;
                }
            }
            else
            {
                pos = headPosition + gazeDirection * 4f;
            }
            gazeDirection.y = 0;

            ui.SetUISize(pos, gazeDirection, scale);


            ChangeUIData();

            ui.Show();

            ui.HideLoading();

            oldInputLayer = inputManager.layerMask;
            inputManager.layerMask = LayerMask.GetMask("UI");
        }

        public void HideUI()
        {
            ui.Hide();

            inputManager.layerMask = oldInputLayer;

            cbClickResource = null;

            isShowUI = false;

        }


        public bool IsShowUI()
        {
            return isShowUI;
        }



        public void ChangeUIData()
        {
            if (manager.listResult != null)
            {
                List<BlockListData> uiDataList = new List<BlockListData>();

                if (manager.totalCount > 0)
                {
                    Debug.Log("data len=" + manager.listResult.Count);

                    for (int i = 0; i < manager.listResult.Count; i++)
                    {
                        BaseResource res = manager.listResult[i];

                        BlockListData data = new BlockListData();
                        data.name = res.name;
                        data.data = res;
                        for (int j = 0; j < res.thumbnails.Length; j++)
                        {
                            if (res.thumbnails[j] != null)
                            {
                                data.icon = ImageHelper.GetImageUrl("http://" + manager.serverHost, res.thumbnails[j]);
                                break;
                            }
                        }

                        uiDataList.Add(data);
                    }

                }

                ui.SetData(uiDataList, manager.curPage, manager.totalCount);

            }

        }


        private void OnUIClose()
        {
            HideUI();

            if (cbClickClose != null)
                cbClickClose();
        }



        // Update is called once per frame
        void Update()
        {

        }
    }

}