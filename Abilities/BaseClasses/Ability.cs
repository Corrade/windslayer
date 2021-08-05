using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    [Tooltip("Frame duration of the startup phase")]
    public int StartUpDuration;

    [Tooltip("Frame duration of the active phase")]
    public int ActiveDuration;

    [Tooltip("Frame duration of the recovery phase")]
    public int RecoveryDuration;

    protected GameObject m_Player;
    protected PlayerStatusManager m_PlayerStatusManager;
    protected PlayerMovementManager m_PlayerMovementManager;
    protected PlayerStatManager m_PlayerStatManager;
    Coroutine m_Coroutine;

    public virtual void Initialise(GameObject player)
    {
        m_Player = player;
        m_PlayerStatusManager = player.GetComponent<PlayerStatusManager>();
        m_PlayerMovementManager = player.GetComponent<PlayerMovementManager>();
        m_PlayerStatManager = player.GetComponent<PlayerStatManager>();
        m_Coroutine = StartCoroutine(Run());
    }

    public virtual void Interrupt() {
        StopCoroutine(m_Coroutine);
        End();
    }

    void End() {
        m_PlayerStatusManager.ClearStatus(Status.Casting);
        Destroy(gameObject);
    }

    IEnumerator Run()
    {
        OnStartUpBegin();
        if (StartUpDuration > 0) {
            yield return Sync.Delay(StartUpDuration, StartUpUpdate);
        }
        OnStartUpEnd();

        OnActiveBegin();
        if (ActiveDuration > 0) {
            yield return Sync.Delay(ActiveDuration, ActiveUpdate);
        }
        OnActiveEnd();

        OnRecoveryBegin();
        if (RecoveryDuration > 0) {
            yield return Sync.Delay(RecoveryDuration, RecoveryUpdate);
        }
        OnRecoveryEnd();

        End();
    }

    protected virtual void OnStartUpBegin() {
        m_PlayerStatusManager.StartStatus(Status.Casting, StartUpDuration + ActiveDuration + RecoveryDuration);
    }
    protected virtual void StartUpUpdate(int frameNum) {}
    protected virtual void OnStartUpEnd() {}

    protected virtual void OnActiveBegin() {}
    protected virtual void ActiveUpdate(int frameNum) {}
    protected virtual void OnActiveEnd() {}

    protected virtual void OnRecoveryBegin() {}
    protected virtual void RecoveryUpdate(int frameNum) {}
    protected virtual void OnRecoveryEnd() {}
}
