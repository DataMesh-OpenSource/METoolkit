using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataMesh.AR.UI;

namespace DataMesh.AR.Storage
{
    public class StorageAssetData
    {
        public string assetName;
        public string assetFileName;
        public byte[] bundleBytes;
        public AssetBundle bundle;
    }

    public class StorageData
    {
        public string name;
        public string crc;
        public StorageAssetData data;

        public BlockListData CreateUIData()
        {
            BlockListData data = new BlockListData();
            data.name = name;

            data.data = this;

            return data;
        }
    }
}