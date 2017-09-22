using UnityEngine;
using System.Collections;

public class CoroutineUtil
{
    private static CoreCoroutines _this = null;

	public static CoreCoroutines instance
    {
        get
        {
            if (_this == null)
            {
                GameObject go = new GameObject("COROUTINES");
                go.hideFlags = HideFlags.HideAndDontSave;
                GameObject.DontDestroyOnLoad(go);
                _this = go.AddComponent<CoreCoroutines>();
            }
            return _this;
        }
    }

    public static Coroutine Run(IEnumerator function)
    {
        return instance.StartCoroutine(function);
    }

    public static void Stop(IEnumerator function)
    {
        instance.StopCoroutine(function);
    }

    public static void StopAll()
    {
        instance.StopAllCoroutines();
    }

     

}

public class CoreCoroutines : MonoBehaviour { }
