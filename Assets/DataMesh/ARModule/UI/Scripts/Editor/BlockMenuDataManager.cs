using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using MEHoloClient.Utils;
using DataMesh.AR.Utility;

namespace DataMesh.AR.UI
{

    public class BlockMenuDataEditor : EditorWindow
    {
        //private BlockMenuData menu = null;
        private string defaultFilePath = "Asset/Resources/UI/Data";

        private string filePath = null;

        private FileStream file;

        private bool dirty = false;

        private int[] order = { 1, 0, 2, 3 };
        private string[] names = { "Right Top", "Left Top", "Left Bottom", "Right Bottom" };


        [MenuItem("DataMesh/Open Block Menu Maker")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            BlockMenuDataEditor window = (BlockMenuDataEditor)EditorWindow.GetWindow(typeof(BlockMenuDataEditor));

        }

        private Vector2 scrollPos;
        void OnGUI()
        {
            BlockMenuDataManager bm = BlockMenuDataManager.Instance;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            if (GUILayout.Button("Load Menu Data"))
            {
                if (bm.menu != null && dirty)
                {
                    if (!EditorUtility.DisplayDialog("confirm", "Current menu has not been saved. Give up all changes and load menu?", "Yes", "Cancel"))
                    {
                        return;
                    }
                }

                string[] filter = { "JSon files", "json" };
                filePath = EditorUtility.OpenFilePanelWithFilters("Load JSon", defaultFilePath, filter);

                Debug.Log("File:" + filePath);
                if (filePath == null)
                    return;

                LoadFile(filePath);
                dirty = false;

            }

            if (GUILayout.Button("Create Mew Menu"))
            {
                if (bm.menu != null && dirty)
                {
                    if (!EditorUtility.DisplayDialog("confirm", "Current menu has not been saved. Give up all changes and create new menu?", "Yes", "Cancel"))
                    {
                        return;
                    }
                }

                string[] filter = { "JSon files", "json" };
                filePath = EditorUtility.SaveFilePanel("Save JSon", defaultFilePath, "New Menu", "json");
                if (filePath != null)
                {
                    Debug.Log("File:" + filePath);

                    bm.menu = new BlockMenuData();

                    SaveFile(filePath);
                    dirty = false;
                }


            }

            if (GUILayout.Button("Save Menu"))
            {
                if (dirty && filePath != null)
                {
                    SaveFile(filePath);

                    dirty = false;
                }


            }

            if (GUILayout.Button("Clean Menu"))
            {
                if (dirty && filePath != null)
                {
                    if (bm.menu != null && dirty)
                    {
                        if (!EditorUtility.DisplayDialog("confirm", "Current menu has not been saved. Give up all changes?", "Yes", "Cancel"))
                        {
                            return;
                        }
                    }
                }

                bm.menu = null;

                dirty = false;

                filePath = null;


            }

            ShowMenuEditor();

            EditorGUILayout.EndScrollView();
        }

        private void ShowMenuEditor()
        {
            BlockMenuDataManager bm = BlockMenuDataManager.Instance;

            if (bm.menu == null)
                return;

            GUI.changed = false;

            bm.menu.name = EditorGUILayout.TextField("Menu Name", bm.menu.name);

            if (GUI.changed)
                dirty = true;

            int level = 1;


            if (bm.menu.rootPanel == null)
            {
                if (GUILayout.Button("Create Root Panel"))
                {
                    BlockPanelData panel = new BlockPanelData();
                    bm.menu.rootPanel = panel;
                    dirty = true;
                }
            }
            else
            {
                DrawBlockPanel(bm.menu.rootPanel, level);
            }


        }

        private bool IsOppositeQuadrant(int q1, int q2)
        {
            int q = q1 + 2;
            if (q >= 4)
                q -= 4;

            if (q == q2)
                return true;

            return false;
        }

