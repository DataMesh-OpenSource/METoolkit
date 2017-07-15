using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IPreLoadAsset{
	List<string> GetAssets();
}

public static class PrefabUtils
{

    public static GameObject CreateGameObjectToParent(GameObject parent)
    {
        GameObject toCreateObject = new GameObject();
        if (parent != null)
        {
            toCreateObject.transform.parent = parent.transform;
            toCreateObject.transform.localScale = Vector3.one;
            toCreateObject.transform.localPosition = Vector3.zero;
            toCreateObject.transform.eulerAngles = Vector3.zero;
        }
        return toCreateObject;
    }

	public static GameObject CreateGameObjectToParent(GameObject parent, GameObject prefab)
	{
		GameObject toCreateObject = (GameObject)GameObject.Instantiate(prefab);
		if(parent != null){
			toCreateObject.transform.SetParent(parent.transform);
			toCreateObject.transform.localScale = prefab.transform.localScale;
			toCreateObject.transform.localPosition = prefab.transform.localPosition;
			toCreateObject.transform.localRotation = prefab.transform.localRotation;
		}
		return toCreateObject;
	}

	public class UnLoadAssetOnDestroy:MonoBehaviour{
		public System.Action progress;
		void OnDestroy(){
			if (progress != null) {progress ();}
		}
	}


	public static void DestroyAllChild(GameObject parent)
	{
		destroyAllChild(parent, null);
	}


	public static void destroyAllChild(GameObject parent, List<string> except)
	{
		Transform pt = parent.transform;
		foreach(Transform t in pt)
		{
			if (except != null)
			{
				bool find = false;
				for (int i = 0;i < except.Count; i++)
				{
					if (t.name == except[i])
					{
						find = true;
						break;
					}
				}
				if (find)
				{
					continue;
				}
			}
			if (Application.isPlaying) GameObject.Destroy(t.gameObject);
			else GameObject.DestroyImmediate(t.gameObject);
		}

	}

	/// <summary>
	/// 循环设置一个物体和之下所有子物体的显示状态 
	/// </summary>
	/// <param name="rootObject">Root object.</param>
	/// <param name="active">If set to <c>true</c> active.</param>
	public static void SetActiveRecursively(GameObject rootObject, bool active)
	{
		rootObject.SetActive(active);
		
		foreach (Transform childTransform in rootObject.transform)
		{
			SetActiveRecursively(childTransform.gameObject, active);
		}
	}


}
