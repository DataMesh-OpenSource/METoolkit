using UnityEngine;
using System.IO;
using System;
using DataMesh.AR.Utility;

namespace DataMesh.AR.SpectatorView
{
    public class Calibration : MonoBehaviour
    {
        [Tooltip("Enable this checkbox if your camera is mounted below or to the left of your camera.")]
        public bool RotateCalibration = false;
        private bool prevRotateCalibration;

        [HideInInspector]
        public HolographicCameraManager holoCamera;

        public Vector3 Translation { get; private set; }
        public Quaternion Rotation { get; private set; }
        public Vector2 DSLR_fov { get; private set; }
        public Vector4 DSLR_distortion { get; private set; }
        public Vector4 DSLR_matrix { get; private set; }

#if UNITY_EDITOR || UNITY_STANDALONE

        void Start()
        {
            prevRotateCalibration = RotateCalibration;

            Translation = Vector3.zero;
            Rotation = Quaternion.identity;
            DSLR_fov = Vector2.one * 60.0f;
            DSLR_distortion = Vector4.zero;
            DSLR_matrix = Vector4.zero;

            ReadCalibrationData();
        }

        void OnValidate()
        {
            if (prevRotateCalibration != RotateCalibration)
            {
                prevRotateCalibration = RotateCalibration;

                Vector3 euler = -1 * Rotation.eulerAngles;
                Rotation = Quaternion.Euler(euler);

                Translation *= -1;

                gameObject.transform.localPosition = Translation;
                gameObject.transform.localRotation = Rotation;
            }
        }

        private void ReadCalibrationData()
        {
            AppConfig config = AppConfig.Instance;
            config.LoadConfig(MEHoloConstant.CalibrationConfigFile);

            string value;
            
            // load translation 
            value = config.GetConfigByFileName(MEHoloConstant.CalibrationConfigFile, "Rotation");
            if (value != null)
            {
                String[] tokens = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                Translation = new Vector3(
                    (float)Convert.ToDouble(tokens[0]),
                    (float)Convert.ToDouble(tokens[1]),
                    (float)Convert.ToDouble(tokens[2])
                    );

                if (!RotateCalibration)
                {
                    Translation *= -1;
                }

                // Convert from OpenCV space to Unity space.
                Translation = new Vector3(Translation.x, Translation.y, -1 * Translation.z);

                Debug.Log("Loaded calibration translation: " + Translation.x + ", " + Translation.y + ", " + Translation.z);
            }

            // load rotation 
            value = config.GetConfigByFileName(MEHoloConstant.CalibrationConfigFile, "Rotation");
            if (value != null)
            {
                String[] tokens = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                Rotation = Quaternion.LookRotation(
                    // Third column as forward direction.
                    new Vector3(
                        (float)Convert.ToDouble(tokens[6]),
                        (float)Convert.ToDouble(tokens[7]),
                        (float)Convert.ToDouble(tokens[8])
                    ),
                    // Second column as up direction.
                    new Vector3(
                        (float)Convert.ToDouble(tokens[3]),
                        (float)Convert.ToDouble(tokens[4]),
                        (float)Convert.ToDouble(tokens[5])
                    )
                );

                Vector3 euler = Rotation.eulerAngles;
                if (!RotateCalibration)
                {
                    euler *= -1;
                }

                // Convert from OpenCV space to Unity space.
                euler.y *= -1;
                Rotation = Quaternion.Euler(euler);

                Debug.Log("Loaded calibration quaternion: " + Rotation.x + ", " + Rotation.y + ", " + Rotation.z + ", " + Rotation.w);
            }

            // load dslr_fov 
            value = config.GetConfigByFileName(MEHoloConstant.CalibrationConfigFile, "DSLR_fov");
            if (value != null)
            {
                String[] tokens = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                DSLR_fov = new Vector2(
                    (float)Convert.ToDouble(tokens[0]),
                    (float)Convert.ToDouble(tokens[1])
                    );

                Debug.Log("Loaded calibration fov: " + DSLR_fov.x + ", " + DSLR_fov.y);
            }

            // load dslr_distortion 
            value = config.GetConfigByFileName(MEHoloConstant.CalibrationConfigFile, "DSLR_distortion");
            if (value != null)
            {
                String[] tokens = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                DSLR_distortion = new Vector4((float)Convert.ToDouble(tokens[0]),
                    (float)Convert.ToDouble(tokens[1]),
                    (float)Convert.ToDouble(tokens[2]),
                    (float)Convert.ToDouble(tokens[3]));

                Debug.Log("Loaded calibration dslr distortion: " + DSLR_distortion.x + ", " + DSLR_distortion.y + ", " + DSLR_distortion.z + ", " + DSLR_distortion.w);
            }

            // load dslr_camera_matrix 
            value = config.GetConfigByFileName(MEHoloConstant.CalibrationConfigFile, "DSLR_camera_Matrix");
            if (value != null)
            {
                String[] tokens = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                DSLR_matrix = new Vector4((float)Convert.ToDouble(tokens[0]),
                    (float)Convert.ToDouble(tokens[1]),
                    (float)Convert.ToDouble(tokens[2]),
                    (float)Convert.ToDouble(tokens[3]));

                Debug.Log("Loaded calibration dslr matrix: " + DSLR_matrix.x + ", " + DSLR_matrix.y + ", " + DSLR_matrix.z + ", " + DSLR_matrix.w);
            }

            // Set param to Holographic camera
            holoCamera.EnableHolographicCameraMe();


        }
#endif
    }
}
