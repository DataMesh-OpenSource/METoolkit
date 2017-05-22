//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TweenAlpha))]
public class TweenAlphaEditor : UITweenerEditor
{
	public override void OnInspectorGUI ()
	{
		GUILayout.Space(6f);
		EditorTools.SetLabelWidth(120f);

		TweenAlpha tw = target as TweenAlpha;
		GUI.changed = false;

		float from = EditorGUILayout.Slider("From", tw.from, 0f, 1f);
		float to = EditorGUILayout.Slider("To", tw.to, 0f, 1f);

		if (GUI.changed)
		{
			tw.from = from;
			tw.to = to;
            EditorTools.SetDirty(tw);
		}

		DrawCommonProperties();
	}
}
