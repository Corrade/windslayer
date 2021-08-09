using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : ToggleAbility
{
    protected override void OnActiveBegin() {
        m_PlayerStatusManager.StartStatus(Status.Blocking, 99999);
    }

    protected override void OnActiveEnd() {
        m_PlayerStatusManager.ClearStatus(Status.Blocking);
    }

    protected override void End() {
        m_PlayerStatusManager.ClearStatus(Status.Blocking);
        base.End();
    }
}
