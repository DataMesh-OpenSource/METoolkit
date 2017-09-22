using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DataMesh.AR;

[CustomEditor(typeof(MEHoloEntrance))]
public class MEHoloEntranceEditor : Editor
{
    private string newId;

    private void InstantiatePrefabToParent(GameObject prefab, Transform root)
    {
        if (prefab == null)
            return;
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
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WSA
                InstantiatePrefabToParent(entrance.AnchorPrefab, holoRoot.transform);
#endif
                InstantiatePrefabToParent(entrance.InputPrefab, holoRoot.transform);
                InstantiatePrefabToParent(entrance.SpeechPrefab, holoRoot.transform);
                InstantiatePrefabToParent(entrance.UIPerfab, holoRoot.transform);
                InstantiatePrefabToParent(entrance.CollaborationPrefab, holoRoot.transform);
                InstantiatePrefabToParent(entrance.StoragePrefab, holoRoot.transform);
                InstantiatePrefabToParent(entrance.SocialPrefab, holoRoot.transform);
                InstantiatePrefabToParent(entrance.LivePrefab, holoRoot.transform);

            }

            return;
        }

        GUI.changed = false;

        GUIStyle nameStyle = new GUIStyle(GUI.skin.box);
        nameStyle.fontSize = 20;
        nameStyle.padding = new RectOffset(10, 10, 10, 10);
        nameStyle.margin = new RectOffset(10, 10, 10, 10);

        string showName = null;
        bool needInput = false;
        if (string.IsNullOrEmpty(entrance.AppID))
        {
            showName = "Please Set App ID";
            nameStyle.normal.textColor = new Color(0.7f, 0f, 0f);
            needInput = true;
        }
        else
        {
            showName = "App ID: " + entrance.AppID;
            nameStyle.normal.textColor = new Color(0f, 0.7f, 0f);
            needInput = false;
        }

        GUILayout.Box(showName, nameStyle);
        if (needInput)
        {
            newId = EditorGUILayout.TextField("App ID:", newId);
            if (GUILayout.Button("Set App ID"))
            {
                entrance.AppID = newId;
            }
        }
        else
        {
            if (GUILayout.Button("Remove this id"))
            {
                if (EditorUtility.DisplayDialog("Confirm", "Do you want to delete this id?", "Confirm", "Cancel"))
                {
                    entrance.AppID = null;
                }
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WSA
        entrance.NeedAnchor = EditorGUILayout.Toggle("If Need Anchor:", entrance.NeedAnchor);
        if (entrance.NeedAnchor)
        {
            entrance.NeedInput = true;
        }
#endif

        entrance.NeedInput = EditorGUILayout.Toggle("If Need Input:", entrance.NeedInput);
        entrance.NeedUI = EditorGUILayout.Toggle("If Need UI:", entrance.NeedUI);
        entrance.NeedCollaboration = EditorGUILayout.Toggle("If Need Collaboration:", entrance.NeedCollaboration);
        entrance.NeedStorage = EditorGUILayout.Toggle("If Need Storage:", entrance.NeedStorage);
        entrance.NeedSpeech = EditorGUILayout.Toggle("If Need Speech:", entrance.NeedSpeech);
        entrance.NeedSocial = EditorGUILayout.Toggle("If Need Social:", entrance.NeedSocial);

        entrance.NeedLive = EditorGUILayout.Toggle("If Need Live!:", entrance.NeedLive);
        if (entrance.NeedLive)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WSA
            entrance.NeedAnchor = true;
#endif
            entrance.NeedInput = true;
        }

        if (GUI.changed)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            EditorUtility.SetDirty(entrance);
        }

    }

}
