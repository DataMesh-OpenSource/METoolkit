using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class UnityHelper : MonoBehaviour
    {
        /// <summary>
        /// 查找子节点对象
        /// </summary>
        /// <returns></returns>
        public static Transform FindTheChildNode(GameObject parent, string childName)
        {
            Transform searchTrans = null;
            searchTrans = parent.transform.Find(childName);
            if (searchTrans == null)
            {
                foreach (Transform trans in parent.transform)
                {
                    searchTrans = FindTheChildNode(trans.gameObject , childName);
                    if (searchTrans != null)
                        return searchTrans;
                }
            }
            return searchTrans;
        }

        /// <summary>
        /// 获取子对象的脚本
        /// </summary>
        public static T GetTheChildNodeComponentScripts<T>(GameObject parent, string childName) where T : Component
        {
            Transform searchTransformNode = null;
            searchTransformNode = FindTheChildNode(parent , childName);
            if (searchTransformNode != null)
            {
                return searchTransformNode.gameObject.GetComponent<T>();
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 给子节点添加脚本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static T AddChildNodeComponent<T>(GameObject parent, string childName) where T : Component
        {
            Transform searchTransform = null;
            searchTransform = FindTheChildNode(parent , childName);
            if (searchTransform != null)
            {
                T[] componentScriptsArray = searchTransform.GetComponents<T>();
                for (int i = 0; i < componentScriptsArray.Length; i++)
                {
                    if (componentScriptsArray[i] != null)
                    {
                        Destroy(componentScriptsArray[i]);
                    }
                }
                return searchTransform.gameObject.AddComponent<T>();
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 给子节点添加父类
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        public static void AddParentNode(Transform parent, Transform child)
        {
            child.SetParent(parent,false);
            child.localScale = Vector3.one;
            child.localEulerAngles = Vector3.zero;
            child.localPosition = Vector3.zero;
        }


    }
