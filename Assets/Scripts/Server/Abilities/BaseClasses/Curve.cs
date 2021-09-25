using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Windslayer;

namespace Windslayer.Server
{
    // Use https://www.desmos.com/calculator to sketch parametric curves

    public class Curve
    {
        public static Curve Upwards = new Curve(
            delegate(int t) { return 0f; },
            delegate(int t) { return t; }
        );

        public static Curve Right = new Curve(
            delegate(int t) { return t; },
            delegate(int t) { return 0f; }
        );

        public static Curve Static = new Curve(
            delegate(int t) { return 0f; },
            delegate(int t) { return 0f; }
        );

        Func<int, float> m_SetX; // f(t) -> x co-ordinate at time t
        Func<int, float> m_SetY; // f(t) -> y co-ordinate at time t

        public Curve(Func<int, float> setX, Func<int, float> setY)
        {
            m_SetX = setX;
            m_SetY = setY;
        }

        public Vector3 GetPoint(int t)
        {
            return new Vector3(m_SetX(t), m_SetY(t), 0f);
        }
    }
}
