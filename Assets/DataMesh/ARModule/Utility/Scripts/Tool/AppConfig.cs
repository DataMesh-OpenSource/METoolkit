using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace DataMesh.AR.Utility
{
    public class AppConfig 
    {
        protected Dictionary<string, Dictionary<string,string>> dicConfigList = new Dictionary<string, Dictionary<string, string>>();

        public string configFileSourcePath;
        public string configFilePath;

        private static AppConfig _instance;

        private AppConfig()
        {
            configFileSourcePath = Application.streamingAssetsPath;
#if !UNITY_EDITOR && !UNITY_STANDALONE
            configFilePath = Application.persistentDataPath;
#else
            configFilePath = configFileSourcePath;
#endif

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
        /// 从指定名称的配置文件中获取一条设置，如果找不到设置则返回null
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetConfigByFileName(string fileName, string key)
        {
            return GetConfigByFileName(fileName, key, null);
        }

        /// <summary>
        /// 从指定名称的配置文件中获取一条设置，如果找不到设置则返回defaultValue
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetConfigByFileName(string fileName, string key, string defaultValue)
        {
            string value = defaultValue;

            if (dicConfigList.ContainsKey(fileName))
            {
                Dictionary<string, string> dic = dicConfigList[fileName];

                if (dic.ContainsKey(key))
                {
                    value = dic[key];
                }
            }

            return value;
        }

        public Vector3 GetVector3ConfigByFileName(string fileName, string key, Vector3 defaultValue)
        {
            string value = GetConfigByFileName(fileName, key, null);
            if (value == null)
                return defaultValue;

            string[] p = value.Split(',');
            if (p.Length != 3)
            {
                return defaultValue;
            }

            Vector3 rs = new Vector3();
            for (int i = 0;i <p.Length;i ++)
            {
                float f;
                if (!float.TryParse(p[i], out f))
                {
                    return defaultValue;
                }
                rs[i] = f;
            }

            return rs;
        }

        /// <summary>
        /// 加载一个配置文件 
        /// </summary>
        /// <param name="FileName"></param>
        public void LoadConfig(string FileName)
        {
            string sourceFilePath = configFileSourcePath + "/" + FileName;
            string filePath = configFilePath + "/" + FileName;

            bool found = false;
            if (!File.Exists(filePath))
            {
                // 如果配置目录中没有文件，则去检查一下源目录，如果有则复制过来 
                if (configFilePath != configFileSourcePath)
                {
                    if (File.Exists(sourceFilePath))
                    {
                        File.Copy(sourceFilePath, filePath);
                        found = true;
                    }
                }
            }
            else
            {
                found = true;
            }

            if (!found)
            {
                Debug.LogWarning("Can not found config file [" + FileName + "]");
                return;
            }

            Debug.Log("Config path: " + filePath);

            Dictionary<string, string> Str_Dic = AnalyseConfigFile(filePath);
            if (Str_Dic == null)
                return;

            if (dicConfigList.ContainsKey(FileName))
            {
                dicConfigList.Remove(FileName);
            }

            dicConfigList.Add(FileName, Str_Dic);

        }

        /// <summary>
        /// 解析一行配置文件的内容
        /// </summary>
        /// <param name="str">一行配置</param>
        /// <param name="key">输出的key，如果解析失败则为null</param>
        /// <param name="value">输出的value，如果解析失败则为null</param>
        public static void AnalyseConfigString(string str, out string key, out string value)
        {
            key = null;
            value = null;

            if (str == null)
                return;

            str = str.Trim();

            // 排除空行 
            if (str == "")
                return;

            // 排除注释行 
            if (str.Substring(0, 1) == "#")
                return;

            //Debug.Log("Config str: " + str);

            // 用等号拆解
            int index = str.IndexOf("=");
            if (index < 0)
                return;

            key = str.Substring(0, index).Trim();
            if (key == "")
                return;

            value = str.Substring(index + 1).Trim();
        }

        /// <summary>
        /// 从指定的文件路径，读取并解析一个配置文件
        /// </summary>
        /// <param name="path">指定的文件路径</param>
        /// <returns></returns>
        public static Dictionary<string, string> AnalyseConfigFile(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            Dictionary<string, string> rs = new Dictionary<string, string>();
            string[] strs = File.ReadAllLines(path);

            for (int i = 0; i < strs.Length; i++)
            {
                string str = strs[i];

                string key, value;
                AnalyseConfigString(str, out key, out value);


                if (key != null && value != null)
                {
                    Debug.Log("Load Config [" + key + "]=" + value);
                    rs.Add(key, value);
                }
                else
                {
                    //Debug.Log("Config [" + key + "] not exist");
                }
            }

            return rs;
        }

        /// <summary>
        /// 将配置文件写入指定的文件中
        /// </summary>
        /// <param name="path">指定的文件路径</param>
        /// <param name="data">配置，key-value对</param>
        public static void SaveConfigFile(string path, Dictionary<string, string> data)
        {
            int index = path.LastIndexOf("/");
            if (index >= 0)
            {
                string dir = path.Substring(0, index + 1);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }

            string content = "";
            foreach (string key in data.Keys)
            {
                string value = data[key];
                content += key + " = " + value + "\r\n";
            }

            try
            {
                File.WriteAllText(path, content);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Save File Exception! " + e);
            }


            
        }
    }


}