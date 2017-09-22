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

    
}
