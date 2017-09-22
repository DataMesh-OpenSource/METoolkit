using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using DataMesh.AR.Utility;

namespace DataMesh.AR.Storage
{
    public class StorageAssetManager
    {
        private static StorageAssetManager _instance;
        public static StorageAssetManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new StorageAssetManager();
                return _instance;
            }
        }

        private Dictionary<string, StorageAssetData> storageAssetDic = new Dictionary<string, StorageAssetData>();

        private StorageAssetManager() { }

        /// <summary>
        /// 将一个Asset加密
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        /// <param name="originAssetPath"></param>
        public static void EncryptAsset(string fileName, string srcPath, string destPath, string originAssetPath)
        {
            Debug.Log("Begin Encrypt...");

            string filePath = srcPath + "/" + fileName;

            FileStream inStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 1024, false);

            string outputFilePath = destPath + "/" + fileName + ".medb";
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }

            FileStream outStream = null;

            try
            {
                outStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);

                byte[] bt = null;
                int index = 0;

                // 1、写入资源名称和长度
                index = 0;
                byte[] assetNameBytes = Encoding.UTF8.GetBytes(fileName);
                bt = new byte[fileName.Length + sizeof(int)];
                BytesUtility.WriteIntToBytes(bt, fileName.Length, ref index);
                Array.Copy(assetNameBytes, 0, bt, index, assetNameBytes.Length);

                outStream.Write(bt, 0, bt.Length);

                // 2、写入资源全称的长度，以及资源全称 
                index = 0;
                byte[] fileNameBytes = Encoding.UTF8.GetBytes(originAssetPath);
                bt = new byte[fileNameBytes.Length + sizeof(int)];
                BytesUtility.WriteIntToBytes(bt, originAssetPath.Length, ref index);
                Array.Copy(fileNameBytes, 0, bt, index, fileNameBytes.Length);

                outStream.Write(bt, 0, bt.Length);

                Debug.Log("Write path " + originAssetPath);

                // 3、写入资源文件长度  
                bt = new byte[sizeof(long)];
                index = 0;
                BytesUtility.WriteLongToBytes(bt, inStream.Length, ref index);
                outStream.Write(bt, 0, bt.Length);

                // 4、写入资源文件 
                byte[] buffer = new byte[1024];
                Debug.Log("Write File length=" + inStream.Length);

                while (true)
                {
                    int readLen = inStream.Read(buffer, 0, 1024);
                    outStream.Write(buffer, 0, readLen);

                    //Debug.Log("Write " + readLen + " bytes");

                    if (readLen == 0)
                        break;
                }

                outStream.Flush();
            }
            catch (System.Exception e)
            {
                Debug.Log("Exception! " + e);
            }
            finally
            {
                if (outStream != null)
                {
                    outStream.Dispose();
                }
            }

        }

        /// <summary>
        /// 解密Asset
        /// </summary>
        /// <param name="assetBytes"></param>
        /// <returns></returns>
        private StorageAssetData DecryptAsset(byte[] assetBytes)
        {
            StorageAssetData data = null;

            byte[] bt = null;
            int index = 0;

            // 读取文件名长度 
            int assetNameLen;
            BytesUtility.GetIntFromBytes(assetBytes, out assetNameLen, ref index);
            Debug.Log("Read asset name Length=" + assetNameLen);

            // 读取文件名 
            bt = new byte[assetNameLen];
            Array.Copy(assetBytes, index, bt, 0, assetNameLen);
            index += assetNameLen;

            string assetName = Encoding.UTF8.GetString(bt);
            Debug.Log("Read asset name=" + assetName);

            // 读取Asset 
            // 判断是否已经加载该资源 
            if (storageAssetDic.ContainsKey(assetName))
            {
                data = storageAssetDic[assetName];
                Debug.Log("Has loaded this asset!");
            }
            else
            {
                data = new StorageAssetData();
                data.assetName = assetName;

                // 读取文件路径长度 
                int fileNameLen;
                BytesUtility.GetIntFromBytes(assetBytes, out fileNameLen, ref index);

                Debug.Log("Read asset file name Length=" + fileNameLen);

                // 读取文件路径  
                bt = new byte[fileNameLen];
                Array.Copy(assetBytes, index, bt, 0, fileNameLen);
                index += fileNameLen;

                data.assetFileName = Encoding.UTF8.GetString(bt);
                Debug.Log("Read file name=" + data.assetFileName);

                // 读取Asset文件长度 
                long fileLen;
                BytesUtility.GetLongFromBytes(assetBytes, out fileLen, ref index);
                Debug.Log("Read file len=" + fileLen);

                byte[] assetData = new byte[fileLen];
                Array.Copy(assetBytes, index, assetData, 0, (int)fileLen);

                data.bundle = AssetBundle.LoadFromMemory(assetData);
                Debug.Log("Read AssetBundle " + data.bundle);

                storageAssetDic.Add(data.assetName, data);
            }


            return data;
        }

        /// <summary>
        /// 加载一个AssetData和其中的AssetBundle
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public StorageAssetData LoadAsset(string fileName)
        {
            string filePath = StorageManager.GetFilePath() + fileName;
            if (!File.Exists(filePath))
            {
                Debug.Log("No such File! " + filePath);
                return null;
            }

            byte[] bytes = File.ReadAllBytes(filePath);

            StorageAssetData data = DecryptAsset(bytes);

            return data;
        }

        /// <summary>
        /// 卸载一个AssetData和其中的AssetBundle
        /// </summary>
        /// <param name="data"></param>
        public void ReleaseAsset(StorageAssetData data)
        {
            data.bundle.Unload(true);
            if (storageAssetDic.ContainsKey(data.assetName))
            {
                storageAssetDic.Remove(data.assetName);
            }
        }

    }

}