        public void DrawBlockPanel(BlockPanelData panel, int level, int fromQuadrant = -1)
        {
            for (int j = panel.buttons.Count; j < order.Length; j++)
            {
                panel.buttons.Add(null);
            }

            string panelTitle = level == 1 ? "Root Panel" : "Level " + level + " Panel";

            if (EditorTools.DrawHeader(panelTitle))
            {

                EditorTools.BeginContents();

                for (int i = 0; i < order.Length; i++)
                {
                    int quadrant = order[i];
                    string name = names[i];

                    if (EditorTools.DrawHeader(panelTitle + " -- " + names[quadrant] + " Button"))
                    {

                        EditorTools.BeginContents();

                        if (IsOppositeQuadrant(quadrant, fromQuadrant))
                        {
                            EditorGUILayout.LabelField("Can not use");
                        }
                        else
                        {
                            BlockButtonData button = panel.buttons[quadrant];

                            if (button == null)
                            {
                                if (GUILayout.Button("Create Button"))
                                {
                                    button = new BlockButtonData();
                                    panel.buttons[quadrant] = button;
                                    dirty = true;
                                }
                            }
                            else
                            {
                                DrawBlockButton(button, quadrant, level);
                                if (GUILayout.Button("Delete Button"))
                                {
                                    panel.buttons[quadrant] = null;
                                    dirty = true;
                                }
                            }

                        }
                        EditorTools.EndContents();
                    }

                }

                if (GUI.changed)
                {
                    dirty = true;
                }

                EditorTools.EndContents();
            }
        }

        private void DrawBlockButton(BlockButtonData button, int quadrant, int level)
        {
            BlockMenuDataManager bm = BlockMenuDataManager.Instance;

            GUI.changed = false;
            button.buttonId = EditorGUILayout.TextField("Button ID", button.buttonId);
            button.buttonName = EditorGUILayout.TextField("Button Name", button.buttonName);
            button.buttonColor = EditorGUILayout.ColorField("Button Color", button.buttonColor);
            button.buttonPic = EditorGUILayout.TextField("Button Pic", button.buttonPic);
            button.canClick = EditorGUILayout.Toggle("Can Click?", button.canClick);
            if (GUI.changed)
                dirty = true;

            if (button.subPanel == null)
            {
                if (GUILayout.Button("Create Sub Panel"))
                {
                    button.subPanel = new BlockPanelData();
                    dirty = true;
                }
            }
            else
            {
                DrawBlockPanel(button.subPanel, level + 1, quadrant);
            }
        }

        private void LoadFile(string filePath)
        {
            FileStream file = File.OpenRead(filePath);

            BlockMenuDataManager bm = BlockMenuDataManager.Instance;



            StreamReader sr = new StreamReader(file);
            string json = sr.ReadToEnd();

            sr.Close();
            file.Close();

            //bm.menu = JsonUtility.FromJson<BlockMenuData>(json);

            //bm.menu = JsonReader.Deserialize<BlockMenuData>(json);

            bm.menu = JsonUtil.Deserialize<BlockMenuData>(json);
            /*
            bm.menu = new BlockMenuData();
            JSONObject obj = new JSONObject(json);
            bm.menu.parseJsonObject(obj);
            */
        }

        private void SaveFile(string filePath)
        {
            BlockMenuDataManager bm = BlockMenuDataManager.Instance;

            //string json = JsonUtility.ToJson(bm.menu);
            //string json = JsonWriter.Serialize(bm.menu);
            //string json = bm.menu.buildJsonObject().Print();
            string json = JsonUtil.Serialize(bm.menu, true, true);


            FileStream file;

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            file = File.Create(filePath);

            StreamWriter sw = new StreamWriter(file);
            sw.Write(json);


            sw.Close();
            file.Close();

        }
    }


    public class BlockMenuDataManager
    {
        private static BlockMenuDataManager instance = null;
        public static BlockMenuDataManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new BlockMenuDataManager();
                return instance;
            }
        }

        public BlockMenuData menu = null;


    }
}