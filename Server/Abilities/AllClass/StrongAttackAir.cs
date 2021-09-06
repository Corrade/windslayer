using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Windslayer;

namespace Windslayer.Server
{
    public class StrongAttackAir : BasicAirAttack
    {
        protected override void OnActiveBegin()
        {
            Hitbox hitboxInstance = Instantiate(ThisHitbox, transform);
            m_HitboxesToDestroyOnInterrupt.Add(hitboxInstance);
            hitboxInstance.Initialise(this, InitPos, 0f,  Curve.Static, ActiveDuration, IsTarget, Hit);
        }
    }
}
