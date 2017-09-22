using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace DataMesh.AR.Utility
{
    public class WSAConfigureWindow : AbstractConfigureWindow<WSAConfigureWindow.WSASetting>
    {
        public enum WSASetting
        {
            BuildForWindowsStore,
            VirtualRealitySupported,
            ChangeQualityToFastest //Open Edit -> Project Setting -> Quality, subsequently check all Levels and close V Sync Count of each level
        }

        protected override void ApplySettings()
        {
            if (Values[WSASetting.BuildForWindowsStore])
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.WSAPlayer);
                EditorUserBuildSettings.wsaSDK = WSASDK.UWP;
                EditorUserBuildSettings.wsaSubtarget = WSASubtarget.HoloLens;
                EditorUserBuildSettings.wsaUWPBuildType = WSAUWPBuildType.D3D;

            }
            if (Values[WSASetting.VirtualRealitySupported])
            {
                //PlayerSettings.virtualRealitySupported = true;
                EnableVirtualReality();
            }

            if (Values[WSASetting.ChangeQualityToFastest])
            {
                SetFastestDefaultQuality();
            }

            bool canReload = EditorUtility.DisplayDialog(
                "Project reload required!",
                "Some changes require a project reload to take effect.\n\nReload now?",
                "Yes", "No");

            if (canReload)
            {
                string projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                EditorApplication.OpenProject(projectPath);
            }

        }

        protected override void LoadSettings()
        {
            for (int i = (int)WSASetting.BuildForWindowsStore; i <= (int)WSASetting.ChangeQualityToFastest; i++)
            {
                Values[(WSASetting)i] = true;
            }

        }

        protected override void LoadStrings()
        {
            Names[WSASetting.BuildForWindowsStore] = "Build For Windows Store";
            Descriptions[WSASetting.BuildForWindowsStore] = "Set the build target for Windows Store";

            Names[WSASetting.VirtualRealitySupported] = "Virtual Reality Supported";
            Descriptions[WSASetting.VirtualRealitySupported] = "Virtual Reality Supported";

            Names[WSASetting.ChangeQualityToFastest] = "Change Quality To Fastest";
            Descriptions[WSASetting.ChangeQualityToFastest] = "Change Quality To Fastest";
        }

        protected override void OnEnable()
        {
            // Pass to base first
            base.OnEnable();

            // Set size
            this.minSize = new Vector2(350, 240);
            this.maxSize = this.minSize;
        }

        private void EnableVirtualReality()
        {
            try
            {
                // Grab the text from the project settings asset file
                string settingsPath = "ProjectSettings/ProjectSettings.asset";
                string settings = File.ReadAllText(settingsPath);

                // We're looking for the list of VR devices for the current build target, then
                // ensuring that the HoloLens is in that list
                bool foundBuildTargetVRSettings = false;
                bool foundBuildTargetMetro = false;
                bool foundBuildTargetEnabled = false;
                bool foundDevices = false;
                bool foundHoloLens = false;

                StringBuilder builder = new StringBuilder(); // Used to build the final output
                string[] lines = settings.Split(new char[] { '\n' });
                for (int i = 0; i < lines.Length; ++i)
                {
                    string line = lines[i];

                    // Look for the build target VR settings
                    if (!foundBuildTargetVRSettings)
                    {
                        if (line.Contains("m_BuildTargetVRSettings:"))
                        {
                            // If no targets are enabled at all, just create the known entries and skip the rest of the tests
                            if (line.Contains("[]"))
                            {
                                // Remove the empty array symbols
                                line = line.Replace(" []", "\n");

                                // Generate the new lines
                                line += "  - m_BuildTarget: Metro\n";
                                line += "    m_Enabled: 1\n";
                                line += "    m_Devices:\n";
                                line += "    - HoloLens";

                                // Mark all fields as found so we don't search anymore
                                foundBuildTargetVRSettings = true;
                                foundBuildTargetMetro = true;
                                foundBuildTargetEnabled = true;
                                foundDevices = true;
                                foundHoloLens = true;
                            }
                            else
                            {
                                // The target VR settngs were found but the others
                                // still need to be searched for.
                                foundBuildTargetVRSettings = true;
                            }
                        }
                    }

                    // Look for the build target for Metro
                    else if (!foundBuildTargetMetro)
                    {
                        if (line.Contains("m_BuildTarget: Metro"))
                        {
                            foundBuildTargetMetro = true;
                        }
                    }

                    else if (!foundBuildTargetEnabled)
                    {
                        if (line.Contains("m_Enabled"))
                        {
                            line = "    m_Enabled: 1";
                            foundBuildTargetEnabled = true;
                        }
                    }

                    // Look for the enabled Devices list
                    else if (!foundDevices)
                    {
                        if (line.Contains("m_Devices:"))
                        {
                            // Clear the empty array symbols if any
                            line = line.Replace(" []", "");
                            foundDevices = true;
                        }
                    }

                    // Once we've found the list look for HoloLens or the next non element
                    else if (!foundHoloLens)
                    {
                        // If this isn't an element in the device list
                        if (!line.Contains("-"))
                        {
                            // add the hololens element, and mark it found
                            builder.Append("    - HoloLens\n");
                            foundHoloLens = true;
                        }

                        // Otherwise test if this is the hololens device
                        else if (line.Contains("HoloLens"))
                        {
                            foundHoloLens = true;
                        }
                    }

                    builder.Append(line);

                    // Write out a \n for all but the last line
                    // NOTE: Specifically preserving unix line endings by avoiding StringBuilder.AppendLine
                    if (i != lines.Length - 1)
                    {
                        builder.Append('\n');
                    }
                }

                // Capture the final string
                settings = builder.ToString();

                File.WriteAllText(settingsPath, settings);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }


        private void SetFastestDefaultQuality()
        {
            try
            {
                // Find the WSA element under the platform quality list and replace it's value with 0
                string settingsPath = "ProjectSettings/QualitySettings.asset";
                string matchPattern = @"(m_PerPlatformDefaultQuality.*Windows Store Apps:) (\d+)";
                string replacePattern = @"$1 0";

                string settings = File.ReadAllText(settingsPath);
                settings = Regex.Replace(settings, matchPattern, replacePattern, RegexOptions.Singleline);

                File.WriteAllText(settingsPath, settings);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
