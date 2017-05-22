using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace DataMesh.AR.SpectatorView
{
    public class LivePreview : MonoBehaviour
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

        #region DLLImports
        [DllImport("UnityCompositorInterface")]
        private static extern bool QueueingHoloFrames();
        #endregion

#endif

        public RawImage imageCapture;

        [HideInInspector]
        public LiveController controller;

        private RenderTexture tex = null;
        private bool hasInit = false;

        // Use this for initialization
        void Start()
        {
        }

        public void Init(LiveController con)
        {
            controller = con;
            hasInit = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (!hasInit)
                return;

            if (!controller.holoCamera.gameObject.activeSelf)
                return;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            
            if (imageCapture.texture == null)
            {
                imageCapture.texture = controller.holoCamera.shaderManager.colorTexture;
                imageCapture.material = controller.holoCamera.shaderManager.alphaBlendPreviewMat;
            }

            if (controller.holoCamera != null &&
                controller.holoCamera.shaderManager != null &&
                controller.holoCamera.shaderManager.colorTexture != null &&
                controller.holoCamera.shaderManager.renderTexture != null &&
                controller.holoCamera.shaderManager.holoTexture != null &&
                controller.holoCamera.shaderManager.alphaBlendVideoMat != null &&
                controller.holoCamera.shaderManager.alphaBlendOutputMat != null &&
                controller.holoCamera.shaderManager.alphaBlendPreviewMat != null)
            {
                controller.holoCamera.shaderManager.alphaBlendVideoMat.SetFloat("_Alpha", controller.alpha);
                controller.holoCamera.shaderManager.alphaBlendOutputMat.SetFloat("_Alpha", controller.alpha);
                controller.holoCamera.shaderManager.alphaBlendPreviewMat.SetFloat("_Alpha", controller.alpha);
            }

            if (controller.holoCamera != null &&
                controller.holoCamera.shaderManager != null)
            {
                if (
                        controller.holoCamera.shaderManager.colorTexture != null &&
                        controller.holoCamera.shaderManager.renderTexture != null &&
                        controller.holoCamera.shaderManager.holoTexture != null &&
                        controller.holoCamera.shaderManager.alphaBlendVideoMat != null &&
                        controller.holoCamera.shaderManager.alphaBlendOutputMat != null &&
                        controller.holoCamera.shaderManager.alphaBlendPreviewMat != null
                    )
                {
                    if (!controller.holoCamera.shaderManager.setAlphaBlendPreviewHoloTex)
                    {
                        controller.holoCamera.shaderManager.setAlphaBlendPreviewHoloTex = true;
                        if (QueueingHoloFrames())
                        {
                            controller.holoCamera.shaderManager.alphaBlendPreviewMat.SetTexture("_FrontTex", controller.holoCamera.shaderManager.holoTexture);
                            controller.holoCamera.shaderManager.alphaBlendVideoMat.SetTexture("_FrontTex", controller.holoCamera.shaderManager.holoTexture);
                            controller.holoCamera.shaderManager.alphaBlendOutputMat.SetTexture("_FrontTex", controller.holoCamera.shaderManager.holoTexture);
                        }
                        else
                        {
                            controller.holoCamera.shaderManager.alphaBlendPreviewMat.SetTexture("_FrontTex", controller.holoCamera.shaderManager.renderTexture);
                            controller.holoCamera.shaderManager.alphaBlendVideoMat.SetTexture("_FrontTex", controller.holoCamera.shaderManager.renderTexture);
                            controller.holoCamera.shaderManager.alphaBlendOutputMat.SetTexture("_FrontTex", controller.holoCamera.shaderManager.renderTexture);
                        }
                    }

                }

            }
            
#endif
        }
    }
}