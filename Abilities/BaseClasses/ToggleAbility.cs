using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ToggleAbility : Ability
{
    bool m_ToggleOn = true;

    public void ToggleOff() {
        m_ToggleOn = false;
    }

    protected override IEnumerator Run()
    {
        OnStartUpBegin();
        if (StartUpDuration > 0) {
            yield return Sync.Delay(StartUpDuration, StartUpUpdate);
        }
        OnStartUpEnd();

        OnActiveBegin();
        while (m_ToggleOn) {
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

    protected override void OnStartUpBegin() {
        m_PlayerStatusManager.StartStatus(Status.Casting, 99999);
    }

    protected override void End() {
        m_PlayerStatusManager.ClearStatus(Status.Casting);
        base.End();
    }
}
