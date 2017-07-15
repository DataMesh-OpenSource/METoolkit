using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading;

#if UNITY_METRO && !UNITY_EDITOR
using System.Threading.Tasks;
#endif

using UnityEngine;

namespace DataMesh.AR.Log
{
    internal class LogEntry
    {
        public string logName;

        private Queue logDataQueue;
        private FileStream fileStream;
        private StreamWriter writer;

        public float lastFlushTime = 0;

        private Queue _logDataQueue = new Queue();

        public LogEntry()
        {
            // 创建线程安全的队列 
            logDataQueue = Queue.Synchronized(_logDataQueue);
        }

        /// <summary>
        /// 初始化Log信息，创建并持续打开Log文件
        /// </summary>
        public void Init()
        {
            string path = LogManager.saveFilePathRoot;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            DateTime dt = DateTime.Now;
            string filePath = path + logName + dt.ToString("_[yyyy-MM-dd]") + ".log";

            /*
            if (!File.Exists(filePath))
            {
                File.Create(filePath);
            }
            */

            try
            {
                fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                writer = new StreamWriter(fileStream);
            }
            catch (System.Exception e)
            {
                Debug.Log("Create file stream error! " + e);
            }

            Debug.Log("Create Log[" + logName + "]!");
            Debug.Log("-->File: " + filePath);
        }

        public void AddLogData(string log)
        {
            logDataQueue.Enqueue(log);
        }

        /// <summary>
        /// 将缓存的Log写入文件 
        /// </summary>
        public void WriteFile()
        {
            //Debug.Log("Log [" + logName + "] Write to file");
            int count = 0;
            bool hasWrite = false;
            while (logDataQueue.Count > 0)
            {
                string data = (string)logDataQueue.Dequeue();
                //Debug.Log("Find a log: " + data);
                if (fileStream != null && writer != null)
                {
                    try
                    {
                        writer.WriteLine(data);
                    }
                    catch (System.Exception e)
                    {
                        //Debug.Log("Write log file Error! " + e);
                    }
                    hasWrite = true;

                    count++;
                }
            }
            if (hasWrite)
            {
                try
                {
                    writer.Flush();
                }
                catch (System.Exception e)
                {
                    //Debug.Log("Flush error! " + e);
                }

                //Debug.Log("Write " + count + " logs!");
            }
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            if (fileStream != null)
            {
                try
                {
                    fileStream.Dispose();
                }
                catch (System.Exception e)
                {
                }
            }
            if (writer != null)
            {
                try
                {
                    writer.Dispose();
                }
                catch (System.Exception e)
                {
                }
            }

            logDataQueue.Clear();

            Debug.Log("Release log[" + logName + "]");
        }
    }

    public class LogManager
    {
        static private LogManager _instance;
        static private readonly object _lock = new object();

        static public LogManager Instance
        {
            get
            {
                // 确保线程安全 
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new LogManager();
                    }
                    return _instance;
                }
            }
        }

        public static string saveFilePathRoot;

#if UNITY_METRO && !UNITY_EDITOR
        private Task logTask;
#else
        private Thread logThread;
#endif

        private LogManager()
        {

#if UNITY_METRO && !UNITY_EDITOR
            logTask = new Task(LogWorker);
            logTask.Start();
#else
            logThread = new Thread(LogWorker);
            logThread.Start();
#endif

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            saveFilePathRoot = Application.dataPath + "/../logs/";
#else
            saveFilePathRoot = Application.persistentDataPath + "/logs/";
#endif

        }


        public void Clear()
        {
#if UNITY_METRO && !UNITY_EDITOR
            logTask.ContinueWith((task)=>
                {
                    OnWorkerStop();
                }
            );
#else
            logThread.Abort();
            OnWorkerStop();
#endif
        }

        ///================================================================

        public const float LOG_FLUSH_INTERVAL = 1.0f;

        private Dictionary<string, LogEntry> logEntryDic = new Dictionary<string, LogEntry>();
        private object logDicLock = new object();

        /// <summary>
        /// Log线程实体
        /// </summary>
        private void LogWorker()
        {
            while (true)
            {
                
                lock (logDicLock)
                {
                    foreach (LogEntry entry in logEntryDic.Values)
                    {
                        entry.WriteFile();
                    }
                }
                

#if UNITY_METRO && !UNITY_EDITOR
                Task.Delay((int)(LOG_FLUSH_INTERVAL * 1000)).Wait();
#else
                Thread.Sleep((int)(LOG_FLUSH_INTERVAL * 1000));
#endif
            }
        }

        private void OnWorkerStop()
        {
            Debug.Log("Stop Log Worker!");
            lock (logDicLock)
            {
                foreach (LogEntry entry in logEntryDic.Values)
                {
                    entry.Clear();
                }

                logEntryDic.Clear();
            }
        }

        /// <summary>
        /// 创建一个新的Log对象
        /// </summary>
        /// <param name="logName"></param>
        private LogEntry GetLog(string logName)
        {
            lock (logDicLock)
            {
                LogEntry entry = null;
                if (!logEntryDic.ContainsKey(logName))
                {
                    entry = new LogEntry();
                    entry.logName = logName;
                    entry.Init();
                    logEntryDic.Add(logName, entry);
                }
                else
                {
                    entry = logEntryDic[logName];
                }

                return entry;
            }
        }

        /// <summary>
        /// 移除一个Log，不再记录 
        /// </summary>
        /// <param name="logName"></param>
        public void StopLog(string logName)
        {
            lock (logDicLock)
            {
                if (logEntryDic.ContainsKey(logName))
                {
                    LogEntry entry = logEntryDic[logName];
                    entry.Clear();

                    logEntryDic.Remove(logName);
                }
            }
        }

        /// <summary>
        /// 在指定Log中记录一条信息
        /// </summary>
        /// <param name="logName"></param>
        /// <param name="logData"></param>
        public void Log(string logName, string logData)
        {
            lock (logDicLock)
            {
                LogEntry entry = GetLog(logName);
                if (entry != null)
                {
                    DateTime dt = DateTime.Now;
                    string data = dt.ToString("[HH:mm:ss fff]") + logData;
                    entry.AddLogData(data);

                    //Debug.Log("Log:" + data);
                }
            }
        }

    }
}
