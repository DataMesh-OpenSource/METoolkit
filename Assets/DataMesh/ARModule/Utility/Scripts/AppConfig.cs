using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace DataMesh.AR.Utility
{
    public class AppConfig
    {
        private static AppConfig _instance;

        /// <summary>
        /// 所有配置的记录
        /// </summary>
        private Dictionary<string, string> configList = new Dictionary<string, string>();

        private AppConfig()
        {
            Init();
        }

        public static AppConfig Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AppConfig();
                return _instance;
            }
        }

        /// <summary>
        /// 初始化，会从streamingAssets中读取指定名称的配置文件
        /// </summary>
        public void Init()
        {
            string PathURL;
#if UNITY_ANDROID
        		PathURL ="jar:file://" + Application.dataPath + "!/assets/";  
#elif UNITY_IPHONE
            PathURL = Application.dataPath + "/Raw/";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR||UNITY_METRO
            PathURL = Application.dataPath + "/StreamingAssets/";
#else
            PathURL = string.Empty;
#endif
            
            string FilePath = PathURL + "/config.ini";

#if !UNITY_EDITOR && !UNITY_STANDALONE
            string path = Application.persistentDataPath.ToString() + "/config.ini";
            if (!File.Exists(path))
            {
                File.Copy(FilePath, path);
            }
        
#else
            string path = Application.streamingAssetsPath.ToString() + "/config.ini";
#endif
            //Debug.Log(FilePath);
            //Debug.Log(File.Exists(FilePath));
            //if (!File.Exists(path))
            //{
            //}

            configList.Clear();

            string[] strs = File.ReadAllLines(path);
            for (int i = 0; i < strs.Length; i++)
            {
                string str = strs[i].Trim();

                // 排除空行 
                if (str == "")
                    continue;

                // 排除注释行 
                if (str.Substring(0, 1) == "#")
                    continue;

                // 用等号拆解
                int index = str.IndexOf("=");
                if (index < 0)
                    continue;

                string key = str.Substring(0, index).Trim();
                if (key == "")
                    continue;

                string value = str.Substring(index + 1).Trim();

                Debug.Log("Load Config [" + key + "]=" + value);

                configList.Add(key, value);
            }
        }


        /// <summary>
        /// 获取一条设置 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetConfig(string key)
        {
            return GetConfig(key, null);
        }

        public string GetConfig(string key, string defaultValue)
        {
            if (configList.ContainsKey(key))
            {
                return configList[key];
            }

            return defaultValue;
        }

        
        

    }
}