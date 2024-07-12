using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    public static void DrawSquare(Vector3 pos, Vector2 size, Color color, float duration)
    {
        Vector3 halfSize3 = new Vector3(size.x, size.y, 0) / 2;
        Debug.DrawLine(pos - halfSize3, pos + new Vector3(halfSize3.x, -halfSize3.y, 0), color, duration);
        Debug.DrawLine(pos + new Vector3(halfSize3.x, -halfSize3.y, 0), pos + halfSize3, color, duration);
        Debug.DrawLine(pos + halfSize3, pos + new Vector3(-halfSize3.x, halfSize3.y, 0), color, duration);
        Debug.DrawLine(pos + new Vector3(-halfSize3.x, halfSize3.y, 0), pos - halfSize3, color, duration);
    }
}
