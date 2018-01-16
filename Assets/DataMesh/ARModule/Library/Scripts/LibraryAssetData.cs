using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataMesh.AR.UI;

namespace DataMesh.AR.Library
{
    public class LibraryAssetData
    {
        public string assetName;
        public string assetFileName;
        public byte[] bundleBytes;
        public AssetBundle bundle;
    }

    public class LibraryData
    {
        public string name;
        public string crc;
        public LibraryAssetData data;

        public BlockListData CreateUIData()
        {
            BlockListData data = new BlockListData();
            data.name = name;

            data.data = this;

            return data;
        }
    }
}