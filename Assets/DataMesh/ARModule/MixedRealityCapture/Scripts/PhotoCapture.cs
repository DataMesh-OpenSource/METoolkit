using UnityEngine;
using System.Collections;
using System.Linq;
using DataMesh.AR.Common;

#if UNITY_METRO && !UNITY_EDITOR
using UnityEngine.XR.WSA.WebCam;
#endif

namespace DataMesh.AR.MRC {
    public class PhotoCaptureController
    {
        public string filename = "Image_jkafdhr983rhf89hrfunfd04858tjhfn9348jfsm943jenef82qacdzcxw578kfj.jpg";
        public string filePath;

#if UNITY_METRO && !UNITY_EDITOR
        PhotoCapture photoCaptureObject = null;
        CameraParameters cameraParameters;
#endif

        public Texture2D targetTexture = null;
        int width;
        int height;

        public PhotoCaptureController()
        {
            filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
#if UNITY_METRO && !UNITY_EDITOR
            try
            {
                Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
                width = cameraResolution.width;
                height = cameraResolution.height;
                targetTexture = new Texture2D(width, height);
                //TakeAPicture();
                cameraParameters = new CameraParameters();
                cameraParameters.hologramOpacity = 1.0f;
                cameraParameters.cameraResolutionWidth = width;
                cameraParameters.cameraResolutionHeight = height;
                cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;
            }
            catch (System.Exception e)
            {

            }
#endif
        }
        public void TakeAPicture()
        {
#if UNITY_METRO && !UNITY_EDITOR

            //Debug.Log("cameraResolution.width" + cameraResolution.width);
            //Debug.Log("cameraResolution.height" + cameraResolution.height);
            // Create a PhotoCapture object
            PhotoCapture.CreateAsync(true, delegate (PhotoCapture captureObject) {
                photoCaptureObject = captureObject;
                // Activate the camera
                photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result) {
                    // Take a picture
                    photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
                });
            });
#endif
        }
        public void TakeAPictureToDisk()
        {
#if UNITY_METRO && !UNITY_EDITOR

            PhotoCapture.CreateAsync(true, delegate (PhotoCapture captureObject) {
                photoCaptureObject = captureObject;
                // Activate the camera
                photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result) {
                    // Take a picture
                    //photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
                    //string filename = string.Format(@"CapturedImage{0}.jpg", capturedImageCount);

                    //string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
                    if (ReadWrite.Instance.FileExist(filePath))
                    {
                        ReadWrite.Instance.FileDelete(filePath);
                    }
                    photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
                });
            });
#endif
        }


#if UNITY_METRO && !UNITY_EDITOR
        void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
        {
            // Copy the raw image data into the target texture
            photoCaptureFrame.UploadImageDataToTexture(targetTexture);

            // Create a GameObject to which the texture can be applied
            //GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            //Renderer quadRenderer = quad.GetComponent<Renderer>() as Renderer;
            //quadRenderer.material = new Material(Shader.Find("Custom/Unlit/UnlitTexture"));

            //quad.transform.parent = this.transform;
            //quad.transform.localPosition = new Vector3(0.0f, 0.0f, 3.0f);

            //quadRenderer.material.SetTexture("_MainTex", targetTexture);

            // Deactivate the camera
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        }
        void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
        {
            Debug.Log("Saved Picture To Disk!");
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
            //string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);

        }
        void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
        {
            // Shutdown the photo capture resource
            photoCaptureObject.Dispose();
            photoCaptureObject = null;
            MixedRealityCapture.Instance.AfterTakeAPicture();
        }
#endif
    }
}
