using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A basic attack that is only cast in the air
public abstract class BasicAirAttack : BasicAttack
{
    [Tooltip("Attacker stun duration in frames")]
    public int AttackerStun;

    protected override void Hit(GameObject other)
    {
        base.Hit(other);

        m_PlayerStatusManager.StartStatus(Status.Stunned, AttackerStun);
    }
}
