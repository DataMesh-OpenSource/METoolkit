//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;

namespace DataMesh.AR.Utility
{

    [CustomEditor(typeof(TweenPosition))]
    public class TweenPositionEditor : UITweenerEditor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            EditorTools.SetLabelWidth(120f);

            TweenPosition tw = target as TweenPosition;
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