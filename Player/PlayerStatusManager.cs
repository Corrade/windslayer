using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum Status
{
    None,

    // A rooted player cannot influence their own movement from regular keypresses nor abilities, but is still subject to gravity and others' forces
    Rooted,

    // A suspended player cannot move or be made to move. An airborne player exiting suspension will drop vertically, even if their velocity was heading elsewhere prior to the suspension (i.e. suspension resets velocity)
    Suspended,

    // A silenced player cannot use any abilities
    Silenced,

    // A silenced player cannot light attack nor strong attack
    Disarmed,

    // A confused player has their left and right movement keybinds swapped
    Confused,

    // An invincible player cannot be damaged (even by DoT effects applied before the invincibility), but may be healed and knocked back etc.
    Invincible,

    // An intangible player cannot be targeted but may still take damage from effects applied before the intangibility
    Intangible,

    // An invisible player has their player hidden from others but still produces dust clouds and ability effects etc.
    Invisible,

    // Suspended + silenced + disarmed
    Stunned,

    // Stunned + intangible
    Frozen,
}

public class PlayerStatusManager : MonoBehaviour
{
    // Re-applications overwrite: if an ability inflicting a status S for X frames is applied to a player who already has S with Y remaining frames, the player is set to have S for X more frames

    // Compound statuses are independent from their parts: if an ability inflicting a status S1 for X frames is applied to a player who has a status S' = S1 | S2 | ..., the player is set to have S1 for X frames and S' remains as usual (as opposed to S' being overwritten just because one of its statuses is overwritten)

    public class StatusInfo
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

        public bool HasStatus()
        {
            return m_FramesRemaining > 0;
        }

        public int GetRemainingFrames()
        {
            return m_FramesRemaining;
        }
    }

    Dictionary<Status, StatusInfo> m_Statuses;

    void Start()
    {
        m_Statuses = new Dictionary<Status, StatusInfo>();

        foreach (Status status in Enum.GetValues(typeof(Status))) {
            m_Statuses[status] = new StatusInfo();
        }
    }

    void Update()
    {
        // Consider script execution order
        foreach (KeyValuePair<Status, StatusInfo> entry in m_Statuses) {
            entry.Value.Tick();
        }
    }

    // Statuses will clear by themselves
    public void StartStatus(Status status, int frames)
    {
        m_Statuses[status].StartStatus(frames);
    }

    public void ClearStatus(Status status)
    {
        m_Statuses[status].ClearStatus();
    }

    public bool HasStatus(Status status)
    {
        return m_Statuses[status].HasStatus();
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
