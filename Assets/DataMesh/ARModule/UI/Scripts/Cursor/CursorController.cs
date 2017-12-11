using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DataMesh.AR.Interactive;
using DataMesh.AR.Anchor;

namespace DataMesh.AR.UI
{

    public class CursorController : MonoBehaviour
    {
        public Transform CursorRoot;
        public Transform InfoRoot;

        public GameObject cursorNormalPrefab;
        public GameObject cursorTapPrefab;
        public GameObject cursorLoadingPrefab;
        public GameObject cursorInfoPrefab;
        public GameObject infoPrefab;

        public bool cursorFaceToCamera = false;

        [HideInInspector]
        public GameObject cursorNormal;
        [HideInInspector]
        public GameObject cursorTap;
        [HideInInspector]
        public GameObject cursorLoading;
        [HideInInspector]
        public GameObject cursorInfo;

        /// <summary>
        /// 设置过滤层，检查当前视线碰撞物体，如果不在layerMask之中，则显示Normal，否则显示Hand 
        /// </summary>
        public LayerMask layerMask = int.MaxValue;

        private MultiInputManager gm;
        private bool isTurnOn = false;

        //private bool isOpenAnchorNav = false;


        ////////////////以下是NavCursor相关/////////////////

        /// <summary>
        /// 设置游标为忙状态 
        /// </summary>
        [HideInInspector]
        public bool isBusy = false;
        private bool isShowInfoCursor = false;
        private Dictionary<GameObject, GameObject> dicNavCursors;
        private Camera mainCamera;
        private GameObject[] anchorMarks;
        private int cameraOriginlayer;
        private List<GameObject> displayAnchorListObjects;




        
        ///////////////////以下是鼠标相关////////////////
        public float DistanceFromCollision;


        public void Init()
        {
            gm = MultiInputManager.Instance;

            if (cursorNormal != null)
                cursorNormal.SetActive(false);
            if (cursorTap != null)
                cursorTap.SetActive(false);
            if (cursorLoading != null)
                cursorLoading.SetActive(false);

            //未完成
            //CheckAnchorNavs();
            mainCamera = Camera.main;

            //SceneAnchorController.Instance.AddCallbackTurnOn(OpenAnchorNavigation);
            //SceneAnchorController.Instance.AddCallbackTurnOff(HideAnchorNavigation);

            RefreshAll();
        }


        public void TurnOn()
        {
            CursorRoot.gameObject.SetActive(true);
            InfoRoot.gameObject.SetActive(true);

            isTurnOn = true;
        }

        public void TurnOff()
        {
            CursorRoot.gameObject.SetActive(false);
            InfoRoot.gameObject.SetActive(false);

            isTurnOn = false;
        }

        public void RefreshAll()
        {
            RefreshNormal();
            RefreshBusy();
            RefreshTap();
            RefreshInfo();
            //RefreshAnchorNav();
        }

        public void RefreshNormal()
        {
            if (cursorNormal != null)
            {
                Destroy(cursorNormal);
            }

            if (cursorNormalPrefab != null)
            {
                cursorNormal = GameObject.Instantiate(cursorNormalPrefab);
                cursorNormal.transform.SetParent(CursorRoot);
                cursorNormal.transform.localPosition = Vector3.zero;
            }
            else
            {
                cursorNormal = null;
            }
        }

        public void RefreshBusy()
        {
            if (cursorLoading != null)
            {
                Destroy(cursorLoading);
            }

            if (cursorLoadingPrefab != null)
            {
                cursorLoading = GameObject.Instantiate(cursorLoadingPrefab);
                cursorLoading.transform.SetParent(CursorRoot);
                cursorLoading.transform.localPosition = Vector3.zero;
            }
            else
            {
                cursorLoading = null;
            }
        }

        public void RefreshTap()
        {
            if (cursorTap != null)
            {
                Destroy(cursorTap);
            }

            if (cursorTapPrefab != null)
            {
                cursorTap = GameObject.Instantiate(cursorTapPrefab);
                cursorTap.transform.SetParent(CursorRoot);
                cursorTap.transform.localPosition = Vector3.zero;
            }
            else
            {
                cursorTap = null;
            }
        }

        public void RefreshInfo()
        {
            if (cursorInfo != null)
            {
                Destroy(cursorInfo);
            }

            if (cursorInfoPrefab != null)
            {
                cursorInfo = GameObject.Instantiate(cursorInfoPrefab);
                cursorInfo.transform.SetParent(InfoRoot);
                cursorInfo.transform.localPosition = Vector3.zero;
            }
            else
            {
                cursorInfo = null;
            }
        }

        public void SetCursorNormal(GameObject newCursor)
        {
            if (cursorNormal != null)
            {
                Destroy(cursorNormal);
            }

            cursorNormal = newCursor;
            cursorNormal.transform.SetParent(CursorRoot);
            cursorNormal.transform.localPosition = Vector3.zero;
        }
        public void SetCursorTap(GameObject newCursor)
        {
            if (cursorTap != null)
            {
                Destroy(cursorTap);
            }

            cursorTap = newCursor;
            cursorTap.transform.SetParent(CursorRoot);
            cursorTap.transform.localPosition = Vector3.zero;
        }
        public void SetCursorLoading(GameObject newCursor)
        {
            if (cursorLoading != null)
            {
                Destroy(cursorLoading);
            }

            cursorLoading = newCursor;
            cursorLoading.transform.SetParent(CursorRoot);
            cursorLoading.transform.localPosition = Vector3.zero;

        }
        public void SetCursorInfo(GameObject newCursor)
        {
            if (cursorInfo != null)
            {
                Destroy(cursorInfo);
            }

            cursorInfo = newCursor;
            cursorInfo.transform.SetParent(InfoRoot);
            cursorInfo.transform.localPosition = Vector3.zero;
        }

