using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataMesh.AR.SpectatorView
{
    public sealed class SocialAlbumInfo
    {
        private static SocialAlbumInfo instance;
        private static readonly object obj = new object();
        private SocialAlbumInfo() { }
        public static SocialAlbumInfo Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (obj)
                    {
                        if (null == instance)
                        {
                            instance = new SocialAlbumInfo();
                        }
                    }
                }
                return instance;
            }
        }

        public string currentAlbumName;
        public float recordTime;
        public string videoOutputPath;
        public string videoFileName;
        public string imageOutputPath;
        public string imageFileName;
        public UploadFileType uploadFileType;
        public List<string> listAlbumProfileName;
        public List<string> listAutoRecordTime;

    }
}

