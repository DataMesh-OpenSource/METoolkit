using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace DataMesh.AR.Utility
{

    public class METoolKitConfigureMenu : MonoBehaviour
    {

        /// <summary>
        /// Applies recommended scene settings to the current scenes
        /// </summary>
        [MenuItem("DataMesh/Configure/Camera Setting", false, 1)]
        public static void CameraSettings()
        {
            CameraSettings window = (CameraSettings)EditorWindow.GetWindow(typeof(CameraSettings), true, "Camera Settings");
            window.Show();
        }

        /// <summary>
        /// Applies recommended project settings to the current project
        /// </summary>
        [MenuItem("DataMesh/Configure/PC Standalone Setting", false, 2)]
        public static void PCStandaloneSetting()
        {
            PCStandaloneSettingWindow window = (PCStandaloneSettingWindow)EditorWindow.GetWindow(typeof(PCStandaloneSettingWindow), true, "PC Settings");
            window.Show();
        }

        /// <summary>
        /// Applies recommended capability settings to the current project
        /// </summary>
        [MenuItem("DataMesh/Configure/Windows Store Setting", false, 3)]
        static void WindowsStoreSetting()
        {
            WSAConfigureWindow window = (WSAConfigureWindow)EditorWindow.GetWindow(typeof(WSAConfigureWindow), true, "Windows Store Settings");
            window.Show();
        }

        /// <summary>
        /// Applies recommended capability settings to the current project
        /// </summary>
        [MenuItem("DataMesh/Configure/Capability Settings", false, 4)]
        static void HoloLensCapabilitySettings()
        {
            CapabilityConfigureWindow window = (CapabilityConfigureWindow)EditorWindow.GetWindow(typeof(CapabilityConfigureWindow), true, "Capability Settings");
            window.Show();
        }

        /// <summary>
        /// Applies recommended capability settings to the current project
        /// </summary>
        [MenuItem("DataMesh/Configure/XBOX Input Manager Settings", false, 5)]
        static void InputManagerSettings()
        {
            XBOXInputWindow window = (XBOXInputWindow)EditorWindow.GetWindow(typeof(XBOXInputWindow), true, "XBOX Input Manager Settings");
            window.Show();
        }

        /// <summary>
        /// Displays a help page for the HoloToolkit.
        /// </summary>
        [MenuItem("DataMesh/Configure/Show Help", false, 6)]
        public static void ShowHelp()
        {
            Application.OpenURL("http://docs.datamesh.com/projects/me-live/en/latest/METoolkit-overview/");
        }
    }
}