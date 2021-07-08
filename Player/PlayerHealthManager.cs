using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerStatManager)), RequireComponent(typeof(PlayerStatusManager))]
public class PlayerHealthManager : MonoBehaviour
{
    [Tooltip("Percentage of damage mitigated by block")]
    public float BlockModifier = 0.5f;

    public float Health { get; private set; }
    public float Mana { get; private set; }

    bool m_IsDead;
    PlayerStatManager m_PlayerStatManager;
    PlayerStatusManager m_PlayerStatusManager;

    void Start()
    {
        m_PlayerStatManager = GetComponent<PlayerStatManager>();
        m_PlayerStatusManager = GetComponent<PlayerStatusManager>();

        Health = m_PlayerStatManager.MaxHealth;
        Mana = m_PlayerStatManager.MaxMana;
        m_IsDead = false;
    }

    public void TakeDamage(float damage, GameObject damageSource, bool affectedByBlock, int hitStun)
    {
        float healthBefore = Health;
        Health -= DamageFormula(damage, affectedByBlock);
        Health = Mathf.Clamp(Health, 0f, m_PlayerStatManager.MaxHealth);

        float trueDamageAmount = healthBefore - Health;
        if (trueDamageAmount > 0f) {
            if (hitStun > 0) {
                m_PlayerStatusManager.StartStatus(Status.Suspended, hitStun);
            }
        }

        HandleDeath();
    }

    float DamageFormula(float rawDamage, bool affectedByBlock)
    {
        float finalDamage = rawDamage;

        if (affectedByBlock && false) { // AND is blocking
            finalDamage *= BlockModifier;
        }

        return finalDamage;
    }

    public void TakeHealing(float healing, GameObject healSource)
    {
        TakeDamage(-healing, healSource, false, 0);
    }

    public void Kill()
    {
        TakeDamage(m_PlayerStatManager.MaxHealth, null, false, 0);
    }

    void HandleDeath()
    {
        if (m_IsDead) {
            return;
        }

        if (Health <= 0f) {
            m_IsDead = true;
        }
    }
}
