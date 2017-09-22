using UnityEngine;
using System.IO;
using System;
using DataMesh.AR.Utility;

namespace DataMesh.AR.SpectatorView
{
    public class CalibrationManager : MonoBehaviour
    {
        [Tooltip("Enable this checkbox if your camera is mounted below or to the left of your camera.")]
        public bool RotateCalibration = false;
        private bool prevRotateCalibration;

#if UNITY_EDITOR || UNITY_STANDALONE

        public CalibrationData data { get; private set; }


        public void Init()
        {
            prevRotateCalibration = RotateCalibration;

            data = new CalibrationData();

            data.Translation = Vector3.zero;
            data.Rotation = Quaternion.identity;
            data.DSLR_fov = Vector2.one * 60.0f;
            data.DSLR_distortion = Vector4.zero;
            data.DSLR_matrix = Vector4.zero;

            ReadCalibrationData();
        }

        /*
        void OnValidate()
        {
            if (prevRotateCalibration != RotateCalibration)
            {
                prevRotateCalibration = RotateCalibration;

                Vector3 euler = -1 * data.Rotation.eulerAngles;
                data.Rotation = Quaternion.Euler(euler);

                data.Translation *= -1;

                gameObject.transform.localPosition = data.Translation;
                gameObject.transform.localRotation = data.Rotation;
            }
        }
        */

        private void ReadCalibrationData()
        {
            AppConfig config = AppConfig.Instance;
            config.LoadConfig(MEHoloConstant.CalibrationConfigFile);

            string value;
            
            // load translation 
            value = config.GetConfigByFileName(MEHoloConstant.CalibrationConfigFile, "Translation");
            if (value != null)
            {
                String[] tokens = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                data.Translation = new Vector3(
                    (float)Convert.ToDouble(tokens[0]),
                    (float)Convert.ToDouble(tokens[1]),
                    (float)Convert.ToDouble(tokens[2])
                    );

                if (!RotateCalibration)
                {
                    data.Translation *= -1;
                }

                // Convert from OpenCV space to Unity space.
                data.Translation = new Vector3(data.Translation.x, data.Translation.y, -1 * data.Translation.z);

                Debug.Log("Loaded calibration translation: " + data.Translation.x + ", " + data.Translation.y + ", " + data.Translation.z);
            }

            // load rotation 
            value = config.GetConfigByFileName(MEHoloConstant.CalibrationConfigFile, "Rotation");
            if (value != null)
            {
                String[] tokens = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                data.Rotation = Quaternion.LookRotation(
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

                Vector3 euler = data.Rotation.eulerAngles;
                if (!RotateCalibration)
                {
                    euler *= -1;
                }

                // Convert from OpenCV space to Unity space.
                euler.y *= -1;
                data.Rotation = Quaternion.Euler(euler);

                Debug.Log("Loaded calibration quaternion: " + data.Rotation.x + ", " + data.Rotation.y + ", " + data.Rotation.z + ", " + data.Rotation.w);
            }

            // load dslr_fov 
            value = config.GetConfigByFileName(MEHoloConstant.CalibrationConfigFile, "DSLR_fov");
            if (value != null)
            {
                String[] tokens = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                data.DSLR_fov = new Vector2(
                    (float)Convert.ToDouble(tokens[0]),
                    (float)Convert.ToDouble(tokens[1])
                    );

                Debug.Log("Loaded calibration fov: " + data.DSLR_fov.x + ", " + data.DSLR_fov.y);
            }

            // load dslr_distortion 
            value = config.GetConfigByFileName(MEHoloConstant.CalibrationConfigFile, "DSLR_distortion");
            if (value != null)
            {
                String[] tokens = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                data.DSLR_distortion = new Vector4((float)Convert.ToDouble(tokens[0]),
                    (float)Convert.ToDouble(tokens[1]),
                    (float)Convert.ToDouble(tokens[2]),
                    (float)Convert.ToDouble(tokens[3]));

                Debug.Log("Loaded calibration dslr distortion: " + data.DSLR_distortion.x + ", " + data.DSLR_distortion.y + ", " + data.DSLR_distortion.z + ", " + data.DSLR_distortion.w);
            }

            // load dslr_camera_matrix 
            value = config.GetConfigByFileName(MEHoloConstant.CalibrationConfigFile, "DSLR_camera_Matrix");
            if (value != null)
            {
                String[] tokens = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                data.DSLR_matrix = new Vector4((float)Convert.ToDouble(tokens[0]),
                    (float)Convert.ToDouble(tokens[1]),
                    (float)Convert.ToDouble(tokens[2]),
                    (float)Convert.ToDouble(tokens[3]));

                Debug.Log("Loaded calibration dslr matrix: " + data.DSLR_matrix.x + ", " + data.DSLR_matrix.y + ", " + data.DSLR_matrix.z + ", " + data.DSLR_matrix.w);
            }


        }
#endif
    }
}
