using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public interface IFinishObject
{
    Action<IFinishObject> cbFinish { get; set; }
}

public class FinishObjectController
{
    
    private List<IFinishObject> objList = new List<IFinishObject>();

    public Action cbFinish;

    private bool hasStart = false;

    public void AddObject(IFinishObject obj)
    {
        objList.Add(obj);

        obj.cbFinish += RemoveObject;

        //LogManager.Log("Add fo "+ obj +"! count=" + objList.Count);
    }

    public void RemoveObject(IFinishObject obj)
    {
        objList.Remove(obj);
        obj.cbFinish -= RemoveObject;

        //LogManager.Log("Remove fo " + obj + "! count=" + objList.Count);
        if (hasStart)
        {
            Check();
        }

       
    }


    public void Start()
    {
        hasStart = true;
        Check();
    }

    private void Check()
    {
       // LogManager.Log("check!!");

        if (objList.Count == 0)
        {
            if (cbFinish != null)
            {
               // LogManager.Log("Fire Finish");
                cbFinish();
            }

            cbFinish = null;
        }
    }

    public void Terminate()
    {
        for (int i = 0;i < objList.Count;i ++)
        {
            IFinishObject obj = objList[i];
            obj.cbFinish -= RemoveObject;
        }

        cbFinish = null;
    }
}

public class FinishObjectUtil
{
    private static FinishObjectUtil _instance;

    public static FinishObjectUtil getInstance()
    {
        if (_instance == null)
        {
            _instance = new FinishObjectUtil();
        }
        return _instance;
    }

    private List<FinishObjectController> cList;

    public FinishObjectController createFinishObjectController()
    {
        return new FinishObjectController();
    }
}
