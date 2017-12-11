using UnityEngine;
using System.Collections;
using System.Linq;
using DataMesh.AR.Common;

#if UNITY_METRO && !UNITY_EDITOR
using UnityEngine.XR.WSA.WebCam;
#endif

namespace DataMesh.AR.MRC {
    public class VideoCaptureController
    {
#if UNITY_METRO && !UNITY_EDITOR
        CameraParameters cameraParameters;
        VideoCapture m_VideoCapture = null;
#endif
        static readonly float MaxRecordingTime = 600.0f;
        public string filename = "Video_jkafdhr983rhf89hrfunfd04858tjhfn9348jfsm943jenef82qacdzcxw578kfj.mp4";
        public string filepath;
        public bool recording;
        float m_stopRecordingTimer = float.MaxValue;

        public VideoCaptureController()
        {
            filepath = System.IO.Path.Combine(Application.persistentDataPath, filename);
            filepath = filepath.Replace("/", @"\");

#if UNITY_METRO && !UNITY_EDITOR
            //StartVideoCaptureTest();
            try
            {
                Resolution cameraResolution = VideoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
                //Debug.Log(cameraResolution);

                float cameraFramerate = VideoCapture.GetSupportedFrameRatesForResolution(cameraResolution).OrderByDescending((fps) => fps).First();
                //Debug.Log(cameraFramerate);
                cameraParameters = new CameraParameters();
                cameraParameters.hologramOpacity = 1.0f;
                cameraParameters.frameRate = cameraFramerate;
                cameraParameters.cameraResolutionWidth = cameraResolution.width;
                cameraParameters.cameraResolutionHeight = cameraResolution.height;
                cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;
            }
            catch (System.Exception e)
            {

            }

            //StartVideoCaptureTest();
#endif
        }

        void Update()
        {
#if UNITY_METRO && !UNITY_EDITOR

            if (m_VideoCapture == null || !m_VideoCapture.IsRecording)
            {
                return;
            }

            if (Time.time > m_stopRecordingTimer)
            {
                m_VideoCapture.StopRecordingAsync(OnStoppedRecordingVideo);
            }
#endif
        }
        public void StartVideoCapture()
        {
            StartVideoCaptureTest();
        }
        void StartVideoCaptureTest()
        {
#if UNITY_METRO && !UNITY_EDITOR
            VideoCapture.CreateAsync(true, delegate (VideoCapture videoCapture)
            {
                if (videoCapture != null)
                {
                    m_VideoCapture = videoCapture;
                    Debug.Log("Created VideoCapture Instance!");

                    m_VideoCapture.StartVideoModeAsync(cameraParameters,
                                                       VideoCapture.AudioState.ApplicationAndMicAudio,
                                                       OnStartedVideoCaptureMode);
                }
                else
                {
                    Debug.LogError("Failed to create VideoCapture Instance!");
                }
            });
#endif
        }
        public void StopVideoCapture()
        {
#if UNITY_METRO && !UNITY_EDITOR

           //m_stopRecordingTimer=Time.time;
            m_VideoCapture.StopRecordingAsync(OnStoppedRecordingVideo);
#endif
        }

#if UNITY_METRO && !UNITY_EDITOR

        void OnStartedVideoCaptureMode(VideoCapture.VideoCaptureResult result)
        {
            if (result.success)
            {
                Debug.Log("Started Video Capture Mode!");

                //string timeStamp = Time.time.ToString().Replace(".", "").Replace(":", "");
                //string filename = string.Format("TestVideo_{0}.mp4", timeStamp);
                ////string filename = "Video_jkafdhr983rhf89hrfunfd04858tjhfn9348jfsm943jenef82qacdzcxw578kfj.mp4";
                //string filepath = System.IO.Path.Combine(Application.persistentDataPath, filename);
                //filepath = filepath.Replace("/", @"\");

                if (ReadWrite.Instance.FileExist(filepath))
                {
                    ReadWrite.Instance.FileDelete(filepath);
                }
                m_VideoCapture.StartRecordingAsync(filepath, OnStartedRecordingVideo);
            }
        }

        void OnStoppedVideoCaptureMode(VideoCapture.VideoCaptureResult result)
        {
            Debug.Log("Stopped Video Capture Mode!");
            MixedRealityCapture.Instance.AfterStopVideoCapture();
        }

        void OnStartedRecordingVideo(VideoCapture.VideoCaptureResult result)
        {
            Debug.Log("Started Recording Video!");
            m_stopRecordingTimer = Time.time + MaxRecordingTime;
        }

        void OnStoppedRecordingVideo(VideoCapture.VideoCaptureResult result)
        {
            Debug.Log("Stopped Recording Video!");
            m_VideoCapture.StopVideoModeAsync(OnStoppedVideoCaptureMode);
            
        }
#endif
    }
}
