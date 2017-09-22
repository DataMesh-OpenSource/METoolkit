using UnityEngine;
using UnityEditor;

namespace DataMesh.AR.Utility
{

    [CustomEditor(typeof(TweenMaterialColor))]
    public class TweenMaterialColorEditor : UITweenerEditor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            EditorTools.SetLabelWidth(120f);

            TweenMaterialColor tw = target as TweenMaterialColor;
            GUI.changed = false;

            string property = EditorGUILayout.TextField("Property", tw.propertyName);

            Color from = EditorGUILayout.ColorField("From", tw.from);
            Color to = EditorGUILayout.ColorField("To", tw.to);

            if (GUI.changed)
            {
                tw.propertyName = property;
                tw.from = from;
                tw.to = to;
                EditorTools.SetDirty(tw);
            }

            DrawCommonProperties();
        }
    }
}