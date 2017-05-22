using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;


namespace DataMesh.AR.SpectatorView
{
    public class HolographicCameraManager : MonoBehaviour
    {
        #region DLLImports
        [DllImport("UnityCompositorInterface")]
        private static extern int GetFrameWidth();

        [DllImport("UnityCompositorInterface")]
        private static extern int GetFrameHeight();

        [DllImport("UnityCompositorInterface")]
        private static extern int GetFrameWidthHiRes();

        [DllImport("UnityCompositorInterface")]
        private static extern int GetFrameHeightHiRes();

        [DllImport("UnityCompositorInterface")]
        private static extern IntPtr GetRenderEventFunc();

        [DllImport("UnityCompositorInterface")]
        private static extern void SetAudioData(byte[] audioData);

        [DllImport("UnityCompositorInterface")]
        private static extern void Reset();

        [DllImport("UnityCompositorInterface")]
        private static extern Int64 GetCurrentUnityTime();

        [DllImport("UnityCompositorInterface")]
        private static extern bool InitializeFrameProvider();

        [DllImport("UnityCompositorInterface")]
        private static extern void StopRecording();

        [DllImport("UnityCompositorInterface")]
        private static extern void StopFrameProvider();

        [DllImport("UnityCompositorInterface")]
        private static extern void UpdateCompositor();

        [DllImport("UnityCompositorInterface")]
        private static extern void SetExplicitHoloTime(long holoTime);

        [DllImport("UnityCompositorInterface")]
        private static extern int GetFrameDelta();

        [DllImport("UnityCompositorInterface")]
        private static extern bool NewColorFrame();

        [DllImport("UnityCompositorInterface")]
        private static extern bool IsRecording();
        #endregion

        public bool IsCurrentlyActive { get; set; }

        [Header("Hologram Settings")]
        public Depth TextureDepth = Depth.TwentyFour;
        public AntiAliasingSamples AntiAliasing = AntiAliasingSamples.Eight;
        public FilterMode Filter = FilterMode.Trilinear;

        public enum Depth { None, Sixteen = 16, TwentyFour = 24 }
        public enum AntiAliasingSamples { One = 1, Two = 2, Four = 4, Eight = 8 };

        [HideInInspector]
        public bool frameProviderInitialized = false;

        [HideInInspector]
        public ShaderManager shaderManager;
        [HideInInspector]
        public Calibration calibration;

        [HideInInspector]
        public bool requestSpatialMappingData = false;

        long prevHoloTime = 0;


        public void Init()
        {
            IsCurrentlyActive = false;

#if UNITY_EDITOR || UNITY_STANDALONE
            Initialize();
#endif

            Camera[] cameras = gameObject.GetComponentsInChildren<Camera>();
            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i].enabled = false;
            }
        }

        private void Initialize()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            calibration = gameObject.AddComponent<Calibration>();
            calibration.holoCamera = this;

            shaderManager =  gameObject.AddComponent<ShaderManager>();
            shaderManager.holoCamera = this;
            shaderManager.calibration = calibration;


            // Change audio listener to the holographic camera.
            AudioListener listener = Camera.main.GetComponent<AudioListener>();
            if (listener != null)
            {
                GameObject.DestroyImmediate(listener);
            }

            listener = GetComponent<AudioListener>();
            if (listener == null)
            {
                gameObject.AddComponent<AudioListener>();
            }

            
#endif

        }

#if UNITY_EDITOR || UNITY_STANDALONE
        public void EnableHolographicCamera(Transform parent)
        {
            gameObject.transform.parent = parent;
            gameObject.transform.localPosition = calibration.Translation;
            gameObject.transform.localRotation = calibration.Rotation;
            gameObject.transform.localScale = Vector3.one;

            shaderManager.EnableHolographicCamera(parent);
            IsCurrentlyActive = true;
        }
        public void EnableHolographicCameraMe()
        {
            Common.FollowMainCamera followMainCamera = gameObject.GetComponent<Common.FollowMainCamera>();
            followMainCamera.positionOffset = calibration.Translation;
            followMainCamera.rotationOffset =  calibration.Rotation.eulerAngles;

            shaderManager.EnableHolographicCamera(gameObject.transform);

            IsCurrentlyActive = true;
        } 
        /// <summary>
        /// Restore the Holographic Camera to the root of the Unity Hierarchy.
        /// </summary>
        public void ResetHolographicCamera()
        {
            // Cache the last known position and rotation of the spectator view rig so we do not lose state while waiting for the new camera to come online.
            GameObject cachedTPPC = GameObject.Find("CachedTPPC");
            if (cachedTPPC == null)
            {
                cachedTPPC = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cachedTPPC.name = "CachedTPPC";
                cachedTPPC.GetComponent<Renderer>().enabled = false;
            }

            cachedTPPC.transform.position = gameObject.transform.position;
            cachedTPPC.transform.rotation = gameObject.transform.rotation;

            IsCurrentlyActive = false;
            gameObject.transform.parent = cachedTPPC.transform;
        }

        void OnEnable()
        {
            frameProviderInitialized = false;
            StartCoroutine("CallPluginAtEndOfFrames");
        }

        void OnDestroy()
        {
            ResetCompositor();
        }

        public void ResetCompositor()
        {
            Debug.Log("Disposing DLL Resources.");
            Reset();

            StopFrameProvider();
            if (IsRecording())
            {
                StopRecording();
            }

            if (shaderManager != null)
            {
                shaderManager.Reset();
            }
        }

        private IEnumerator CallPluginAtEndOfFrames()
        {
            while (true)
            {
                // Wait until all frame rendering is done
                yield return new WaitForEndOfFrame();

                // Issue a plugin event with arbitrary integer identifier.
                // The plugin can distinguish between different
                // things it needs to do based on this ID.
                // For our simple plugin, it does not matter which ID we pass here.
                GL.IssuePluginEvent(GetRenderEventFunc(), 1);
            }
        }

        void Update()
        {
            if (!frameProviderInitialized)
            {
                frameProviderInitialized = InitializeFrameProvider();
            }
            else if (frameProviderInitialized)
            {
                UpdateCompositor();
            }
        }

        void OnPreRender()
        {
            prevHoloTime = GetCurrentUnityTime();

            // Set delta time for TPPC HoloLens to report poses relative to color frame time.
            if (NewColorFrame())
            {
                SetExplicitHoloTime(prevHoloTime);

                int deltaTime = GetFrameDelta();
                if (deltaTime > 0) { deltaTime *= -1; }

                // Convert to nanoseconds.
                deltaTime *= 100;

                //SpectatorView.SV_CustomMessages.Instance.SendTimeOffset(deltaTime);
            }
        }

        // Send audio data to Compositor.
        void OnAudioFilterRead(float[] data, int channels)
        {
            Byte[] audioBytes = new Byte[data.Length * 2];

            for (int i = 0; i < data.Length; i++)
            {
                // Rescale float to short range for encoding.
                short audioEntry = (short)(data[i] * short.MaxValue);
                BitConverter.GetBytes(audioEntry).CopyTo(audioBytes, i * 2);
            }

            SetAudioData(audioBytes);
        }
#endif

    }
}
