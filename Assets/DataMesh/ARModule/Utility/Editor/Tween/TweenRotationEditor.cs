//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;

namespace DataMesh.AR.Utility
{

    [CustomEditor(typeof(TweenRotation))]
    public class TweenRotationEditor : UITweenerEditor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            EditorTools.SetLabelWidth(120f);

            TweenRotation tw = target as TweenRotation;
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