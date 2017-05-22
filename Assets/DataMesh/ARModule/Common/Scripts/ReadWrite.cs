using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using System;

namespace DataMesh.AR.Common {
    public class ReadWrite : DataMesh.AR.MEHoloModuleSingleton<ReadWrite>
    {
        //public TextAsset TxtFile;
        //private string Mytxt;
        //public Text txt;
        //不同平台下StreamingAssets的路径是不同的，这里需要注意一下。  
        private string PathURL;
        private string persistentDataPath;
        //private string filepath;
        ////切记，你的二进制文件一定要放在StreamingAssets ！！！！！！

        protected override void _Init()
        {
            persistentDataPath = Application.persistentDataPath.ToString();

#if UNITY_ANDROID   //安卓  
        		PathURL ="jar:file://" + Application.dataPath + "!/assets/";  
#elif UNITY_IPHONE  //iPhone  
        		PathURL =Application.dataPath + "/Raw/";  
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR  //windows平台和web平台  
            PathURL = "file://" + Application.dataPath + "\\StreamingAssets\\";
#else
            PathURL = string.Empty;
#endif
        }

        protected override void _TurnOn()
        {
            
        }

        protected override void _TurnOff()
        {
            
        }

        IEnumerator getAssetbundleFromNet(string url)
        {
            WWW www = new WWW(url);
            yield return www;
            //GameObject instance = Instantiate(www.assetBundle.mainAsset) as GameObject;
            byte[] bytes = www.bytes;
            string name = "cbue.fbx";
            WriteFile(name, bytes);

        }
        void Example()
        {
            PlayerPrefs.SetString("Player Name", "Foobar");
        }

        public void testWriteFile()
        {
            string Filename = Application.dataPath + "\\StreamingAssets\\MyText.txt";
            int num = Filename.LastIndexOf("\\");
            string name = Filename.Substring(num + 1, Filename.Length - num - 1);
            Filename = Application.persistentDataPath.ToString() + "\\" + name;
            WriteTXT(Filename, 1, "改为123");
        }
        public string ReadTextAsset(string name)
        {
            return ((TextAsset)Resources.Load(name)).text;
        }
        public string ReadTXT(string FilePath, int linenumber)
        {
            FilePath = GetFileFullName(FilePath);
            if (!File.Exists(FilePath))
            {
                return string.Empty;
            }
            string[] strs = File.ReadAllLines(FilePath);
            if (linenumber == 0)
            {
                return string.Empty;
            }
            else
            {
                return strs[linenumber - 1];
            }
        }
        public void WriteTXT(string FilePath, int linenumber, string txt)
        {
            if (!FileExist(FilePath))
            {
            }
            FilePath = GetFileFullName(FilePath);
            string[] strs = File.ReadAllLines(FilePath);
            if (linenumber != 0)
            {
                strs[linenumber - 1] = txt;
                File.WriteAllLines(FilePath, strs);
            }

        }
 
