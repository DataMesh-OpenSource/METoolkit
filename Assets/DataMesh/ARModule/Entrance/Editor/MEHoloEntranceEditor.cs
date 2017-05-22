using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DataMesh.AR;

[CustomEditor(typeof(MEHoloEntrance))]
public class MEHoloEntranceEditor : Editor
{
    private void InstantiatePrefabToParent(GameObject prefab, Transform root)
    {
        GameObject obj;
        obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        obj.transform.SetParent(root);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
    }

    public override void OnInspectorGUI()
    {

        MEHoloEntrance entrance = (MEHoloEntrance)target;

        GameObject holoRoot = GameObject.Find("MEHolo");
        if (holoRoot == null)
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (GUILayout.Button("Create All MEHolo Module"))
            {
                holoRoot = new GameObject("MEHolo");

                InstantiatePrefabToParent(entrance.CommonPrefab, holoRoot.transform);
                InstantiatePrefabToParent(entrance.AnchorPrefab, holoRoot.transform);
                InstantiatePrefabToParent(entrance.InputPrefab, holoRoot.transform);
                InstantiatePrefabToParent(entrance.SpeechPrefab, holoRoot.transform);
                InstantiatePrefabToParent(entrance.MenuPrefab, holoRoot.transform);
                InstantiatePrefabToParent(entrance.CursorPrefab, holoRoot.transform);
                InstantiatePrefabToParent(entrance.CollaborationPrefab, holoRoot.transform);
                InstantiatePrefabToParent(entrance.SocialPrefab, holoRoot.transform);
                InstantiatePrefabToParent(entrance.LivePrefab, holoRoot.transform);

            }

            return;
        }

        GUI.changed = false;


        entrance.NeedAnchor = EditorGUILayout.Toggle("If Need Anchor:", entrance.NeedAnchor);
        if (entrance.NeedAnchor)
        {
            entrance.NeedInput = true;
        }
        entrance.NeedInput = EditorGUILayout.Toggle("If Need Input:", entrance.NeedInput);
        entrance.NeedCursor = EditorGUILayout.Toggle("If Need Cursor:", entrance.NeedCursor);
        entrance.NeedCollaboration = EditorGUILayout.Toggle("If Need Collaboration:", entrance.NeedCollaboration);
        entrance.NeedMenu = EditorGUILayout.Toggle("If Need Menu:", entrance.NeedMenu);
        entrance.NeedSpeech = EditorGUILayout.Toggle("If Need Speech:", entrance.NeedSpeech);
        entrance.NeedSocial = EditorGUILayout.Toggle("If Need Social:", entrance.NeedSocial);

        entrance.NeedLive = EditorGUILayout.Toggle("If Need Live!:", entrance.NeedLive);
        if (entrance.NeedLive)
        {
            entrance.NeedAnchor = true;
            entrance.NeedInput = true;
        }

        if (GUI.changed)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

    }

}
