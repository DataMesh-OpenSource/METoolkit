using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace DataMesh.AR.Anchor
{

    [CustomEditor(typeof(VuforiaExtend))]
    public class VuforiaExtendEditor : Editor
    {
#if ME_VUFORIA_ACTIVE
        private ReorderableList ImageTargetList;
        VuforiaExtend entrance;
        readonly GUIContent bGUI = new GUIContent(">", "Go to ImageTarget");

        GameObject holoRoot;
        private void OnEnable()
        {
            entrance = (VuforiaExtend)target;

            holoRoot = GameObject.Find("VuforiaObjects");
            ImageTargetList = new ReorderableList(serializedObject,
                serializedObject.FindProperty("ImageTargets"),
                true, true, true, true);

            ImageTargetList.drawElementCallback = DrawNameElement;
            ImageTargetList.drawHeaderCallback = (Rect rect) =>
            {
                GUI.Label(rect, "ImageTargets");
            };
            ImageTargetList.onRemoveCallback = (ReorderableList list) =>
            {
                if (EditorUtility.DisplayDialog("警告", "是否真的要删除？", "是", "否"))
                {
                    //ReorderableList.defaultBehaviours.DoRemoveButton(list);
                    if (list.serializedProperty != null)
                    {

                        GameObject ImageTarget = (GameObject)list.serializedProperty.GetArrayElementAtIndex(list.index).objectReferenceValue;
                        entrance.ImageTargets.Remove(ImageTarget);
                        DestroyImmediate(ImageTarget);
                    }

                }
            };
            ImageTargetList.onAddCallback = (ReorderableList list) =>
            {
                GameObject ImageTarget = InstantiatePrefabToParentReturn(entrance.ImageTargetPrefab, holoRoot.transform);
                DataMesh.AR.Anchor.VuforiaFollow vuforiaFollow= ImageTarget.AddComponent<DataMesh.AR.Anchor.VuforiaFollow>();
                vuforiaFollow.ImageTarget = ImageTarget;
                //vuforiaFollow.FollowObject=
                entrance.ImageTargets.Add(ImageTarget);
                //if (list.serializedProperty != null)
                //{
                //    list.serializedProperty.arraySize++;
                //    list.index = list.serializedProperty.arraySize - 1;

                //    SerializedProperty itemData = list.serializedProperty.GetArrayElementAtIndex(list.index);
                //}
                //else
                //{
                //    ReorderableList.defaultBehaviours.DoAddButton(list);
                //}
            };
        }
        private void DrawNameElement(Rect rect, int index, bool selected, bool focused)
        {
            SerializedProperty itemData = ImageTargetList.serializedProperty.GetArrayElementAtIndex(index);

            //rect.y += 2;
            //rect.height = EditorGUIUtility.singleLineHeight;
            //EditorGUI.PropertyField(rect, itemData, GUIContent.none);
            Rect lRect = new Rect(rect.x, rect.y + 2f, rect.width - 20f, EditorGUIUtility.singleLineHeight);
            Rect bRect = new Rect(rect.x + lRect.width, rect.y + 2f, 18f, EditorGUIUtility.singleLineHeight);

            if (itemData.objectReferenceValue != null)
            {
                EditorGUI.LabelField(lRect, itemData.objectReferenceValue.name);
         

                if (GUI.Button(bRect, bGUI))
                {
                    Selection.activeObject = itemData.objectReferenceValue;
                }
            }
            else
            {
                EditorGUI.LabelField(lRect, "Missing ImageTarget");
                //ReorderableList.defaultBehaviours.DoRemoveButton(ImageTargetList);
            }
        }

        public override void OnInspectorGUI()
        {
            //if (Application.isPlaying) return;
            //base.OnInspectorGUI();
            //VuforiaExtend entrance = (VuforiaExtend)target;

            //GameObject holoRoot = GameObject.Find("VuforiaObjects");

            entrance = (VuforiaExtend)target;

            holoRoot = GameObject.Find("VuforiaObjects");
            if (entrance.ARCameraPrefab == null)
            {
                entrance.ARCameraPrefab = AssetDatabase.LoadAssetAtPath("Assets/Vuforia/Prefabs/ARCamera.prefab", typeof(GameObject)) as GameObject;
            }
            if (entrance.ImageTargetPrefab == null)
            {
                entrance.ImageTargetPrefab = AssetDatabase.LoadAssetAtPath("Assets/Vuforia/Prefabs/ImageTarget.prefab", typeof(GameObject)) as GameObject;
            }
            if (entrance.ARCameraPrefab == null)
            {
                entrance.VuforiaIsExist = false;
            }
            else
            {
                entrance.VuforiaIsExist = true;
            }
            
            if (entrance.VuforiaIsExist && holoRoot == null)
            {
                holoRoot = new GameObject("VuforiaObjects");
               
            }
            GameObject VuforiaManager = GameObject.Find("VuforiaManager");
            if (VuforiaManager)
            {
                holoRoot.transform.parent = VuforiaManager.transform;
            }
       
            
            if (!entrance.VuforiaIsExist)
            {
                DestroyImmediate(holoRoot);
                return;
            }
            entrance.ARCameraPrefab = (GameObject)EditorGUILayout.ObjectField("ARCameraPrefab:", entrance.ARCameraPrefab, typeof(GameObject));
            entrance.ImageTargetPrefab = (GameObject)EditorGUILayout.ObjectField("ImageTargetPrefab:", entrance.ImageTargetPrefab, typeof(GameObject));
            entrance.VuforiaKey = EditorGUILayout.TextField("Vuforia Key:", entrance.VuforiaKey);

            GameObject ARCamera = GameObject.Find(entrance.ARCameraPrefab.name);
            if (entrance.VuforiaIsExist && holoRoot != null && ARCamera == null)
            {

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                //if (GUILayout.Button("Add Extend"))
                //{
                //}
                ARCamera = InstantiatePrefabToParentReturn(entrance.ARCameraPrefab, holoRoot.transform);

                return;
            }

            if (ARCamera != null)
            {

                Vuforia.VuforiaBehaviour.Instance.SetAppLicenseKey(entrance.VuforiaKey);
                //Vuforia.VuforiaBehaviour.Instance.SetWorldCenterMode(Vuforia.VuforiaAbstractBehaviour.WorldCenterMode.FIRST_TARGET);

                Vuforia.DigitalEyewearBehaviour.Instance.SetEyewearType(Vuforia.DigitalEyewearAbstractBehaviour.EyewearType.OpticalSeeThrough);
                Vuforia.DigitalEyewearBehaviour.Instance.SetSeeThroughConfiguration(Vuforia.DigitalEyewearAbstractBehaviour.SeeThroughConfiguration.HoloLens);
                if (Camera.main.transform != null)
                {
                    //Vuforia.DigitalEyewearBehaviour.Instance.SetCentralAnchorPoint(Camera.main.transform);
                }
                if (GameObject.Find("Fade")==null) {
                    Vuforia.DigitalEyewearBehaviour.Instance.SetCentralAnchorPoint(new GameObject("Fade").transform);
                }
            }
            Vuforia.ImageTargetBehaviour[] imageTargetBehaviours = GameObject.FindObjectsOfType<Vuforia.ImageTargetBehaviour>();
            for (int i = 0; i < imageTargetBehaviours.Length; i++)
            {
                if (!entrance.ImageTargets.Contains(imageTargetBehaviours[i].gameObject)) entrance.ImageTargets.Add(imageTargetBehaviours[i].gameObject);
            }
            //GUI.changed = false;


            //if (GUI.changed)
            //{
            //    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            //}
            //if (GUILayout.Button("Add ImageTarget"))
            //{
            //    GameObject ImageTarget = InstantiatePrefabToParentReturn(entrance.ImageTargetPrefab, holoRoot.transform);
            //    entrance.ImageTargets.Add(ImageTarget);
            //}

            serializedObject.Update();
            ImageTargetList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
        private void InstantiatePrefabToParent(GameObject prefab, Transform root)
        {
            GameObject obj;
            obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            obj.transform.SetParent(root);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
        }
        private GameObject InstantiatePrefabToParentReturn(GameObject prefab, Transform root)
        {
            GameObject obj;
            obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            obj.transform.SetParent(root);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            return obj;
        }
#endif
    }
}


