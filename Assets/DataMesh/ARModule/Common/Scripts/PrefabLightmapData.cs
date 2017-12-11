
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
namespace DataMesh.AR.Common {

    public class PrefabLightmapData : MonoBehaviour
    {
        //void Start()
        //{
        //    Debug.Log("SetUpStart");
        //    //SetUp();
        //    Debug.Log("SetUpFinish");
        //}

        //LightMap信息
        [System.Serializable]
        public struct RendererInfo
        {
            public Renderer renderer;
            public int lightmapIndex;
            public Vector4 lightmapOffsetScale;
        }

        //场景中的Fog信息
        [System.Serializable]
        public struct FogInfo
        {
            public bool fog;
            public FogMode fogMode;
            public Color fogColor;
            public float fogStartDistance;
            public float fogEndDistance;
            public float fogDensity;
        }


        public FogInfo fogInfo;
        public List<RendererInfo> m_RendererInfo;
        public List<Texture2D> lightmapNear;
        public List<Texture2D> lightmapFar;
        public LightmapData[] lightmapData;
        public LightmapsMode lightmapsMode;

        //地形的LightMap信息
        public Terrain terrain;
        public RendererInfo terrainRendererInfo;

        //设置光照信息
        [ContextMenu("SetUp")]
        public void SetUp()
        {
            lightmapData = new LightmapData[lightmapNear.Count > lightmapFar.Count ? lightmapNear.Count : lightmapFar.Count];
            for (int i = 0; i < lightmapData.Length; i++)
            {
                lightmapData[i] = new LightmapData();
                lightmapData[i].lightmapColor = i < lightmapFar.Count ? lightmapFar[i] : null;
                lightmapData[i].lightmapDir = i < lightmapNear.Count ? lightmapNear[i] : null;
            }
            LightmapSettings.lightmapsMode = lightmapsMode;
            LightmapSettings.lightmaps = lightmapData;
            LoadLightmap();
            RenderSettings.fog = fogInfo.fog;
            RenderSettings.fogMode = fogInfo.fogMode;
            RenderSettings.fogColor = fogInfo.fogColor;
            RenderSettings.fogStartDistance = fogInfo.fogStartDistance;
            RenderSettings.fogEndDistance = fogInfo.fogEndDistance;
            RenderSettings.fogDensity = fogInfo.fogDensity;
        }

        //保存光照信息
        [ContextMenu("SaveData")]
        public void SaveData()
        {
            SaveLightmap();
        }

        public void SaveLightmap()
        {
            fogInfo = new FogInfo();
            fogInfo.fog = RenderSettings.fog;
            fogInfo.fogMode = RenderSettings.fogMode;
            fogInfo.fogColor = RenderSettings.fogColor;
            fogInfo.fogStartDistance = RenderSettings.fogStartDistance;
            fogInfo.fogEndDistance = RenderSettings.fogEndDistance;

            lightmapNear = new List<Texture2D>();
            lightmapFar = new List<Texture2D>();
            for (int i = 0; i < LightmapSettings.lightmaps.Length; i++)
            {
                LightmapData data = LightmapSettings.lightmaps[i];
                if (data.lightmapDir != null)
                {
                    lightmapNear.Add(data.lightmapDir);
                }

                if (data.lightmapColor != null)
                {
                    lightmapFar.Add(data.lightmapColor);
                }
            }
            m_RendererInfo = new List<RendererInfo>();
            var renderers = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer r in renderers)
            {
                if (r.lightmapIndex != -1)
                {
                    RendererInfo info = new RendererInfo();
                    info.renderer = r;
                    info.lightmapOffsetScale = r.lightmapScaleOffset;
                    info.lightmapIndex = r.lightmapIndex;
                    m_RendererInfo.Add(info);
                }
            }

            terrain = GetComponentInChildren<Terrain>();
            if (terrain != null)
            {
                terrainRendererInfo = new RendererInfo();
                terrainRendererInfo.lightmapOffsetScale = terrain.lightmapScaleOffset;
                terrainRendererInfo.lightmapIndex = terrain.lightmapIndex;
            }
            lightmapsMode = LightmapSettings.lightmapsMode;
        }

        public void LoadLightmap()
        {
            if (m_RendererInfo.Count <= 0) return;

            if (terrain != null)
            {
                terrain.lightmapScaleOffset = terrainRendererInfo.lightmapOffsetScale;
                terrain.lightmapIndex = terrainRendererInfo.lightmapIndex;
            }

            foreach (var item in m_RendererInfo)
            {
                item.renderer.lightmapIndex = item.lightmapIndex;
                item.renderer.lightmapScaleOffset = item.lightmapOffsetScale;
            }
        }
    }

}
