using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthManager : MonoBehaviour
{
    [Tooltip("Maximum amount of health")]
    public float MaxHealth = 10f;

    public Action<float, GameObject> OnDamaged;
    public Action<float> OnHealed;
    public Action OnDie;

    public float CurrentHealth { get; set; }
    public bool IsInvincible { get; set; }

    bool m_IsDead;

    void Start()
    {
        CurrentHealth = MaxHealth;
        m_IsDead = false;
    }

    public void TakeDamage(float damage, GameObject damageSource)
    {
        if (IsInvincible) {
            return;
        }

        float healthBefore = CurrentHealth;
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

        float trueDamageAmount = healthBefore - CurrentHealth;
        if (trueDamageAmount > 0f) {
            OnDamaged?.Invoke(trueDamageAmount, damageSource);
        }

        HandleDeath();
    }

    public void TakeHealing(float healing, GameObject healSource)
    {
        TakeDamage(-healing, healSource);
    }

    public void Kill()
    {
        CurrentHealth = 0f;
        OnDamaged?.Invoke(MaxHealth, null);
        HandleDeath();
    }

    void HandleDeath()
    {
        if (m_IsDead) {
            return;
        }

        if (CurrentHealth <= 0f) {
            m_IsDead = true;
            OnDie?.Invoke();
        }
    }
}
