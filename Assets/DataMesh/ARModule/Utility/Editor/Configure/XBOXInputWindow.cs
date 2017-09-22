using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace DataMesh.AR.Utility
{
    public class XBOXInputWindow : AbstractConfigureWindow<XBOXInputWindow.XBOXInputSetting>
    {
        public enum XBOXInputSetting
        {
            XBOXInputConfig
        }

        #region 需要添加的配置
        private string add;
        private Dictionary<string, string> addConfigDic = new Dictionary<string, string>()
        {
            {"RightHorizontal","\n" + @"  - serializedVersion: 3
    m_Name: RightHorizontal
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: 
    altNegativeButton: 
    altPositiveButton: 
    gravity: 0
    dead: 0.19
    sensitivity: 1
    snap: 0
    invert: 0
    type: 2
    axis: 3
    joyNum: 0" },

             {"RightVertical", @"  - serializedVersion: 3
    m_Name: RightVertical
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: 
    altNegativeButton: 
    altPositiveButton: 
    gravity: 0
    dead: 0.19
    sensitivity: 1
    snap: 0
    invert: 1
    type: 2
    axis: 4
    joyNum: 0" },

              {"JoystickLB", @"  - serializedVersion: 3
    m_Name: JoystickLB
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: joystick button 4
    altNegativeButton: 
    altPositiveButton: 
    gravity: 1000
    dead: 0.001
    sensitivity: 1000
    snap: 0
    invert: 0
    type: 0
    axis: 0
    joyNum: 0" },

               {"JoystickRB", @"  - serializedVersion: 3
    m_Name: JoystickRB
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: joystick button 5
    altNegativeButton: 
    altPositiveButton: 
    gravity: 1000
    dead: 0.001
    sensitivity: 1000
    snap: 0
    invert: 0
    type: 0
    axis: 0
    joyNum: 0" },

             {"JoystickView", @"  - serializedVersion: 3
    m_Name: JoystickView
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: joystick button 6
    altNegativeButton: 
    altPositiveButton: 
    gravity: 1000
    dead: 0.001
    sensitivity: 1000
    snap: 0
    invert: 0
    type: 0
    axis: 0
    joyNum: 0" },

              {"JoystickMenu",@"  - serializedVersion: 3
    m_Name: JoystickMenu
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: joystick button 7
    altNegativeButton: 
    altPositiveButton: 
    gravity: 1000
    dead: 0.001
    sensitivity: 1000
    snap: 0
    invert: 0
    type: 0
    axis: 0
    joyNum: 0" },

              {"LeftAnalog",@"  - serializedVersion: 3
    m_Name: LeftAnalog
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: joystick button 8
    altNegativeButton: 
    altPositiveButton: 
    gravity: 1000
    dead: 0.001
    sensitivity: 1000
    snap: 0
    invert: 0
    type: 0
    axis: 0
    joyNum: 0" },

             {"RightAnalog",@"  - serializedVersion: 3
    m_Name: RightAnalog
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: joystick button 9
    altNegativeButton: 
    altPositiveButton: 
    gravity: 1000
    dead: 0.001
    sensitivity: 1000
    snap: 0
    invert: 0
    type: 0
    axis: 0
    joyNum: 0" },

              {"JoystickLT",@"  - serializedVersion: 3
    m_Name: JoystickLT
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: 
    altNegativeButton: 
    altPositiveButton: 
    gravity: 0
    dead: 0.19
    sensitivity: 1
    snap: 0
    invert: 0
    type: 2
    axis: 8
    joyNum: 0" },

               {"JoystickRT",@"  - serializedVersion: 3
    m_Name: JoystickRT
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: 
    altNegativeButton: 
    altPositiveButton: 
    gravity: 0
    dead: 0.19
    sensitivity: 1
    snap: 0
    invert: 0
    type: 2
    axis: 9
    joyNum: 0" },

             {"JoystickA",@"  - serializedVersion: 3
    m_Name: JoystickA
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: joystick button 0
    altNegativeButton: 
    altPositiveButton: 
    gravity: 1000
    dead: 0.001
    sensitivity: 1000
    snap: 0
    invert: 0
    type: 0
    axis: 0
    joyNum: 0" },

              {"JoystickB",@"  - serializedVersion: 3
    m_Name: JoystickB
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: joystick button 1
    altNegativeButton: 
    altPositiveButton: 
    gravity: 1000
    dead: 0.001
    sensitivity: 1000
    snap: 0
    invert: 0
    type: 0
    axis: 0
    joyNum: 0" },

               {"JoystickX",@"  - serializedVersion: 3
    m_Name: JoystickX
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: joystick button 2
    altNegativeButton: 
    altPositiveButton: 
    gravity: 1000
    dead: 0.001
    sensitivity: 1000
    snap: 0
    invert: 0
    type: 0
    axis: 0
    joyNum: 0" },

              {"JoystickY",@"  - serializedVersion: 3
    m_Name: JoystickY
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: joystick button 3
    altNegativeButton: 
    altPositiveButton: 
    gravity: 1000
    dead: 0.001
    sensitivity: 1000
    snap: 0
    invert: 0
    type: 0
    axis: 0
    joyNum: 0" }
        };


        #endregion


        protected override void ApplySettings()
        {
            if (Values[XBOXInputSetting.XBOXInputConfig])
            {
                string settingsPath = "ProjectSettings/InputManager.asset";
                string content = File.ReadAllText(settingsPath);
                StreamWriter writer = File.AppendText(settingsPath);
                bool isNeedReload = false;

                foreach (var keyValue in addConfigDic)
                {
                    //如果不包含，则添加
                    if (!content.Contains(keyValue.Key))
                    {
                        isNeedReload = true;
                        writer.WriteLine(keyValue.Value);
                        writer.Flush();
                    }
                }
                writer.Close();
                writer = null;

                if (isNeedReload)
                {
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

            }
        }

        protected override void LoadSettings()
        {
            Values[XBOXInputSetting.XBOXInputConfig] = true;
        }

        protected override void LoadStrings()
        {
            Names[XBOXInputSetting.XBOXInputConfig] = "Config XBOX Input";
            Descriptions[XBOXInputSetting.XBOXInputConfig] = "Config XBOX Input Manager";
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
