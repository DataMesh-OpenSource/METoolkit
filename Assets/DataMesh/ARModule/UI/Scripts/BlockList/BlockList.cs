using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DataMesh.AR.UI;


namespace DataMesh.AR.UI
{

    public class BlockList : MonoBehaviour
    {
        public GameObject itemPrefab;
        public GameObject grid;
        public Text pageText;
        public GameObject loadingObj;
        public GazeEventReceiver prevReceiver;
        public GazeEventReceiver nextReceiver;
        public GazeEventReceiver closeReceiver;

        public int CountX = 3;
        public int CountY = 4;

        public TransitObject showItemTransit;
        public TransitObject showPanelTransit;

        [HideInInspector]
        public System.Action CallbackClose;

        /// <summary>
        /// 所有的格子，以列表长宽来决定
        /// </summary>
        private List<BlockListItem> itemList = new List<BlockListItem>();

        /// <summary>
        /// 当前页的数据。每次只设置一页的数据 
        /// </summary>
        private List<BlockListData> dataList = new List<BlockListData>();

        private int countPerPage = 0;
        private int curPage = 1;
        private int totalCount = 0;
        private int pageCount = 0;

        [HideInInspector]
        public bool IsBusy = true;

        public int CountPerPage
        {
            get { return countPerPage; }
        }

        public int PageCount
        {
            get { return pageCount; }
        }

        private System.Action<BlockListData> cbClick;
        private System.Action<int> cbChangePage;

        [HideInInspector]
        public bool isChangingPage = false;

        public void Init()
        {
            Clear();

            for (int j = 0; j < CountY; j++)
            {
                for (int i = 0;i < CountX;i ++)
                {
                    GameObject obj = PrefabUtils.CreateGameObjectToParent(grid, itemPrefab);
                    BlockListItem item = obj.GetComponent<BlockListItem>();
                    item.Init(this);

                    itemList.Add(item);

                    CalItemPos(item, i, j);
                }
            }

            countPerPage = CountX * CountY;

            prevReceiver.cbEnter = OnButtonEnter;
            prevReceiver.cbExit = OnButtonExit;
            prevReceiver.cbTap = OnTapPrev;

            nextReceiver.cbEnter = OnButtonEnter;
            nextReceiver.cbExit = OnButtonExit;
            nextReceiver.cbTap = OnTapNext;

            closeReceiver.cbEnter = OnButtonEnter;
            closeReceiver.cbExit = OnButtonExit;
            closeReceiver.cbTap = OnTapClose;

            loadingObj.SetActive(false);
            prevReceiver.gameObject.SetActive(false);
            nextReceiver.gameObject.SetActive(false);

            pageText.text = "";

            gameObject.SetActive(false);

        }


        #region CallBack相关操作

        public void AddCallbackClick(System.Action<BlockListData> cb)
        {
            cbClick += cb;
        }
        public void RemoveCallbackClick(System.Action<BlockListData> cb)
        {
            cbClick -= cb;
        }
        public void AddCallbackChangePage(System.Action<int> cb)
        {
            cbChangePage += cb;
        }
        public void RemoveCallbackChangePage(System.Action<int> cb)
        {
            cbChangePage -= cb;
        }

        #endregion

        public void SetUISize(Vector3 pos, Vector3 forward, float scale)
        {

            transform.position = pos;
            transform.forward = forward;

            transform.localScale = new Vector3(scale, scale, scale);

        }

        public void Show()
        {
            IsBusy = true;
            showPanelTransit.transit(true, ()=> { IsBusy = false; });

            for (int i = 0; i < itemList.Count; i++)
            {
                BlockListItem item = itemList[i];
                item.showTransit.resetTransit(true);
                item.gameObject.SetActive(false);
            }
        }

        public void Hide()
        {
            IsBusy = true;
            showPanelTransit.transit(false, () => { IsBusy = false; });

            for (int i = 0;i < itemList.Count;i ++)
            {
                BlockListItem item = itemList[i];
                if (item.gameObject.activeSelf)
                {
                    item.showTransit.transit(false, null);
                }
            }
        }

