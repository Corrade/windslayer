using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Use https://www.desmos.com/calculator to sketch parametric curves

public class Curve : MonoBehaviour
{
    Func<float, float> m_SetX; // f(t) -> x co-ordinate at time t
    Func<float, float> m_SetY; // f(t) -> y co-ordinate at time t

    public Curve(Func<float, float> setX, Func<float, float> setY)
    {
        m_SetX = setX;
        m_SetY = setY;
    }

    public Vector2 GetPoint(float t)
    {
        return new Vector2(m_SetX(t), m_SetY(t));
    }

    // Returns an upwards line (0, t)
    public static Curve Upwards(float t)
    {
        return new Curve(
            delegate(float t) { return 0f; },
            delegate(float t) { return t; }
        );
    }

    // Returns a right-going line (t, 0)
    public static Curve Right(float t)
    {
        return new Curve(
            delegate(float t) { return t; },
            delegate(float t) { return 0f; }
        );
    }

    // Returns a static point at (0, 0)
    public static Curve Static(float t)
    {
        return new Curve(
            delegate(float t) { return 0f; },
            delegate(float t) { return 0f; }
        );
    }
}
