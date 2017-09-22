using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DataMesh.AR.Utility;

public class InfoText : MonoBehaviour
{
    public Text text;

    void Awake()
    {
        TweenPosition tween = GetComponent<TweenPosition>();
        if (tween != null)
        {
            tween.AddFinishAction(DestoryMe, true);
        }
    }

    public void SetText(string s)
    {
        text.text = s;
    }

    public void DestoryMe()
    {
        GameObject.Destroy(this.gameObject);
    }
}
