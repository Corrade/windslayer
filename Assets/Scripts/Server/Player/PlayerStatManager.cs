using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Windslayer;

namespace Windslayer.Server
{
    [RequireComponent(typeof(PlayerStatusManager))]
    public class PlayerStatManager : MonoBehaviour
    {
        [Tooltip("Percentage of damage mitigated by block")]
        public float BlockModifier = 0.5f;

        [Tooltip("Team number")]
        public int Team = 0;

        public int MaxHealth { get; private set; } = 100;
        public int MaxMana { get; private set; } = 100;
        public int Power { get; private set; } = 10;
        public int Defence { get; private set; } = 10;

        public float Health { get; private set; }
        public float Mana { get; private set; }
        public int Kills { get; private set; } = 0;
        public int Deaths { get; private set; } = 0;
        public bool IsDead { get; private set; } = false;

        PlayerStatusManager m_PlayerStatusManager;

        void Awake()
        {
            m_PlayerStatusManager = GetComponent<PlayerStatusManager>();

            Health = MaxHealth;
            Mana = MaxMana;
        }

        public void TakeDamage(float damage, GameObject damageSource, bool affectedByBlock)
        {
            if (!m_PlayerStatusManager.Has(Status.Invincible)) {
                // float healthBefore = Health;
                Health -= DamageFormula(damage, affectedByBlock);
                Health = Mathf.Clamp(Health, 0f, MaxHealth);
                // float trueDamageAmount = healthBefore - Health;
            }

            HandleDeath(damageSource);
        }

        float DamageFormula(float rawDamage, bool affectedByBlock)
        {
            float finalDamage = rawDamage;

            if (affectedByBlock && m_PlayerStatusManager.Has(Status.Blocking)) {
                finalDamage *= BlockModifier;
            }

            return finalDamage;
        }

        public void TakeHealing(float healing, GameObject healSource)
        {
            TakeDamage(-healing, healSource, false);
        }

        public void Kill()
        {
            TakeDamage(MaxHealth, null, false);
        }

        void HandleDeath(GameObject damageSource)
        {
            if (IsDead) {
                return;
            }

            if (Health <= 0f) {
                IsDead = true;
                Deaths++;

                PlayerStatManager stat = damageSource.GetComponent<PlayerStatManager>();
                if (stat && stat != m_PlayerStatusManager) {
                    stat.Kills++;
                }
            }
        }
    }
}
