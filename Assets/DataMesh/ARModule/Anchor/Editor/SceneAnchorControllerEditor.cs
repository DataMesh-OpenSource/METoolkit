using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DataMesh.AR.Anchor;

[CustomEditor(typeof(SceneAnchorController))]
public class SceneAnchorControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SceneAnchorController controller = target as SceneAnchorController;

        /*
        if (EditorTools.DrawHeader("Anchor Setting"))
        {
            EditorTools.BeginContents();

            int num = EditorGUILayout.IntField("Numbers of Anchor:", controller.anchorNameList.Count);
            for (int i = controller.anchorNameList.Count - 1; i < num - 1; i++)
            {
                controller.anchorNameList.Add(null);
            }

            for (int i = controller.anchorRootList.Count - 1; i < num - 1; i++)
            {
                controller.anchorRootList.Add(null);
            }

            for (int i = controller.anchorNameList.Count - 1; i >= num; i--)
            {
                controller.anchorNameList.RemoveAt(i);
            }

            for (int i = controller.anchorRootList.Count - 1; i >= num; i--)
            {
                controller.anchorRootList.RemoveAt(i);
            }

            for (int i = 0; i < num; i++)
            {
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label("Anchor " + (i + 1) + ":", GUILayout.Width(120));
                controller.anchorNameList[i] = EditorGUILayout.TextField(controller.anchorNameList[i]);
                controller.anchorRootList[i] = (GameObject)EditorGUILayout.ObjectField(controller.anchorRootList[i], typeof(GameObject), true);

                EditorGUILayout.EndHorizontal();
                if (GUI.changed)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                    UnityEditor.EditorUtility.SetDirty(controller);
                }
            }


            EditorTools.EndContents();
        }
        */

        if (GUI.changed)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            UnityEditor.EditorUtility.SetDirty(controller);
        }

    }
}
