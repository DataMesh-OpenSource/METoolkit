using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TimerManager : MonoBehaviour 
{
    public class TimerStruct
    {
        public float startTime;

        public int key;

        public int totalTimes;
        public int executeTimes;

        public float interal;

        public Callback innerCallback = null;
        public Action cbRemove = null;
        public Hashtable hashtable = null;

		public bool executeOncePerTimeout;

        public void RefreshStartTime()
        {
            startTime = Time.time;
        }
    }

	private int key = 1;
	private int tick;
	private static TimerManager mInstance = null;
    public Dictionary<int, TimerStruct> timerMap = new Dictionary<int, TimerStruct>();
	public delegate void Callback(Hashtable hashtable);

    private List<TimerStruct> timerList = new List<TimerStruct>();

	// 目前沒人用到，不知道要做什麽的， 先註解起來， 需要的請自行打開
    //private List<Action> todoAction = new List<Action>();
	private List<int> removeList = new List<int>();


    public System.Action OnFrame;

    private float frameInterval = 1f / 20f;
    private float lastFrameTime = -1;
    private float curTime;

    public static TimerManager Instance
	{
		get 
		{
            if (mInstance == null)
            {
                GameObject obj = new GameObject("TimerManager");
                mInstance = obj.AddComponent<TimerManager>();
            }
			return mInstance;
		}
	}

    public TimerStruct GetTimer(int key)
    {
        if (timerMap.ContainsKey(key))
            return timerMap[key];

        return null;
    }

    void Update()
    {
        // 实现稳定帧速 
        curTime = Time.time;
        if (lastFrameTime < 0)
        {
            if (OnFrame != null)
                OnFrame();
            lastFrameTime = curTime;
        }
        else
        {
            while (lastFrameTime + frameInterval < curTime)
            {
                if (OnFrame != null)
                    OnFrame();
                lastFrameTime += frameInterval;
            }
        }

        // 触发定时器 
        CheckTimer();
    }

    /// <summary>
    /// 移除一个指定id的定时器
    /// 移除时会触发定时器的移除回调
    /// </summary>
    /// <param name="removeKey">定时器的id</param>
    public void RemoveTimer(int removeKey)
    {
        TimerStruct timer = null;

        if (timerMap.ContainsKey(removeKey))
            timer = timerMap[removeKey];

        if (timer == null)
            return;

        if (timer.cbRemove != null)
            timer.cbRemove();

        timerMap.Remove(removeKey);
        timerList.Remove(timer);
    }

    /// <summary>
    /// 注册一个定时器
    /// </summary>
    /// <param name="callbackFunction">定时器回调函数</param>
    /// <param name="interal">定时器间隔，单位为秒</param>
    /// <param name="repeat">重复次数，0以下表示无限执行</param>
    /// <param name="paramsObject">参数，可以为null</param>
    /// <param name="cbRemove">移除时的回调，默认为null</param>
    /// <param name="executeOnce">同一个时间片是否只执行一次。用于Lag导致同时出发多个时间片的情况下进行控制。</param>
    /// <returns>该定时器的id</returns>
	public int RegisterTimer(Callback callbackFunction, float interal, int repeat, Hashtable paramsObject = null, Action cbRemove = null, bool executeOnce = false)
	{		
		TimerStruct timer = new TimerStruct();

        timer.startTime = Time.time;
		timer.interal = interal;
		
		timer.executeTimes = 0;
		timer.totalTimes = repeat;
		
		timer.key = key++;
		timer.innerCallback += callbackFunction;
        timer.cbRemove = cbRemove;
		timer.hashtable = paramsObject;
		timer.executeOncePerTimeout = executeOnce;

        timerMap.Add(timer.key,timer);
        timerList.Add(timer);

        //foreach (Callback cb in timer.innerCallback)
        //    cb(timer.hashtable);		

		return timer.key;	
	}


	public void RunLater(Callback callbackFunction, Hashtable paramsObject)
	{
		RegisterTimer(callbackFunction, 0, 0, paramsObject);
	}


    public int RunLater(Callback callbackFunction, int delay, Action cbRemove = null, Hashtable paramsObject = null)
    {
        return RegisterTimer(callbackFunction, delay, 0, paramsObject, cbRemove);
    }


    private void CheckTimer()
	{
		for (int i = 0;i < timerList.Count;i ++)
		{
            TimerStruct timer = timerList[i];
			while (curTime - timer.startTime >= timer.interal)
			{
                timer.startTime += timer.interal;

                // 执行回调 
                if (timer.innerCallback != null)
                    timer.innerCallback(timer.hashtable);

                // 累加执行次数 
                timer.executeTimes += 1;

                // 判断执行次数是否到达上限 
                if (timer.totalTimes > 0 && timer.executeTimes >= timer.totalTimes)
                {
                    // 只标记移除，后面会统一移除 
                    removeList.Add(timer.key);
                    break;
                }

                // 如果单时间片只执行一次，则不再循环，并重置时间 
                if (timer.executeOncePerTimeout)
                {
                    while (curTime - timer.startTime >= timer.interal)
                    {
                        timer.startTime += timer.interal;
                    }
                    break;
                }
            }

        }

        // 移除已经完成的定时器 
        if (removeList.Count > 0)
        {
            for (int i = 0; i < removeList.Count; i++)
            {
                RemoveTimer(removeList[i]);
            }

            removeList.Clear();
        }

	}
}