        private void SetActive(GameObject obj, bool b)
        {
            if (obj != null)
                obj.SetActive(b);
        }

        void Update()
        {
            if (!isTurnOn)
            {
                //Debug.Log("!IsturnOn");
                return;
            }

            if (isShowInfoCursor)
            {
                SetActive(cursorLoading,false);
                SetActive(cursorNormal,false);
                SetActive(cursorTap,false);

                SetActive(cursorInfo,true);
            }
            else
            {
                SetActive(cursorInfo,false);
                // 忙的时候，只有转圈
                if (isBusy)
                {
                    SetActive(cursorLoading,true);
                    SetActive(cursorNormal,false);
                    SetActive(cursorTap,false);
                }
                else
                {

                    SetActive(cursorLoading,false);

                    bool focus = false;
                    if (gm.FocusedObject != null)
                    {
                        if ((layerMask & (1 << gm.FocusedObject.layer)) > 0)
                        {
                            focus = true;
                        }
                    }

                    // 根据主状态显示光标
                    if (focus)
                    {
                        SetActive(cursorNormal,false);
                        SetActive(cursorTap,true);
                    }
                    else
                    {

                        SetActive(cursorNormal,true);
                        SetActive(cursorTap,false);
                    }

                }
            }


            Vector3 cp;
            Vector3 cn;
            Vector3 cf;

            float dis = 5;

            // 因为layerMask不同，需要独立计算碰撞 

            // 计算游标位置和方向，多数需要依赖于碰撞物体的位置 
            if (gm.FocusedObject != null)
            {
                cp = gm.hitPoint;
                cn = gm.hitNormal;
                if (cursorFaceToCamera)
                    cf = gm.gazeDirection;
                else
                    cf = -gm.hitNormal;
            }
            else
            {
                cp = gm.headPosition + gm.gazeDirection * dis;
                cn = -gm.gazeDirection;
                cf = gm.gazeDirection;
            }

            CursorRoot.position = cp + cn * DistanceFromCollision;
            CursorRoot.forward = cf;

            // 计算提示图标的位置和方向，通常与碰撞物体无关，固定距离 
            InfoRoot.position = gm.headPosition + gm.gazeDirection * 2;
            InfoRoot.forward = gm.gazeDirection;

            //if (isOpenAnchorNav)
            //    UpdateShowAnchorNav();
        }


        public void ShowInfoCursor()
        {
            if (cursorInfo != null)
                isShowInfoCursor = true;
        }

        public void HideInfoCursor()
        {
            isShowInfoCursor = false;
        }



        public void ShowInfo(string s)
        {
            GameObject obj = PrefabUtils.CreateGameObjectToParent(InfoRoot.gameObject, infoPrefab);
            obj.SetActive(true);
            InfoText info = obj.GetComponent<InfoText>();
            info.SetText(s);

        }

        public void HideError()
        {
            infoPrefab.SetActive(false);
        }



        /*
        public void CheckAnchorNavs()
        {
            if (anchorNavs.Length != 0)
            {
                for (int i = 0; i < anchorNavs.Length; i++)
                {
                    anchorNavs[i].SetActive(false);
                }
            }
        }


        public void RefreshAnchorNav()
        {
            if (anchorNavs != null)
            {
                foreach (var nav in anchorNavs)
                {
                    Destroy(nav);
                }
            }
            ClearNavData();
            if (anchorNavPrefab != null)
            {
                int anchorNums = SceneAnchorController.Instance.GetAnchorNums();
                if (anchorNums > 0)
                {
                    dicNavCursors = new Dictionary<GameObject, GameObject>();
                    anchorMarks = new GameObject[anchorNums];
                    Debug.Log("current anchors num : " + anchorNums);
                    anchorMarks = SceneAnchorController.Instance.GetAnchorMarks();
                    if (anchorMarks == null)
                        Debug.LogError("anchorMarks is null");
                    anchorNavs = new GameObject[anchorNums];
                    for (int i = 0; i < anchorNums; i++)
                    {
                        anchorNavs[i] = GameObject.Instantiate(anchorNavPrefab);
                        anchorNavs[i].transform.SetParent(CursorRoot);
                        anchorNavs[i].SetActive(false);
                        anchorNavs[i].GetComponent<AnchorNavigation>().targetAnchor = anchorMarks[i];
                        dicNavCursors.Add(anchorMarks[i],anchorNavs[i]);
                    }
                  //  HideAnchorNavigation();
                }
            }
            else
            {
                anchorNavs = null;
            }
        }

        public void OpenAnchorNavigation()
        {
            isOpenAnchorNav = true;
            cameraOriginlayer = mainCamera.cullingMask;
            mainCamera.cullingMask = -1;
        }

        public void HideAnchorNavigation()
        {
            isOpenAnchorNav = false;
            if(cameraOriginlayer!=0)
                mainCamera.cullingMask = cameraOriginlayer;
            foreach (var each in anchorNavs)
            {
                each.gameObject.SetActive(false);
            }
        }


        public void ClearNavData()
        {
            if(dicNavCursors!=null)
                dicNavCursors.Clear();
            anchorMarks = null;
            anchorNavs = null;
        }

            */



    }

}