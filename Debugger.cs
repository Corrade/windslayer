using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Debugger
{
    public static void Log(object message) {
        Debug.Log(message);
    }

    public static void DrawRay(Vector3 start, Vector3 dir, Color? color = null, float duration = 0.0f, bool depthTest = true)
    {
        Debug.DrawRay(start, dir, color == null ? Color.white : (Color)color, duration, depthTest);
    }

    public static void DrawLine(Vector3 start, Vector3 end, Color? color = null, float duration = 0.0f, bool depthTest = true)
    {
        Debug.DrawLine(start, end, color == null ? Color.white : (Color)color, duration, depthTest);
    }

    public static string Vector2Full(Vector2 vec)
    {
        return "(" + vec.x + ", " + vec.y + ")";
    }
}
