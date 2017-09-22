using UnityEngine;
using UnityEditor;

namespace DataMesh.AR.Utility
{

    [CustomEditor(typeof(TweenMaterialFloat))]
    public class TweenMaterialFloatEditor : UITweenerEditor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            EditorTools.SetLabelWidth(120f);

            TweenMaterialFloat tw = target as TweenMaterialFloat;
            GUI.changed = false;

            string property = EditorGUILayout.TextField("Property", tw.propertyName);

            float from = EditorGUILayout.FloatField("From", tw.from);
            float to = EditorGUILayout.FloatField("To", tw.to);

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