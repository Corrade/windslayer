using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Windslayer;

namespace Windslayer.Server
{
    public class Dash : Ability
    {
        [Tooltip("The force of the dash")]
        public int Force;

        protected override void OnActiveBegin()
        {
            m_PlayerMovementManager.SetOverrideVelocity(delegate(
                Vector2 candidateVelocity,
                bool isGrounded,
                bool isFacingLeft,
                Vector2 groundNormal
            ) {
                
                float magnitude = isFacingLeft ? -Force : Force;
                return magnitude * PlayerMovementManager.VectorAlongSurface(groundNormal);
            }, ActiveDuration);
        }
    }
}
