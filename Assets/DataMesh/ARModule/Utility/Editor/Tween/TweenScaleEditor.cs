using UnityEngine;
using UnityEditor;

namespace DataMesh.AR.Utility
{

    [CustomEditor(typeof(TweenScale))]
    public class TweenScaleEditor : UITweenerEditor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            EditorTools.SetLabelWidth(120f);

            TweenScale tw = target as TweenScale;
            GUI.changed = false;

            Vector3 from = EditorGUILayout.Vector3Field("From", tw.from);
            Vector3 to = EditorGUILayout.Vector3Field("To", tw.to);

            bool useAdditional = EditorGUILayout.Toggle("Use Additional Curve", tw.UseAdditionalCurve);

            if (GUI.changed)
            {
                tw.from = from;
                tw.to = to;
                tw.UseAdditionalCurve = useAdditional;
                EditorTools.SetDirty(tw);
            }

            DrawCommonProperties();
        }
    }

}