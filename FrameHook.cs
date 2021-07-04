using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameHook : MonoBehaviour
{
    int m_FrameTotal;
    Action m_Callback; // Called when the coroutine ends
    Action<int> m_FrameUpdate; // Called on each frame during the coroutine. Takes in the number of frames that have elapsed since the start of this function. This function will therefore take in ints from 0 to (m_FrameTotal - 1)
    int m_FramesElapsed;

    Coroutine m_Coroutine;

    public FrameHook(int frameTotal, Action callback, Action<int> frameUpdate)
    {
        m_FrameTotal = frameTotal;
        m_Callback = callback;
        m_FrameUpdate = frameUpdate;
    }

    public FrameHook(int frameTotal, Action callback)
    {
        m_FrameTotal = frameTotal;
        m_Callback = callback;
        m_FrameUpdate = (int x) => {};
    }

    public FrameHook(int frameTotal, Action<int> frameUpdate)
    {
        m_FrameTotal = frameTotal;
        m_Callback = () => {};
        m_FrameUpdate = frameUpdate;
    }

    // Starts the coroutine that was constructed
    public void Begin()
    {
        m_Coroutine = StartCoroutine(Run());
    }

    // Ends the coroutine without calling m_Callback
    public void Interrupt()
    {
        StopCoroutine(m_Coroutine);
    }

    IEnumerator Run()
    {
        m_FramesElapsed = 0;

        while (m_FramesElapsed < m_FrameTotal)
        {
            m_FrameUpdate(m_FramesElapsed);
            m_FramesElapsed++;
            yield return null;
        }

        m_Callback();
    }
}
