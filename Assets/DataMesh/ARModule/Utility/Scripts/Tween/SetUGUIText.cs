using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataMesh.AR.Utility;
using System;
using UnityEngine.UI;
public class SetUGUIText : UITweener {

    //float 
    Text textComponent;
    public float alpha
    {
        get
        {
            if (textComponent != null)
            {
                return textComponent.color.a;
            }
            else
            {
                return 0;
            }
        }
        set
        {

            Color c = textComponent.color;
            c.a = value;
            textComponent.color = c;
        }
    }

    private bool hasInit = false;
    void Init()
    {
        if (hasInit)
        {
            return;
        }
        textComponent = GetComponent<Text>();
        if (textComponent == null)
        {
            textComponent = GetComponentInChildren<Text>();
        }
        hasInit = true;
    }


    protected override void OnUpdate(float value, bool isFinished)
    {
        Init();
        if (textComponent == null)
        {
            return;
        }

        Debug.Log("text set:" + value);
        //throw new NotImplementedException();
    }

    public void SetText(string t)
    {
        Init();
        Debug.Log("set text:" + t+ " on"+textComponent.gameObject.name);
        textComponent.text = t;
    }


    bool breakDisappear = false;
    IEnumerator LagDisappear(float timeDelay)
    {
        float endTime = Time.time+timeDelay;
        while (Time.time < endTime)
        {
            if (breakDisappear)
            {
                yield break;
            }
            yield return null;
        }
        SetText("");
    }


    public void SetTextInvoke(Dictionary<string, string> param)
    {
        breakDisappear = false;
        if (param.ContainsKey("text"))
        {
            SetText(param["text"]);
        }
        if (param.ContainsKey("time"))
        {
            breakDisappear = false;
            StartCoroutine(LagDisappear(float.Parse(param["time"])));
        }
    }
}
