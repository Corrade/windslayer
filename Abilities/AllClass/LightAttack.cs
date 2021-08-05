using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightAttack : Attack
{
    [Tooltip("The hitbox")]
    public Hitbox LightAttackHitbox;

    [Tooltip("The offset of the hitbox")]
    public Vector3 PathStart;

    [Tooltip("Damage")]
    public int Damage;

    [Tooltip("Hit stun duration in frames")]
    public int HitStun;

    [Tooltip("Attacker stun duration in frames")]
    public int AttackerStun;

    protected override void OnActiveBegin()
    {
        Hitbox hitboxInstance = Instantiate(LightAttackHitbox, transform.position, transform.rotation);
        m_HitboxesToDestroyOnInterrupt.Add(hitboxInstance);
        hitboxInstance.Initialise(this, m_Player.transform, PathStart, Curve.Static, 0f, ActiveDuration, IsTarget, Hit);
    }

    bool IsTarget(GameObject otherPlayer)
    {
        PlayerStatManager stat = otherPlayer.GetComponent<PlayerStatManager>();
        if (stat) {
            return m_PlayerStatManager.Team != stat.Team;
        }

        return false;
    }

    void Hit(GameObject otherPlayer)
    {
        PlayerStatManager stat = otherPlayer.GetComponent<PlayerStatManager>();
        if (stat) {
            stat.TakeDamage(Damage, m_Player, true);
        }

        PlayerStatusManager status = otherPlayer.GetComponent<PlayerStatusManager>();
        if (status) {
            if (false) { // IsBlocking()
                status.StartStatus(Status.Stunned, HitStun / 2);
                status.StartStatus(Status.Invincible, HitStun);
            } else {
                status.StartStatus(Status.Stunned, HitStun);
                status.StartStatus(Status.Invincible, HitStun);
            }
        }

        if (!m_PlayerMovementManager.IsGrounded) {
            m_PlayerStatusManager.StartStatus(Status.Stunned, AttackerStun);
        }
    }
}
