using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : ToggleAbility
{
    // on interrupt/end, also clear

    protected override void OnActiveBegin() {
        m_PlayerStatusManager.StartStatus(Status.Blocking, 999);
    }

    protected override void OnActiveEnd() {
        m_PlayerStatusManager.ClearStatus(Status.Blocking);
    }
}
