#define ENABLE_LOGS

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Debugger
{
    const bool enableDebugging = true;

    public static void Log(object message) {
        if (!enableDebugging) {
            return;
        }

        Debug.Log(message);
    }

    public static void DrawRay(Vector3 start, Vector3 dir, Color? color = null, float duration = 0.0f, bool depthTest = true)
    {
        if (!enableDebugging) {
            return;
        }

        Debug.DrawRay(start, dir, color == null ? (Color)color : Color.white, duration, depthTest);
    }

    public static void DrawLine(Vector3 start, Vector3 end, Color? color = null, float duration = 0.0f, bool depthTest = true)
    {
        if (!enableDebugging) {
            return;
        }

        Debug.DrawLine(start, end, color == null ? (Color)color : Color.white, duration, depthTest);
    }
}
