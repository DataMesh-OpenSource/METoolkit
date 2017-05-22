using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using MEHoloClient.Utils;

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
        private string[] names = { "右上", "左上", "左下", "右下" };


        [MenuItem("Window/DataMesh/BlockMenuMaker")]
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

            if (GUILayout.Button("读取菜单配置"))
            {
                if (bm.menu != null && dirty)
                {
                    if (!EditorUtility.DisplayDialog("confirm", "似乎有修改还没有存盘，你是否要放弃修改，并打开新文件？", "打开文件", "取消"))
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

            if (GUILayout.Button("创建新的菜单配置"))
            {
                if (bm.menu != null && dirty)
                {
                    if (!EditorUtility.DisplayDialog("confirm", "似乎有修改还没有存盘，你是否要放弃修改，并创建新配置？", "创建", "取消"))
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

            if (GUILayout.Button("存  盘"))
            {
                if (dirty && filePath != null)
                {
                    SaveFile(filePath);

                    dirty = false;
                }


            }

            if (GUILayout.Button("清  空"))
            {
                if (dirty && filePath != null)
                {
                    if (bm.menu != null && dirty)
                    {
                        if (!EditorUtility.DisplayDialog("confirm", "似乎有修改还没有存盘，你是否放弃所有修改？", "是", "取消"))
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

            bm.menu.name = EditorGUILayout.TextField("面板名称", bm.menu.name);

            if (GUI.changed)
                dirty = true;

            int level = 1;


            if (bm.menu.rootPanel == null)
            {
                if (GUILayout.Button("创建根面板"))
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

            string panelTitle = level == 1 ? "根菜单" : "第" + level + "层菜单";

            if (EditorTools.DrawHeader(panelTitle))
            {

                EditorTools.BeginContents();

                for (int i = 0; i < order.Length; i++)
                {
                    int quadrant = order[i];
                    string name = names[i];

                    if (EditorTools.DrawHeader(panelTitle + " : " + names[quadrant] + "按钮"))
                    {

                        EditorTools.BeginContents();

                        if (IsOppositeQuadrant(quadrant, fromQuadrant))
                        {
                            EditorGUILayout.LabelField("不可用");
                        }
                        else
                        {
                            BlockButtonData button = panel.buttons[quadrant];

                            if (button == null)
                            {
                                if (GUILayout.Button("创建按钮"))
                                {
                                    button = new BlockButtonData();
                                    panel.buttons[quadrant] = button;
                                    dirty = true;
                                }
                            }
                            else
                            {
                                DrawBlockButton(button, quadrant, level);
                                if (GUILayout.Button("删除按钮"))
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
            button.buttonId = EditorGUILayout.TextField("按钮ID", button.buttonId);
            button.buttonName = EditorGUILayout.TextField("按钮名称", button.buttonName);
            button.buttonColor = EditorGUILayout.ColorField("按钮颜色", button.buttonColor);
            button.buttonPic = EditorGUILayout.TextField("使用图片", button.buttonPic);
            button.canClick = EditorGUILayout.Toggle("可否点击", button.canClick);
            if (GUI.changed)
                dirty = true;

            if (button.subPanel == null)
            {
                if (GUILayout.Button("创建子面板"))
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