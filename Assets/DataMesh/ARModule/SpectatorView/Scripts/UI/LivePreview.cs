using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace DataMesh.AR.SpectatorView
{
    public class LivePreview : MonoBehaviour, IPointerClickHandler
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

        #region DLLImports
        [DllImport("UnityCompositorInterface")]
        private static extern bool QueueingHoloFrames();
        #endregion

        public RawImage imageCapture;
        public RectTransform LivePreviewImage;

        public Text recordText;

        [HideInInspector]
        public LiveController controller;


        private RenderTexture tex = null;
        private bool hasInit = false;

        private float screenWidth;
        private float screenHeight;
        private float aspect;

        private float frameAspect;

        private RectTransform LivePreviewPanel;

        private bool isFullScreen = false;


        // Use this for initialization
        void Start()
        {
        }

        public void Init(LiveController con, float width, float _frameAspect)
        {
            controller = con;

            aspect = (float)Screen.width / (float)Screen.height;
            
            screenWidth = width;
            screenHeight = screenWidth / aspect;

            UnityEngine.Debug.Log("Screen=[" + screenWidth + "," + screenHeight + "]");

            frameAspect = _frameAspect;

            LivePreviewPanel = transform as RectTransform;

            RefreshPreview();

            recordText.gameObject.SetActive(false);


            hasInit = true;
        }

        public void SetFullScreen(bool b)
        {
            isFullScreen = b;
            RefreshPreview();
        }

        private void RefreshPreview()
        {
            if (isFullScreen)
            {
                float w = screenWidth;
                float h = w / frameAspect;
                LivePreviewPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, screenWidth);
                LivePreviewPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, screenHeight);
                LivePreviewImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
                LivePreviewImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
            }
            else
            {
                float w = screenWidth / 3;
                float h = w / frameAspect;
                LivePreviewPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
                LivePreviewPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
                LivePreviewImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
                LivePreviewImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
            }
        }

        public void ShowRecordText(bool b)
        {
            recordText.gameObject.SetActive(b);
        }

        // Update is called once per frame
        void Update()
        {
            if (!hasInit)
                return;

            if (controller.holoCamera == null || controller.holoCamera.shaderManager == null)
                return;

            if (!controller.holoCamera.gameObject.activeSelf)
                return;

            if (Time.time>3.5f && imageCapture.texture == null)
            {
                //imageCapture.texture = controller.holoCamera.shaderManager.colorTexture;
                imageCapture.material = controller.holoCamera.shaderManager.alphaBlendPreviewMat;
            }

            /*
            if (controller.holoCamera != null &&
                controller.holoCamera.shaderManager != null &&
                controller.holoCamera.shaderManager.colorTexture != null &&
                controller.holoCamera.shaderManager.renderTexture != null &&
                controller.holoCamera.shaderManager.holoTexture != null &&
                controller.holoCamera.shaderManager.alphaBlendVideoMat != null &&
                controller.holoCamera.shaderManager.alphaBlendOutputMat != null &&
                controller.holoCamera.shaderManager.alphaBlendPreviewMat != null)
            {
                controller.holoCamera.shaderManager.alphaBlendVideoMat.SetFloat("_Alpha", LiveParam.Alpha);
                controller.holoCamera.shaderManager.alphaBlendOutputMat.SetFloat("_Alpha", LiveParam.Alpha);
                controller.holoCamera.shaderManager.alphaBlendPreviewMat.SetFloat("_Alpha", LiveParam.Alpha);
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
            */
            
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2)
            {
                if (isFullScreen)
                {
                    //controller.liveUI.ExitFullScreen();
                }
                else
                {
                    //controller.liveUI.OnFullScreen();
                }
            }
        }
#else
        public void OnPointerClick(PointerEventData eventData)
        {
        }

#endif
    }
}