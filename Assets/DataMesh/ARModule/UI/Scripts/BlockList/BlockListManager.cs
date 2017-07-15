using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataMesh.AR.UI
{
    public class BlockListManager : MonoBehaviour
    {
        public GameObject blockListPrefab;

        private bool hasTurnOn = false;

        public void Init()
        {
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
        /// 创建一个块列表面板
        /// </summary>
        /// <param name="uiSizeX"></param>
        /// <param name="uiSizeY"></param>
        /// <returns></returns>
        public BlockList CreateBlockList(int uiSizeX, int uiSizeY)
        {
            GameObject obj = PrefabUtils.CreateGameObjectToParent(gameObject, blockListPrefab);
            BlockList ui = obj.GetComponent<BlockList>();
            ui.CountX = uiSizeX;
            ui.CountY = uiSizeY;

            ui.Init();

            return ui;
        }

        /// <summary>
        /// 以默认尺寸创建一个快列表面板
        /// </summary>
        /// <returns></returns>
        public BlockList CreateBlockList()
        {
            GameObject obj = PrefabUtils.CreateGameObjectToParent(gameObject, blockListPrefab);
            BlockList ui = obj.GetComponent<BlockList>();

            ui.Init();

            return ui;
        }
    }
}