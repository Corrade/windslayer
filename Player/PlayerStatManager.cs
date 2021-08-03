using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerStatManager)), RequireComponent(typeof(PlayerStatusManager))]
public class PlayerStatManager : MonoBehaviour
{
    [Tooltip("Percentage of damage mitigated by block")]
    public float BlockModifier = 0.5f;

    public float Health { get; private set; }
    public float Mana { get; private set; }
    public int Team { get; private set; }
    public int Kills { get; private set; }
    public int Deaths { get; private set; }
    public bool IsDead { get; private set; }

    public int MaxHealth {
        get {
            return m_MaxHealth;
        }
        set {
            m_MaxHealth = value;
            OnMaxHealthChange?.Invoke(this, EventArgs.Empty);
        }
    }

    public int MaxMana {
        get {
            return m_MaxMana;
        }
        set {
            m_MaxMana = value;
            OnMaxManaChange?.Invoke(this, EventArgs.Empty);
        }
    }

    public int Power {
        get {
            return m_Power;
        }
        set {
            m_Power = value;
            OnPowerChange?.Invoke(this, EventArgs.Empty);
        }
    }

    public int Defence {
        get {
            return m_Defence;
        }
        set {
            m_Defence = value;
            OnDefenceChange?.Invoke(this, EventArgs.Empty);
        }
    }

    // Events
    public event EventHandler OnMaxHealthChange;
    public event EventHandler OnMaxManaChange;
    public event EventHandler OnPowerChange;
    public event EventHandler OnDefenceChange;

    // Backing fields
    int m_MaxHealth;
    int m_MaxMana;
    int m_Power;
    int m_Defence;

    PlayerStatusManager m_PlayerStatusManager;

    void Start()
    {
        m_PlayerStatusManager = GetComponent<PlayerStatusManager>();

        m_MaxHealth = 100;
        m_MaxMana = 100;
        m_Power = 10;
        m_Defence = 10;
    
        Health = MaxHealth;
        Mana = MaxMana;
        Team = 0;
        Kills = 0;
        Deaths = 0;
        IsDead = false;
    }

    public void TakeDamage(float damage, GameObject damageSource, bool affectedByBlock, int hitStun)
    {
        if (!m_PlayerStatusManager.Has(Status.Invincible)) {
            float healthBefore = Health;
            Health -= DamageFormula(damage, affectedByBlock);
            Health = Mathf.Clamp(Health, 0f, MaxHealth);
            float trueDamageAmount = healthBefore - Health;
        }

        if (hitStun > 0) {
            m_PlayerStatusManager.StartStatus(Status.Stunned, hitStun);
            m_PlayerStatusManager.StartStatus(Status.Invincible, hitStun);
        } // if affectedByBlock && IsBlocking() then do block stun instead

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
        TakeDamage(MaxHealth, null, false, 0);
    }

    void HandleDeath()
    {
        if (IsDead) {
            return;
        }

        if (Health <= 0f) {
            IsDead = true;
        }
    }
}
