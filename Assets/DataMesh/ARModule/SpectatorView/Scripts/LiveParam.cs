using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using DataMesh.AR.Utility;

namespace DataMesh.AR.SpectatorView
{
    public static class LiveParam
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        [DllImport("UnityCompositorInterface")]
        private static extern void SetAlpha(float alpha);

        [DllImport("UnityCompositorInterface")]
        private static extern void SetFrameOffset(float frameOffset);


        public const string saveFileName = "Live.param";


        /// <summary>
        /// 防抖参数：向前平滑的秒数
        /// </summary>
        private static float antiShakeBeforeTime = -0.2f;
        public static float AntiShakeBeforeTime
        {
            get { return antiShakeBeforeTime; }
            set
            {
                antiShakeBeforeTime = value;
                isDirty = true;
            }
        }

        /// <summary>
        /// 防抖参数：向后平滑的秒数
        /// </summary>
        private static float antiShakeAfterTime = 0.2f;
        public static float AntiShakeAfterTime
        {
            get { return antiShakeAfterTime; }
            set
            {
                antiShakeAfterTime = value;
                isDirty = true;
            }
        }

        /// <summary>
        /// 全息画面的透明度
        /// </summary>
        private static float alpha = 1f;
        public static float Alpha
        {
            get { return alpha; }
            set
            {
                alpha = value;
                isDirty = true;
                SetAlpha(alpha);
            }
        }

        /// <summary>
        /// 全息画面的延迟，单位为秒
        /// </summary>
        private static float syncDelayTime = 0.2f;
        public static float SyncDelayTime
        {
            get { return syncDelayTime; }
            set
            {
                syncDelayTime = value;
                isDirty = true;
            }
        }

        /// <summary>
        /// 整体音量，范围0~1
        /// </summary>
        private static float soundVolume = 1f;
        public static float SoundVolume
        {
            get { return soundVolume; }
            set
            {
                soundVolume = value;
                AudioListener.volume = soundVolume;
                isDirty = true;
            }
        }

        /// <summary>
        /// 真实画面的滤镜颜色深度
        /// </summary>
        private static float filter = 0f;
        public static float Filter
        {
            get { return filter; }
            set
            {
                filter = value;
                ShaderManager mana = GameObject.FindObjectOfType<ShaderManager>();
                mana.alphaBlendPreviewMat.SetFloat("_Filter", Filter);
                isDirty = true;
            }
        }

        /// <summary>
        /// 使用WDP连接SpectatorView的HoloLens时使用的账号
        /// </summary>
        private static string authorId = null;
        public static string AuthorId
        {
            get { return authorId; }
            set
            {
                authorId = value;
                isDirty = true;
            }
        }

        /// <summary>
        /// 使用WDP连接SpectatorView的HoloLens时使用的密码
        /// </summary>
        private static string authorPass = null;
        public static string AuthorPass
        {
            get { return authorPass; }
            set
            {
                authorPass = value;
                isDirty = true;
            }
        }

        private static bool isDirty;
        public static bool IsDirty
        {
            get { return isDirty; }
        }

        /// <summary>
        /// FOV
        /// </summary>
        private static float fov = 0;
        public static float FOV
        {
            get { return fov; }
            set
            {
                fov = value;
                isDirty = true;
            }
        }


        public static string GetSavePath()
        {
            return Application.dataPath + "/../SaveData/" + saveFileName;
        }

        /// <summary>
        /// 从存盘文件中读取配置
        /// </summary>
        public static bool LoadParam()
        {
            string path = GetSavePath();

            Dictionary<string, string> data = AppConfig.AnalyseConfigFile(path);
            if (data != null)
            {
                if (data.ContainsKey("AntiShakeBeforeTime"))
                {
                    antiShakeBeforeTime = float.Parse(data["AntiShakeBeforeTime"]);
                }
                if (data.ContainsKey("AntiShakeAfterTime"))
                {
                    antiShakeAfterTime = float.Parse(data["AntiShakeAfterTime"]);
                }
                if (data.ContainsKey("Alpha"))
                {
                    alpha = float.Parse(data["Alpha"]);
                }
                if (data.ContainsKey("SyncDelayTime"))
                {
                    syncDelayTime = float.Parse(data["SyncDelayTime"]);
                }
                if (data.ContainsKey("SoundVolume"))
                {
                    soundVolume = float.Parse(data["SoundVolume"]);
                }
                if (data.ContainsKey("AuthorId"))
                {
                    authorId = data["AuthorId"];
                }
                if (data.ContainsKey("AuthorPass"))
                {
                    authorPass = data["AuthorPass"];
                }
                if (data.ContainsKey("Filter"))
                {
                    filter = float.Parse(data["Filter"]);
                }
                if (data.ContainsKey("FOV"))
                {
                    fov = float.Parse(data["FOV"]);
                }
            }
            else
            {
                Debug.Log("Can not find save file [" + path + "]");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 将配置写入存盘文件
        /// </summary>
        public static void SaveParam()
        {
            isDirty = false;

            string path = GetSavePath();

            Dictionary<string, string> data = new Dictionary<string, string>();

            data.Add("AntiShakeBeforeTime", AntiShakeBeforeTime.ToString());
            data.Add("AntiShakeAfterTime", AntiShakeAfterTime.ToString());
            data.Add("Alpha", Alpha.ToString());
            data.Add("SyncDelayTime", SyncDelayTime.ToString());
            data.Add("SoundVolume", SoundVolume.ToString());
            data.Add("AuthorId", AuthorId);
            data.Add("AuthorPass", AuthorPass);
            data.Add("Filter", Filter.ToString());
            data.Add("FOV", FOV.ToString());

            Debug.Log("Save file [" + path + "]");
            AppConfig.SaveConfigFile(path, data);


        }

#endif
    }

}