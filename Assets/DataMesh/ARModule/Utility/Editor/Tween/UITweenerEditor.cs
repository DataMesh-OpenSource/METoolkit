using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DataMesh.AR.Utility
{

    [CustomEditor(typeof(UITweener), true)]
    public class UITweenerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            EditorTools.SetLabelWidth(110f);
            base.OnInspectorGUI();
            DrawCommonProperties();
        }

        protected void DrawCommonProperties()
        {
            UITweener tw = target as UITweener;

            if (EditorTools.DrawHeader("Tweener"))
            {
                EditorTools.BeginContents();
                EditorTools.SetLabelWidth(110f);

                GUI.changed = false;

                UITweener.Style style = (UITweener.Style)EditorGUILayout.EnumPopup("Play Style", tw.style);

                int loopCount = EditorGUILayout.IntField("Loop Count", tw.loopCount);

                AnimationCurve curve = EditorGUILayout.CurveField("Animation Curve", tw.animationCurve, GUILayout.Width(170f), GUILayout.Height(62f));

                int additionalCurveCount = tw.GetAdditionalCurveCount();
                for (int i = 0;i < additionalCurveCount;i ++)
                {
                    AnimationCurve oldCurve = null;
                    if (tw.additionalAnimationCurveList.Count <= i)
                    {
                        oldCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));
                        tw.additionalAnimationCurveList.Add(oldCurve);
                    }
                    else
                    {
                        oldCurve = tw.additionalAnimationCurveList[i];
                    }
                    AnimationCurve newCurve = EditorGUILayout.CurveField("Animation Curve", oldCurve, GUILayout.Width(170f), GUILayout.Height(62f));
                    tw.additionalAnimationCurveList[i] = newCurve;
                }

                UITweener.Method method = (UITweener.Method)EditorGUILayout.EnumPopup("Play Method", tw.method);

                float startFactor = EditorGUILayout.Slider("Start Factor", tw.startFactor, 0, 1);
                float startFactorReverse = EditorGUILayout.Slider("End Factor", tw.startFactorReverse, 0, 1);

                float factor = EditorGUILayout.Slider("current Factor", tw.tweenFactor, 0, 1);

                GUILayout.BeginHorizontal();
                float dur = EditorGUILayout.FloatField("Duration", tw.duration, GUILayout.Width(170f));
                GUILayout.Label("seconds");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                float del = EditorGUILayout.FloatField("Start Delay", tw.delay, GUILayout.Width(170f));
                GUILayout.Label("seconds");
                GUILayout.EndHorizontal();

                int tg = EditorGUILayout.IntField("Tween Group", tw.tweenGroup, GUILayout.Width(170f));
                bool ts = EditorGUILayout.Toggle("Ignore TimeScale", tw.ignoreTimeScale);

                if (GUI.changed)
                {
                    tw.animationCurve = curve;
                    tw.method = method;
                    tw.tweenFactor = factor;
                    tw.startFactor = startFactor;
                    tw.startFactorReverse = startFactorReverse;
                    tw.style = style;
                    tw.loopCount = loopCount;
                    tw.ignoreTimeScale = ts;
                    tw.tweenGroup = tg;
                    tw.duration = dur;
                    tw.delay = del;

                    EditorTools.SetDirty(tw);
                }
                EditorTools.EndContents();
            }

            EditorTools.SetLabelWidth(80f);
            //EditorTools.DrawEvents("On Finished", tw, tw.onFinished);
        }
    }

}