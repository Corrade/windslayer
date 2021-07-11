using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sync : MonoBehaviour
{
    // frameUpdate() takes in the number of frames that have elapsed since the start of this function. This function will therefore take in ints from 0 to (m_FrameTotal - 1).
    public static IEnumerator RunPerFrame(int frameTotal, Action<int> frameUpdate, Action callback)
    {
        int framesElapsed = 0;

        while (framesElapsed < frameTotal)
        {
            frameUpdate(framesElapsed);
            framesElapsed++;
            yield return null;
        }

        callback();
    }

    public static IEnumerator RunPerFrame(int frameTotal, Action<int> frameUpdate)
    {
        yield return RunPerFrame(frameTotal, frameUpdate, () => {});
    }

    public static IEnumerator RunPerFrame(int frameTotal, Action callback)
    {
        yield return RunPerFrame(frameTotal, (int x) => {}, callback);
    }

    public static IEnumerator RunPerFrame(int frameTotal)
    {
        yield return RunPerFrame(frameTotal, (int x) => {});
    }
}
