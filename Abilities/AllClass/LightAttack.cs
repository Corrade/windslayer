using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightAttack : Attack
{
    [Tooltip("The hitbox")]
    public Hitbox LightAttackHitbox;

    [Tooltip("Damage")]
    public int Damage;

    [Tooltip("Hit stun duration")]
    public int HitStun;

    protected override void OnActiveBegin()
    {
        Hitbox hitboxInstance = Instantiate(LightAttackHitbox, transform.position, transform.rotation);
        m_HitboxesToDestroyOnInterrupt.Add(hitboxInstance);
        hitboxInstance.Initialise(this, m_Player, m_Player.transform, Curve.Static, 0f, ActiveDuration, IsTarget, Hit);
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
            stat.TakeDamage(Damage, m_Player, true, HitStun);
        }
    }
}
