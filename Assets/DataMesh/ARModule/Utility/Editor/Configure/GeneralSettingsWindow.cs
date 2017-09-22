using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.SceneManagement;

namespace DataMesh.AR.Utility
{
    public class CameraSettings : AbstractConfigureWindow<CameraSettings.CameraSetting>
    {

        public enum CameraSetting
        {
            CameraClearBlack,//Set Camera to Solid Color and background color to 0,0,0
            CameraToOrigin,//Set the position of Camera to 0,0,0
            NearClipPlane,//update the Clipping Planes Near of Camera 
           
        }

        
        protected override void ApplySettings()
        {
            // See the blow notes for why text asset serialization is required
            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                // NOTE: PlayerSettings.virtualRealitySupported would be ideal, except that it only reports/affects whatever platform tab
                // is currently selected in the Player settings window. As we don't have code control over what view is selected there
                // this property is fairly useless from script.

                // NOTE: There is no current way to change the default quality setting from script

                string title = "Updates require text serialization of assets";
                string message = "Unity doesn't provide apis for updating the default quality\n\n" +
                    "Is it ok if we force text serialization of assets so that we can modify the properties directly?";

                bool forceText = EditorUtility.DisplayDialog(title, message, "Yes", "No");
                if (!forceText)
                {
                    return;
                }

                EditorSettings.serializationMode = SerializationMode.ForceText;
            }
          
            // Ensure we have a camera
            if (Camera.main == null)
            {
                Debug.LogWarning(@"Could not apply settings - no camera tagged with ""MainCamera""");
                return;
            }

            if (Values[CameraSetting.CameraClearBlack])
            {
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = Color.clear;
            }

            if (Values[CameraSetting.CameraToOrigin])
            {
                Camera.main.transform.position = Vector3.zero;
            }
            
            if (Values[CameraSetting.NearClipPlane])
            {
                Camera.main.nearClipPlane = 0.85f;
            }

            //this.SendEvent( EditorGUIUtility.CommandEvent()
            bool boo = EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            //Debug.Log("11111111---------->" + boo);
        }

        protected override void LoadSettings()
        {
            for (int i = (int)CameraSetting.CameraClearBlack; i <= (int)CameraSetting.NearClipPlane; i++)
            {
                Values[(CameraSetting)i] = true;
            }
        }

        protected override void LoadStrings()
        {
            Names[CameraSetting.CameraToOrigin] = "Move Camera to Origin";
            Descriptions[CameraSetting.CameraToOrigin] = "Moves the main camera to the origin of the scene (0,0,0).\n\nWhen a HoloLens application starts, the users head is the center of the world. Not having the main camera at 0,0,0 will result in holograms not appearing where they are expeted. This option should remain checked unless you have code that explicitly deals with any offset.";

            Names[CameraSetting.CameraClearBlack] = "Camera Clears to Black";
            Descriptions[CameraSetting.CameraClearBlack] = "Causes the camera to render to a black background instead of the default skybox.\n\nIn HoloLens the color black is transparent. Rendering to a black background allows the user to see the real world wherever there are no holograms. This option should remain checked unless you are building a VR-like experience or are implementing advanced rendering techniques.";

            Names[CameraSetting.NearClipPlane] = "Update Near Clipping Plane";
            Descriptions[CameraSetting.NearClipPlane] = "Updates the near clipping plane of the main camera to the recommended setting.\n\nThe recommended near clipping plane is designed to reduce eye fatigue. This option should remain checked unless you have a specific need to allow closer inspection of holograms and understand the impact of closely focused objects. (e.g. vergence accommodation conflict)";

           
        }

        protected override void OnEnable()
        {
            // Pass to base first
            base.OnEnable();

            // Set size
            this.minSize = new Vector2(350, 240);
            this.maxSize = this.minSize;
        }


      


    }

}

