using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Windslayer;

namespace Windslayer.Server
{
    // Re-applications overwrite: if an ability inflicting a status S for X frames is applied to a player who already has S with Y remaining frames, the player is set to have S for X frames (even if X is less than Y)

    // Compound statuses are independent: if an ability inflicting a status S1 for X frames is applied to a player who has a status S' = S1 | S2 | ..., the player is set to have S1 for X frames and S' remains as usual (as opposed to S' being overwritten just because one of its statuses is overwritten). Furthermore, compounded statuses must be implemented independently - they are not automatically implemented if their parts are implemented.

    public enum Status
    {
        // A rooted player cannot influence their own movement from regular keypresses nor abilities, but is still subject to gravity and others' forces. Additionally, if a player is rooted in the air, their previous air strafe input (if any) is maintained, which gives the impression that their velocity is preserved.
        Rooted = 0b0000_0000_0000_0001,

        // A suspended player cannot move or be made to move. An airborne player exiting suspension will continue along their previous velocity (i.e. suspension preserves velocity)
        Suspended = 0b0000_0000_0000_0010,

        // A silenced player cannot use any abilities
        Silenced = 0b0000_0000_0000_0100,

        // A disarmed player cannot light attack nor strong attack
        Disarmed = 0b0000_0000_0000_1000,

        // A confused player has their left and right movement keybinds swapped
        Confused = 0b0000_0000_0001_0000,

        // An invincible player cannot be damaged (even by DoT effects applied before the invincibility), but may be targeted (hit, healed and knocked back etc.)
        Invincible = 0b0000_0000_0010_0000,

        // An intangible player cannot be targeted but may still take damage from effects applied before the intangibility
        Intangible = 0b0000_0000_0100_0000,

        // An invisible player has their player hidden from others but still produces dust clouds and ability effects etc.
        Invisible = 0b0000_0000_1000_0000,

        // A blocking player mitigates damage
        Blocking = 0b0000_0001_0000_0000,

        // Suspended + silenced + disarmed
        Stunned = Suspended | Silenced | Disarmed,

        // Stunned + intangible
        Frozen = Stunned | Intangible,

        // Rooted + silenced + disarmed
        // This exists as it is slightly different from stun. Even if it weren't PlayerAbilityManager interrupts all ongoing abilities when the player is stunned regularly, but abilities inflict their own stun, e.g. during startup and recovery, and of course should not interrupt themselves
        Casting = Rooted | Silenced | Disarmed,

        // Casting + invincible + intangible
        Dead = Casting | Invincible | Intangible,
    }

    public class PlayerStatusManager : MonoBehaviour
    {
        class StatusInfo
        {
            public event EventHandler OnStart;
            public event EventHandler OnEnd;
            int m_TicksRemaining;

            public StatusInfo()
            {
                m_TicksRemaining = 0;
            }

            public void Tick()
            {
                if (m_TicksRemaining > 0) {
                    m_TicksRemaining--;

                    if (m_TicksRemaining <= 0) {
                        ClearStatus();
                    }
                }
            }

            public void StartStatus(int ticks)
            {
                m_TicksRemaining = ticks;
                OnStart?.Invoke(this, EventArgs.Empty);
            }

            public void ClearStatus()
            {
                m_TicksRemaining = 0;
                OnEnd?.Invoke(this, EventArgs.Empty);
            }

            public bool Is()
            {
                return m_TicksRemaining > 0;
            }
        }

        Dictionary<Status, StatusInfo> m_Statuses = new Dictionary<Status, StatusInfo>();

        void Awake()
        {
            foreach (Status status in Enum.GetValues(typeof(Status))) {
                m_Statuses[status] = new StatusInfo();
            }
        }

        void FixedUpdate()
        {
            // Consider script execution order
            foreach (KeyValuePair<Status, StatusInfo> entry in m_Statuses) {
                entry.Value.Tick();
            }
        }

        // Statuses will clear by themselves
        public void StartStatus(Status status, int ticks)
        {
            if (ticks > 0) {
                m_Statuses[status].StartStatus(ticks);
            }
        }

        public void ClearStatus(Status status)
        {
            m_Statuses[status].ClearStatus();
        }

        // Returns true if the player has a status with the effect of the given status
        public bool Is(Status status)
        {
            foreach (Status s in m_Statuses.Keys) {
                if (((s & status) != 0) && m_Statuses[s].Is()) {
                    return true;
                }
            }
            
            return false;
        }

        // Returns true only if the player has the exact status specified
        public bool IsExact(Status status)
        {
            return m_Statuses[status].Is();
        }

        /*
        public bool HasAny(params Status[] list)
        {
            foreach (Status status in list) {
                if (m_Statuses[status].Is()) {
                    return true;
                }
            }

            return false;
        }
        */

        public bool HasAll(params Status[] list)
        {
            foreach (Status status in list) {
                if (!m_Statuses[status].Is()) {
                    return false;
                }
            }

            return true;
        }

        public void AddStartListener(Status status, EventHandler handler)
        {
            m_Statuses[status].OnStart += handler;
        }

        public void AddEndListener(Status status, EventHandler handler)
        {
            m_Statuses[status].OnEnd += handler;
        }

        public void RemoveStartListener(Status status, EventHandler handler)
        {
            m_Statuses[status].OnStart -= handler;
        }

        public void RemoveEndListener(Status status, EventHandler handler)
        {
            m_Statuses[status].OnEnd -= handler;
        }
    }
}
