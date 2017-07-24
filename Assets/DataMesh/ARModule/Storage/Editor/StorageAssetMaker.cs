using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace DataMesh.AR.Storage
{

    public static class StorageAssetMaker
    {

        [MenuItem("Assets/DataMesh/Build Storage Asset (PC and WSA)")]
        public static void MakeAsset()
        {
            if (Selection.objects.Length == 0)
            {
                Debug.LogError("Please select objects!");
                return;
            }

            string destPath = EditorUtility.SaveFolderPanel("Save Bundle To", null, null);

            int count = 0;
            for (int i = 0; i < Selection.objects.Length; i++)
            {
                var sel = Selection.objects[i];
                if (sel != null && sel is GameObject)
                {
                    Debug.Log("======= Build Asset for [" + sel.name + "] =========");

                    GameObject obj = sel as GameObject;
                    string assetPath = AssetDatabase.GetAssetOrScenePath(sel);


                    Debug.Log(destPath);
                    if (string.IsNullOrEmpty(destPath))
                    {
                        return;
                    }

                    string srcPath = destPath + "/bundles";

                    if (!Directory.Exists(srcPath))
                    {
                        Directory.CreateDirectory(srcPath);
                    }

                    Debug.Log(obj.name.ToLower());

                    AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
                    buildMap[0].assetBundleName = obj.name;
                    buildMap[0].assetNames = new string[] { assetPath };

                    BuildPipeline.BuildAssetBundles(srcPath, buildMap, BuildAssetBundleOptions.None, BuildTarget.WSAPlayer);


                    Debug.Log("Build OK!");

                    StorageAssetManager.EncryptAsset(obj.name.ToLower(), srcPath, destPath, assetPath);

                    Debug.Log("Encrypt Success!");
                    Debug.Log("=========== end ===========");

                    count++;
                }
            }

            Debug.Log("Build " + count + " Asset!");

        }


    }

}