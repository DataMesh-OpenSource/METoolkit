using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEHoloClient.Utils;

namespace DataMesh.AR.UI
{

    public class BlockMenuManager : MonoBehaviour
    {
        /// <summary>
        /// 预设的面板数据 
        /// </summary>
        public List<TextAsset> menuDataList;

        /// <summary>
        /// 创建menu所使用的prefab 
        /// </summary>
        public GameObject menuPrefab;

        /// <summary>
        /// 当前面板关闭的回调
        /// </summary>
        [HideInInspector]
        public System.Action cbMenuHide;

        //private bool hasMenuShow = false;
        private BlockMenu currentMenu = null;

        private Dictionary<string, BlockMenu> menuDic = new Dictionary<string, BlockMenu>();

        private bool hasTurnOn = false;


        public void Init()
        {
            for (int i = 0; i < menuDataList.Count; i++)
            {
                TextAsset t = menuDataList[i];
                if (t == null)
                    continue;

                string str = t.text;

                BlockMenuData data = JsonUtil.Deserialize<BlockMenuData>(str);

                CreateMenu(data);
            }
        }

        public void TurnOn()
        {
            hasTurnOn = true;
        }

        public void TurnOff()
        {
            hasTurnOn = false;
        }



        /// <summary>
        /// 用数据创建一个面板 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool CreateMenu(BlockMenuData data)
        {

            if (menuDic.ContainsKey(data.name))
            {
                // 已经有重名Menu 
                Debug.Log("已经存在同名Menu!");
                return false;
            }

            GameObject obj = PrefabUtils.CreateGameObjectToParent(null, menuPrefab);
            obj.name = "Menu[" + data.name + "]";
            BlockMenu menu = obj.GetComponent<BlockMenu>();
            menuDic.Add(data.name, menu);

            menu.BuildMenu(data);

            return true;
        }

        /// <summary>
        /// 通过menu名称获取menu
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public BlockMenu GetMenu(string name)
        {
            if (!menuDic.ContainsKey(name))
                return null;

            return menuDic[name];
        }

        /// <summary>
        /// 添加一个新menu
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public bool AddMenu(BlockMenu menu)
        {
            if (menuDic.ContainsKey(menu.menuName))
                return false;

            menuDic.Add(menu.menuName, menu);

            return true;
        }

        /// <summary>
        /// 判断是否有面板打开了。同时只允许打开一个面板
        /// </summary>
        /// <returns></returns>
        public bool HasMenuShow()
        {
            return currentMenu != null;
        }

        /// <summary>
        /// 打开一个菜单。打开位置在视线注视位置2米距离。
        /// 同时，这里会自动
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="pos"></param>
        /// <param name="forward"></param>
        /// <returns></returns>
        public bool OpenMenu(string menuName)
        {
            if (!hasTurnOn)
            {
                Debug.Log("Menu System has not turn on!");
                return false;
            }
            BlockMenu menu = GetMenu(menuName);
            if (menu == null)
                return false;

            return OpenMenu(menu);
        }

        /// <summary>
        /// 打开一个菜单。打开位置在视线注视位置2米距离。
        /// 同时，这里会自动
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="pos"></param>
        /// <param name="forward"></param>
        /// <returns></returns>
        public bool OpenMenu(BlockMenu menu)
        {
            if (!hasTurnOn)
            {
                Debug.Log("Menu System has not turn on!");
                return false;
            }

            Vector3 headPosition = Camera.main.transform.position;
            Vector3 gazeDirection = Camera.main.transform.forward;

            Vector3 pos = headPosition + gazeDirection * 2;

            bool rs = ShowMenu(menu, pos, gazeDirection);
            return rs;
        }

        /// <summary>
        /// 用面板名称方式打开一个面板，打开位置由参数指定。因为同时只允许打开一个面板，所以有面板打开时，调用此方法会返回false。
        /// </summary>
        /// <param name="menuName"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool ShowMenu(string menuName, Vector3 pos, Vector3 forward)
        {
            if (!hasTurnOn)
            {
                Debug.Log("Menu System has not turn on!");
                return false;
            }
            BlockMenu menu = GetMenu(menuName);
            if (menu == null)
                return false;

            return ShowMenu(menu, pos, forward);
        }

        /// <summary>
        /// 打开一个面板。因为同时只允许打开一个面板，打开位置由参数指定，所以有面板打开时，调用此方法会返回false。
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool ShowMenu(BlockMenu menu, Vector3 pos, Vector3 forward)
        {
            if (!hasTurnOn)
            {
                Debug.Log("Menu System has not turn on!");
                return false;
            }
            // 只允许打开一个面板
            if (currentMenu != null)
                return false;

            menu.Show(pos, forward);

            currentMenu = menu;
            currentMenu.cbHide = OnMenuHide;

            return true;
        }

        /// <summary>
        /// 关闭当前面板
        /// </summary>
        public void HideCurrentMenu()
        {
            if (currentMenu == null)
                return;

            currentMenu.NeedHideImmediately();
        }

        private void OnMenuHide()
        {
            currentMenu.cbHide -= OnMenuHide;

            if (cbMenuHide != null)
                cbMenuHide();

            //Debug.Log("menu has hide");

            currentMenu = null;

            // 恢复手势识别 
            //GazeGestureManager.Instance.StartCapture();

        }

    }

}