using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathfTool {

    /// <summary>
    /// 计算三角形的面积,参数分别是三个点的xy坐标
    /// </summary>
    /// <returns></returns>
    public static float TriangleArea(float v0x, float v0y, float v1x, float v1y, float v2x, float v2y)
    {
        return Mathf.Abs((v0x * v1y + v1x * v2y + v2x * v0y
          - v1x * v0y - v2x * v1y - v0x * v2y) / 2f);
    }


}
