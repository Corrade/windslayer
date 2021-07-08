using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{
    public int StartUpDuration; // In frames
    public int ActiveDuration;
    public int RecoveryDuration;

    Coroutine m_Coroutine;

    public void Begin()
    {
        m_Coroutine = StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        OnStartUpBegin();
        if (StartUpDuration > 0) {
            yield return Sync.RunPerFrame(StartUpDuration, StartUpUpdate);
        }
        OnStartUpEnd();

        OnActiveBegin();
        if (ActiveDuration > 0) {
            yield return Sync.RunPerFrame(ActiveDuration, ActiveUpdate);
        }
        OnActiveEnd();

        OnRecoveryBegin();
        if (RecoveryDuration > 0) {
            yield return Sync.RunPerFrame(RecoveryDuration, RecoveryUpdate);
        }
        OnRecoveryEnd();
    }

    public virtual void OnInterrupt() {
        StopCoroutine(m_Coroutine);
    }

    protected virtual void OnStartUpBegin() {}
    protected virtual void StartUpUpdate(int frameNum) {}
    protected virtual void OnStartUpEnd() {}

    protected virtual void OnActiveBegin() {}
    protected virtual void ActiveUpdate(int frameNum) {}
    protected virtual void OnActiveEnd() {}

    protected virtual void OnRecoveryBegin() {}
    protected virtual void RecoveryUpdate(int frameNum) {}
    protected virtual void OnRecoveryEnd() {}
}
