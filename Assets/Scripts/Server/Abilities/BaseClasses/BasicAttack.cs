using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Windslayer;

namespace Windslayer.Server
{
    // An attack with a single hitbox that does damage on contact
    public abstract class BasicAttack : Attack
    {
        [Tooltip("The hitbox")]
        public Hitbox ThisHitbox;

        [Tooltip("The initial position of the hitbox from which its path is added onto")]
        public Vector3 InitPos;

        [Tooltip("Damage")]
        public int Damage;

        [Tooltip("Hit stun duration in frames")]
        public int HitStun;

        protected virtual bool IsTarget(GameObject other)
        {
            PlayerStatManager stat = other.GetComponent<PlayerStatManager>();
            if (stat) {
                return m_PlayerStatManager.Team != stat.Team;
            }

            return false;
        }

        protected virtual void Hit(GameObject other)
        {
            PlayerStatManager stat = other.GetComponent<PlayerStatManager>();
            if (stat) {
                stat.TakeDamage(Damage, m_Player, true);
            }

            PlayerStatusManager status = other.GetComponent<PlayerStatusManager>();
            if (status) {
                if (status.Has(Status.Blocking)) {
                    status.StartStatus(Status.Stunned, HitStun / 2);
                    status.StartStatus(Status.Invincible, HitStun / 2);
                } else {
                    status.StartStatus(Status.Stunned, HitStun);
                    status.StartStatus(Status.Invincible, HitStun);
                }
            }
        }
    }
}
