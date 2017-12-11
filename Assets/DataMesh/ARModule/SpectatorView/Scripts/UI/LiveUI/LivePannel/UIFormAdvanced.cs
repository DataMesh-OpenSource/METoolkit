using DataMesh.AR;
using DataMesh.AR.SpectatorView;
using DataMesh.AR.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace DataMesh.AR.SpectatorView
{
    public class UIFormAdvanced : BaseUIForm
    {

        public Text textFolderPath;
        public Button buttonSelectFolder;
        public Button buttonOpenConfigFiles;
        public Button buttonOpenApplicationLogs;
        public Button buttonClearAnchorData;

        private string targetFolderPath;
        private Dictionary<string, string> dicMEconfigLive = new Dictionary<string, string>();

        private const string configFile = "MEConfigNetwork.ini";
        private const string outputFile = "output_log.txt";


#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        private void Awake()
        {
            RigisterButtonEvent(buttonOpenConfigFiles, OpenConfigFiles);
            RigisterButtonEvent(buttonOpenApplicationLogs, OpenApplicationLogs);
            RigisterButtonEvent(buttonClearAnchorData, ClearAnchorData);
            RigisterButtonEvent(buttonSelectFolder, SelectFolder);

            TargetFolderPath = AppConfig.Instance.GetConfigByFileName(MEHoloConstant.LiveConfigFile, "Out_Put_Path", "C:\\HologramCapture");

            dicMEconfigLive = new Dictionary<string, string>();
            dicMEconfigLive = AppConfig.AnalyseConfigFile(Application.streamingAssetsPath + "/" + MEHoloConstant.LiveConfigFile);
            if (dicMEconfigLive == null)
            {
                print("load failed");
            }

        }

        public void OpenConfigFiles(GameObject obj)
        {
            Process.Start(Application.streamingAssetsPath + "/" + configFile);
        }

        public void OpenApplicationLogs(GameObject obj)
        {
            Process.Start("C:\\Users\\" + Environment.UserName + "\\AppData\\LocalLow\\" 
                + Application.companyName + "\\" + Application.productName + "\\output_log.txt");
        }

        public void ClearAnchorData(GameObject obj)
        {
            string filePath = Application.dataPath + "/../SaveData/";
            if (IsFolderExists(filePath))
            {
                Directory.Delete(filePath, true);
            }
        }

        public void SelectFolder(GameObject obj)
        {
            OpenDialogDir ofn2 = new OpenDialogDir();
            ofn2.pszDisplayName = new string(new char[2000]); ;     // 存放目录路径缓冲区    
            ofn2.lpszTitle = "Open Project";// 标题    
                                            //ofn2.ulFlags = BIF_NEWDIALOGSTYLE | BIF_EDITBOX; // 新的样式,带编辑框    
            IntPtr pidlPtr = DllOpenFileDialog.SHBrowseForFolder(ofn2);

            char[] charArray = new char[2000];
            for (int i = 0; i < 2000; i++)
                charArray[i] = '\0';

            DllOpenFileDialog.SHGetPathFromIDList(pidlPtr, charArray);
            string fullDirPath = new String(charArray);
            fullDirPath = fullDirPath.Substring(0, fullDirPath.IndexOf('\0'));
            if (fullDirPath != "")
            {
                fullDirPath += "\\";
                TargetFolderPath = fullDirPath;

                dicMEconfigLive["Out_Put_Path"] = TargetFolderPath;

                AppConfig.SaveConfigFile(Application.streamingAssetsPath + "/" + MEHoloConstant.LiveConfigFile, dicMEconfigLive);
            }
        }

        public static bool IsFolderExists(string folderPath)
        {
            if (folderPath.Equals(string.Empty))
            {
                return false;
            }
            return Directory.Exists(folderPath);
        }

        public string TargetFolderPath
        {
            get { return targetFolderPath; }
            set
            {
                LiveController.Instance.outputPath = targetFolderPath = textFolderPath.text = value;

            }
        }
#endif
    }
}