        public void ShowLoading()
        {
            IsBusy = true;
            loadingObj.SetActive(true);
        }
        public void HideLoading()
        {
            IsBusy = false;
            loadingObj.SetActive(false);
        }

        private void OnButtonEnter(GameObject obj)
        {
            Transform border = obj.transform.Find("border");
            if (border != null)
                border.gameObject.SetActive(true);
            //TransitObject.StartTransit(obj, 0, true);
        }
        private void OnButtonExit(GameObject obj)
        {
            //TransitObject.StartTransit(obj, 0, false);
            Transform border = obj.transform.Find("border");
            if (border != null)
                border.gameObject.SetActive(false);
        }

        private void OnTapPrev(GameObject obj)
        {
            if (IsBusy)
                return;

            TransitObject.StartTransit(obj, 2, true);
            if (curPage > 1 && cbChangePage != null)
                cbChangePage(curPage - 1);
        }
        private void OnTapNext(GameObject obj)
        {
            if (IsBusy)
                return;

            TransitObject.StartTransit(obj, 2, true);
            if (curPage < pageCount && cbChangePage != null)
                cbChangePage(curPage + 1);
        }

        private void OnTapClose(GameObject obj)
        {
            if (IsBusy)
                return;

            if (CallbackClose != null)
                CallbackClose();
        }

        public void OnClick(BlockListItem item)
        {
            if (IsBusy)
                return;

            if (cbClick != null)
            {
                cbClick(item.data);
            }
        }

        private void CalItemPos(BlockListItem item, int x, int y)
        {
            Transform trans = item.transform;

            float widthTotalHalf = CountX * BlockListItem.ITEM_WIDTH / 2;
            float heightTotalHalf = CountY * BlockListItem.ITEM_HEIGHT / 2;

            float xx = ((float)x + 0.5f) * BlockListItem.ITEM_WIDTH - widthTotalHalf;
            float yy = -((float)y + 0.5f) * BlockListItem.ITEM_HEIGHT + heightTotalHalf;

            trans.localPosition = new Vector3(xx, yy, 0);
        }

        public void Clear()
        {
            for (int i = 0;i < itemList.Count;i++)
            {
                Destroy(itemList[i].gameObject);
            }

            itemList.Clear();
        }

        public void SetData(List<BlockListData> list, int page, int total)
        {
            dataList = list;
            curPage = page;
            totalCount = total;
            pageCount = (totalCount - 1) / countPerPage + 1;
            Debug.Log("total=" + totalCount + " page=" + curPage + " max=" + pageCount);
            ChangePage(curPage);
        }

        public void ClearData()
        {
            dataList.Clear();
            curPage = 0;
            totalCount = 0;

            ChangePage(curPage);
        }

        public int GetCurrentPage()
        {
            return curPage;
        }

        public int GetPageCount()
        {
            return pageCount;
        }

        public void ChangePage(int page)
        {
            if (page < 0)
                page = 0;
            if (page > PageCount)
                page = PageCount;

            if (isChangingPage)
                return;

            curPage = page;

            isChangingPage = true;


            for (int i = 0; i < countPerPage; i++)
            {
                BlockListItem item = itemList[i];
                if (item.gameObject.activeSelf)
                {
                    item.showTransit.delayTime = 0;
                }
            }

            showItemTransit.transit(false, HideOK);

            pageText.text = "" + curPage + " / " + pageCount;

            if (pageCount > 1)
            {
                prevReceiver.gameObject.SetActive(true);
                nextReceiver.gameObject.SetActive(true);
            }
            else
            {
                prevReceiver.gameObject.SetActive(false);
                nextReceiver.gameObject.SetActive(false);
            }

        }

        private void HideOK()
        {
            int count = dataList.Count;
            for (int i = 0; i < countPerPage; i++)
            {
                BlockListItem item = itemList[i];
                if (i < count)
                {
                    item.showTransit.delayTime = Random.Range(0f, 0.4f);
                    item.showTransit.transitGroup = 0;
                    item.data = dataList[i];
                }
                else
                {
                    item.showTransit.transitGroup = 999;
                }

            }

            showItemTransit.transit(true, () => { isChangingPage = false; });

        }

        // Update is called once per frame
        void Update()
        {

        }


    }
}