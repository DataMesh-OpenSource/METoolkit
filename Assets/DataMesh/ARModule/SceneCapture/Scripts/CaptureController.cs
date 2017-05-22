using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace DataMesh.AR.SC
{
    public class CaptureController : MonoBehaviour
    {
        //指定用于截屏的相机    
        public GameObject cameraOjbect;

        //摄像机的截图的像素分辨率
        public int captureWidth = 1268, captureHeight = 720;



        RenderTexture rt;
        string lastCapturePath;
        // Use this for initialization
        void Start()
        {
            if (cameraOjbect == null)
            {
                cameraOjbect = gameObject;
                if (cameraOjbect.GetComponent<Camera>() == null)
                {
                    Debug.Log("no camera for capture");
                    return;
                }
            }

            //Camera cam = cameraOjbect.GetComponent<Camera>();

            //动态添加rendertexture
            rt = new RenderTexture(captureWidth, captureHeight, 16, RenderTextureFormat.ARGB32);
            cameraOjbect.GetComponent<Camera>().targetTexture = rt;

            if (rt == null)
            {
                Debug.Log("rt lost again, ****");
            }
        }

        /// <summary>
        /// 通过相机截图并返回得到的图片文件（PNG格式）绝对路径
        /// </summary>
        /// <param name="fileName">指定图片文件名字</param>
        /// <returns></returns>
        public string SaveCaptureToFile(string fileName)
        {
            string resultPath = "";
            if (rt != null)
            {
                RenderTexture prev = RenderTexture.active;
                RenderTexture.active = rt;
                Texture2D png = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
                png.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);

                byte[] bytes = png.EncodeToPNG();
                resultPath = string.Format(Application.persistentDataPath + "/{0}.png", fileName);

                Debug.Log("try to save to:" + resultPath);
                File.WriteAllBytes(resultPath, bytes);
                Texture2D.Destroy(png);
                png = null;
                RenderTexture.active = prev;
                Debug.Log("capture successfully saved");
            }
            else
            {
                Debug.Log("rt is null");
            }
            lastCapturePath = resultPath;
            return resultPath;
        }

        /// <summary>
        /// 返回截图本身，不设参数则使用上一个截下的图像
        /// </summary>
        /// <param name="filePath">图像路径</param>
        /// <returns></returns>
        public Texture2D GetCapturedPNG(string filePath = "")
        {
            if (filePath.Length == 0)
            {
                filePath = lastCapturePath;
            }
            if (!File.Exists(filePath))
            {
                return null;
            }
            byte[] bytes = File.ReadAllBytes(filePath);
            Texture2D png = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
            png.LoadImage(bytes);
            png.Apply();
            return png;
        }

        /// <summary>
        /// 返回贴有截图的平面，不设参数则使用刚截下的图像
        /// </summary>
        /// <param name="filePath">图像路径</param>
        /// <returns></returns>
        public GameObject GetCapturePlane(string filePath = "")
        {
            Texture2D png = GetCapturedPNG(filePath);
            if (png == null)
            {
                return null;
            }

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.GetComponent<Renderer>().material.mainTexture = png;
            go.transform.Rotate(90, 0, 0);
            return go;
        }
    }
}