using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Windslayer;

namespace Windslayer.Server
{
    // Re-applications overwrite: if an ability inflicting a status S for X frames is applied to a player who already has S with Y remaining frames, the player is set to have S for X frames (even if X is less than Y)

    // Compound statuses are independent: if an ability inflicting a status S1 for X frames is applied to a player who has a status S' = S1 | S2 | ..., the player is set to have S1 for X frames and S' remains as usual (as opposed to S' being overwritten just because one of its statuses is overwritten). Furthermore, compounded statuses must be implemented independently - they are not automatically implemented if their parts are implemented.

    [Flags]
    public enum Status
    {
        // A rooted player cannot influence their own movement from regular keypresses nor abilities, but is still subject to gravity and others' forces. Additionally, if a player is rooted in the air, their previous air strafe input (if any) is maintained, which gives the impression that their velocity is preserved.
        Rooted,

        // A suspended player cannot move or be made to move. An airborne player exiting suspension will continue along their previous velocity (i.e. suspension preserves velocity)
        Suspended,

        // A silenced player cannot use any abilities
        Silenced,

        // A disarmed player cannot light attack nor strong attack
        Disarmed,

        // A confused player has their left and right movement keybinds swapped
        Confused,

        // An invincible player cannot be damaged (even by DoT effects applied before the invincibility), but may be targeted (hit, healed and knocked back etc.)
        Invincible,

        // An intangible player cannot be targeted but may still take damage from effects applied before the intangibility
        Intangible,

        // An invisible player has their player hidden from others but still produces dust clouds and ability effects etc.
        Invisible,

        // A blocking player mitigates damage
        Blocking,

        // Suspended + silenced + disarmed
        Stunned,

        // Stunned + intangible
        Frozen,

        // Rooted + silenced + disarmed. (if this were just stunned): This exists as PlayerAbilityManager interrupts all ongoing abilities when the player is stunned regularly, but abilities inflict their own stun, e.g. during startup and recovery, and of course should not interrupt themselves
        Casting,
    }

    public class PlayerStatusManager : MonoBehaviour
    {
        class StatusInfo
        {
            public event EventHandler OnStart;
            public event EventHandler OnEnd;
            int m_FramesRemaining;

            public StatusInfo()
            {
                m_FramesRemaining = 0;
            }

            public void Tick()
            {
                if (m_FramesRemaining > 0) {
                    m_FramesRemaining--;

                    if (m_FramesRemaining <= 0) {
                        ClearStatus();
                    }
                }
            }

            public void StartStatus(int frames)
            {
                m_FramesRemaining = frames;
                OnStart?.Invoke(this, EventArgs.Empty);
            }

            public void ClearStatus()
            {
                m_FramesRemaining = 0;
                OnEnd?.Invoke(this, EventArgs.Empty);
            }

            public bool Has()
            {
                return m_FramesRemaining > 0;
            }

            public int GetRemainingFrames()
            {
                return m_FramesRemaining;
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
        public void StartStatus(Status status, int frames)
        {
            if (frames > 0) {
                m_Statuses[status].StartStatus(frames);
            }
        }

        public void ClearStatus(Status status)
        {
            m_Statuses[status].ClearStatus();
        }

        public bool Has(Status status)
        {
            return m_Statuses[status].Has();
        }

        public bool HasAny(params Status[] list)
        {
            foreach (Status status in list) {
                if (m_Statuses[status].Has()) {
                    return true;
                }
            }

            return false;
        }

        public bool HasAll(params Status[] list)
        {
            foreach (Status status in list) {
                if (!m_Statuses[status].Has()) {
                    return false;
                }
            }

            return true;
        }

        public int GetRemainingFrames(Status status)
        {
            return m_Statuses[status].GetRemainingFrames();
        }

        public void AddStartListener(Status status, EventHandler handler)
        {
            m_Statuses[status].OnStart += handler;
        }

        public void AddEndListener(Status status, EventHandler handler)
        {
            m_Statuses[status].OnEnd += handler;
        }
    }
}
