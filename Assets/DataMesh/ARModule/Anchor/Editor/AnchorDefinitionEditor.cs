using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnchorDefinition))]
public class AnchorDefinitionEditor : Editor
{
    private AnchorDefinition define;
    void OnEnable()
    {
        define = (AnchorDefinition)target;
    }

    void OnSceneGUI()
    {
        if (Application.isPlaying)
            return;

        Handles.color = new Color(108f / 255f, 222f / 255f, 1f);
        Vector3 v = define.transform.localPosition;

        Transform trans = define.transform;

        Vector3 v1 = trans.TransformPoint(new Vector3(define.xMin, define.yMin, define.zMin));
        Vector3 v2 = trans.TransformPoint(new Vector3(define.xMax, define.yMin, define.zMin));
        Vector3 v3 = trans.TransformPoint(new Vector3(define.xMin, define.yMax, define.zMin));
        Vector3 v4 = trans.TransformPoint(new Vector3(define.xMax, define.yMax, define.zMin));
        Vector3 v5 = trans.TransformPoint(new Vector3(define.xMin, define.yMin, define.zMax));
        Vector3 v6 = trans.TransformPoint(new Vector3(define.xMax, define.yMin, define.zMax));
        Vector3 v7 = trans.TransformPoint(new Vector3(define.xMin, define.yMax, define.zMax));
        Vector3 v8 = trans.TransformPoint(new Vector3(define.xMax, define.yMax, define.zMax));
        Handles.DrawLine(v1, v2);
        Handles.DrawLine(v2, v4);
        Handles.DrawLine(v4, v3);
        Handles.DrawLine(v3, v1);

        Handles.DrawLine(v5, v6);
        Handles.DrawLine(v6, v8);
        Handles.DrawLine(v8, v7);
        Handles.DrawLine(v7, v5);

        Handles.DrawLine(v1, v5);
        Handles.DrawLine(v2, v6);
        Handles.DrawLine(v3, v7);
        Handles.DrawLine(v4, v8);

        float centerX = define.xMin + (define.xMax - define.xMin) / 2;
        float centerY = define.yMin + (define.yMax - define.yMin) / 2;
        float centerZ = define.zMin + (define.zMax - define.zMin) / 2;

        Vector3 xMaxVector = Handles.FreeMoveHandle(trans.TransformPoint(new Vector3(define.xMax, centerY, centerZ)), trans.rotation, 0.1f, Vector3.zero, Handles.SphereHandleCap);
        xMaxVector = trans.InverseTransformPoint(xMaxVector);
        define.xMax = xMaxVector.x;

        Vector3 xMinVector = Handles.FreeMoveHandle(trans.TransformPoint(new Vector3(define.xMin, centerY, centerZ)), trans.rotation, 0.1f, Vector3.zero, Handles.SphereHandleCap);
        xMinVector = trans.InverseTransformPoint(xMinVector);
        define.xMin = xMinVector.x;

        Vector3 yMaxVector = Handles.FreeMoveHandle(trans.TransformPoint(new Vector3(centerX, define.yMax, centerZ)), trans.rotation, 0.1f, Vector3.zero, Handles.SphereHandleCap);
        yMaxVector = trans.InverseTransformPoint(yMaxVector);
        define.yMax = yMaxVector.y;

        Vector3 yMinVector = Handles.FreeMoveHandle(trans.TransformPoint(new Vector3(centerX, define.yMin, centerZ)), trans.rotation, 0.1f, Vector3.zero, Handles.SphereHandleCap);
        yMinVector = trans.InverseTransformPoint(yMinVector);
        define.yMin = yMinVector.y;

        Vector3 zMaxVector = Handles.FreeMoveHandle(trans.TransformPoint(new Vector3(centerX, centerY, define.zMax)), trans.rotation, 0.1f, Vector3.zero, Handles.SphereHandleCap);
        zMaxVector = trans.InverseTransformPoint(zMaxVector);
        define.zMax = zMaxVector.z;

        Vector3 zMinVector = Handles.FreeMoveHandle(trans.TransformPoint(new Vector3(centerX, centerY, define.zMin)), trans.rotation, 0.1f, Vector3.zero, Handles.SphereHandleCap);
        zMinVector = trans.InverseTransformPoint(zMinVector);
        define.zMin = zMinVector.z;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        //Debug.Log("---" + worldMountPt);
    }
}