        public bool FileExist(string FilePath)
        {
            return File.Exists(FilePath);
        }
        public bool DirExist(string DirPath)
        {
            return Directory.Exists(DirPath);
        }
        public bool DirDelete(string DirPath)
        {
            DirectoryInfo di = new DirectoryInfo(DirPath);
            di.Delete(true);

            return !Directory.Exists(DirPath);
        }
        public bool FileDelete(string FilePath)
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
            return !File.Exists(FilePath);
        }
        public void CreateDir(string DirPath)
        {
            if (Directory.Exists(DirPath))
            {
            }
            else
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(DirPath);
                directoryInfo.Create();
            }
        }
        public void CreateFile(string FilePath)
        {
            if (File.Exists(FilePath))
            {
            }
            else
            {
                FileInfo fileInfo = new FileInfo(FilePath);
                fileInfo.Create();
            }
        }
        public string[] GetFiles(string FilePath)
        {
            FilePath = GetFileFullName(FilePath);
            ArrayList filelist = GetFileInfo(FilePath);
            string[] files = (string[])filelist.ToArray(typeof(string));
            return files;
        }
        public ArrayList GetFileInfo(string FilePath)
        {
            FilePath = GetFileFullName(FilePath);
            DirectoryInfo theFolder = new DirectoryInfo(FilePath);
            DirectoryInfo[] dirInfo = theFolder.GetDirectories();
            FileInfo[] fileInfo = theFolder.GetFiles();
            //遍历文件夹
            ArrayList filelist = new ArrayList();
            foreach (FileInfo NextFile in fileInfo)
            {
                //print(NextFile.FullName);
                filelist.Add(NextFile.FullName);
            }
            foreach (DirectoryInfo NextFolder in dirInfo)
            {
                ArrayList tmpfilelist = GetFileInfo(NextFolder.FullName);
                foreach (string tmpfilefullname in tmpfilelist)
                {
                    //print(tmpfilefullname);
                    filelist.Add(tmpfilefullname);
                }
            }
            return filelist;
        }
        public void DeletePersistentDataPathAllFile()
        {
            string[] files = GetFiles(Application.persistentDataPath.ToString());
            foreach (string tmpfilefullname in files)
            {
                //print(tmpfilefullname);
                FileDelete(tmpfilefullname);

                //string FilePath = GetFileFullName(tmpfilefullname);
                //DirDelete(FilePath.Substring(0, FilePath.Length - GetFileName(FilePath).Length));

            }

            DirectoryInfo theFolder = new DirectoryInfo(Application.persistentDataPath.ToString());
            DirectoryInfo[] dirInfo = theFolder.GetDirectories();
            foreach (DirectoryInfo NextFolder in dirInfo)
            {
                DirDelete(NextFolder.FullName);


            }
        }
        public void CopyAllFileToPersistentDataPath()
        {
            string[] files = GetFiles(GetStreamingAssetsPath());
            foreach (string tmpfilefullname in files)
            {
                //print(tmpfilefullname);
                CopyFileToPersistentDataPath(tmpfilefullname);
            }
        }
        public void CopyFileToPersistentDataPath(string FilePath)
        {
            FilePath = GetFileFullName(FilePath);
            //print(FilePath);
            //int num = FilePath.LastIndexOf("\\");
            int num = GetStreamingAssetsPath().Length;
            //print(num);
            string name = FilePath.Substring(num, FilePath.Length - num);
            //print(name);
            byte[] readbytes = File.ReadAllBytes(FilePath);

            CreateDir(Application.persistentDataPath.ToString() + "\\" + FilePath.Substring(num, FilePath.Length - num - GetFileName(FilePath).Length));
            if (FileExist(Application.persistentDataPath.ToString() + "\\" + name))
            {
                FileDelete(Application.persistentDataPath.ToString() + "\\" + name);
            }
            File.WriteAllBytes(Application.persistentDataPath.ToString() + "\\" + name, readbytes);
        }
        public void CopyFile(string FilePath, string ToDir, string ToFilePath)
        {
            System.IO.File.Copy(FilePath, ToFilePath);
        }
        public byte[] GetFileBytes(string FilePath)
        {
            FilePath = GetFileFullName(FilePath);
            byte[] readbytes = File.ReadAllBytes(FilePath);
            return readbytes;
        }
        public void WriteFile(string name, byte[] bytes)
        {
            File.WriteAllBytes(Application.persistentDataPath.ToString() + "\\" + name, bytes);
        }
        public void WriteFileAppend(string name, byte[] bytes)
        {
            string FilePath = Application.persistentDataPath.ToString() + "\\" + name;
            if (!File.Exists(FilePath))
            {
                CreateFile(FilePath);
            }
            byte[] readbytes = File.ReadAllBytes(Application.persistentDataPath.ToString() + "\\" + name);
            byte[] tmpBuffer = new byte[readbytes.Length + bytes.Length];
            readbytes.CopyTo(tmpBuffer, 0);
            bytes.CopyTo(tmpBuffer, readbytes.Length);
            File.WriteAllBytes(Application.persistentDataPath.ToString() + "\\" + name, tmpBuffer);
        }
        public string GetFileName(string FilePath)
        {
            FilePath = GetFileFullName(FilePath);
            int num = FilePath.LastIndexOf("\\");
            string name = FilePath.Substring(num + 1, FilePath.Length - num - 1);
            //print(name);
            return name;
        }
        public string GetStreamingAssetsPathFileName(string FilePath)
        {
            FilePath = GetFileFullName(FilePath);
            int num = GetStreamingAssetsPath().Length;
            //print(num);
            string name = FilePath.Substring(num, FilePath.Length - num);
            //print(name);
            return name;
        }
        public string GetStreamingAssetsPath()
        {
            return GetDirFullName(Application.dataPath + "\\StreamingAssets\\");
        }
        public string GetPersistentDataPath()
        {

            return GetDirFullName(Application.persistentDataPath.ToString());
        }
        public string GetFileLastWriteTime(string FilePath)
        {
            FileInfo theFile = new FileInfo(FilePath);
            return theFile.LastWriteTime.ToString();
        }
        public string GetFileFullName(string FilePath)
        {
            FileInfo theFile = new FileInfo(FilePath);
            return theFile.FullName;
        }
        public string GetDirFullName(string DirPath)
        {
            DirectoryInfo theDir = new DirectoryInfo(DirPath);
            return theDir.FullName;
        }
    }

}
