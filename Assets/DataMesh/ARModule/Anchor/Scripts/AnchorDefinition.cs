using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorDefinition : MonoBehaviour
{
    public enum LockType
    {
        None,
        Lock,
        FollowCoordinate
    }

    public string anchorName;

    public float xMin = -1;
    public float xMax = 1;
    public float yMin = -1;
    public float yMax = 1;
    public float zMin = -1;
    public float zMax = 1;

    public LockType lockX = LockType.None;
    public LockType lockY = LockType.None;
    public LockType lockZ = LockType.None;

     
    /*
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(108f / 255f, 222f / 255f, 1f);
        Vector3 v = transform.position;
        float xx = Size.x / 2;
        float yy = Size.y / 2;
        float zz = Size.z / 2;

        Vector3 v1 = new Vector3(v.x + xx, v.y + yy, v.z + zz) + Center;
        Vector3 v2 = new Vector3(v.x - xx, v.y + yy, v.z + zz) + Center;
        Vector3 v3 = new Vector3(v.x + xx, v.y - yy, v.z + zz) + Center;
        Vector3 v4 = new Vector3(v.x - xx, v.y - yy, v.z + zz) + Center;
        Vector3 v5 = new Vector3(v.x + xx, v.y + yy, v.z - zz) + Center;
        Vector3 v6 = new Vector3(v.x - xx, v.y + yy, v.z - zz) + Center;
        Vector3 v7 = new Vector3(v.x + xx, v.y - yy, v.z - zz) + Center;
        Vector3 v8 = new Vector3(v.x - xx, v.y - yy, v.z - zz) + Center;
        Gizmos.DrawLine(v1, v2);
        Gizmos.DrawLine(v2, v4);
        Gizmos.DrawLine(v4, v3);
        Gizmos.DrawLine(v3, v1);

        Gizmos.DrawLine(v5, v6);
        Gizmos.DrawLine(v6, v8);
        Gizmos.DrawLine(v8, v7);
        Gizmos.DrawLine(v7, v5);

        Gizmos.DrawLine(v1, v5);
        Gizmos.DrawLine(v2, v6);
        Gizmos.DrawLine(v3, v7);
        Gizmos.DrawLine(v4, v8);

        Vector3 worldMountPt = transform.TransformPoint(Center);     // 转成世界坐标系
        float sizeFactor = HandleUtility.GetHandleSize(worldMountPt) * 0.25f;            // 这样就不会随着scene面板的远近而动态改变大小，一直不变。
        Handles.color = Color.magenta;                                     // 设置颜色
        worldMountPt = Handles.FreeMoveHandle(worldMountPt, Quaternion.identity, sizeFactor * 0.2f, Vector3.zero, Handles.RectangleCap);     // 拖动handle来改变值

        Debug.Log("---" + worldMountPt);

    }
    */
}