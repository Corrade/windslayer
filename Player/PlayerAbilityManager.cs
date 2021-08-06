using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInputManager)), RequireComponent(typeof(PlayerStatusManager)), RequireComponent(typeof(PlayerMovementManager))]
public class PlayerAbilityManager : MonoBehaviour
{
    [Tooltip("Light attack air")]
    public LightAttackAir LightAttackAirPrefab;

    [Tooltip("Light attack ground")]
    public LightAttackGround LightAttackGroundPrefab;

    [Tooltip("Strong attack air")]
    public StrongAttackAir StrongAttackAirPrefab;

    [Tooltip("Strong attack ground")]
    public StrongAttackGround StrongAttackGroundPrefab;


    PlayerInputManager m_PlayerInputManager;
    PlayerStatusManager m_PlayerStatusManager;
    PlayerMovementManager m_PlayerMovementManager;

    List<Ability> m_ActiveAbilities;

    void Start()
    {
        m_PlayerInputManager = GetComponent<PlayerInputManager>();
        m_PlayerStatusManager = GetComponent<PlayerStatusManager>();
        m_PlayerMovementManager = GetComponent<PlayerMovementManager>();
        m_ActiveAbilities = new List<Ability>();

        m_PlayerStatusManager.AddStartListener(Status.Silenced, InterruptAllAbilites);
        m_PlayerStatusManager.AddStartListener(Status.Stunned, InterruptAllAbilites);
        m_PlayerStatusManager.AddStartListener(Status.Frozen, InterruptAllAbilites);
    }
    
    void Update()
    {
        if (m_PlayerStatusManager.HasAny(Status.Casting, Status.Stunned, Status.Silenced)) {
            return;
        }

        if (m_PlayerInputManager.GetInputDown("light_attack", true)) {
            Ability instance;
            if (m_PlayerMovementManager.IsGrounded) {
                instance = Instantiate(LightAttackGroundPrefab, transform);
            } else {
                instance = Instantiate(LightAttackAirPrefab, transform);
            }

            m_ActiveAbilities.Add(instance);
            instance.Initialise(gameObject);
        }

        if (m_PlayerInputManager.GetInputDown("strong_attack", true)) {
            Ability instance;
            if (m_PlayerMovementManager.IsGrounded) {
                instance = Instantiate(StrongAttackGroundPrefab, transform);
            } else {
                instance = Instantiate(StrongAttackAirPrefab, transform);
            }
       
            m_ActiveAbilities.Add(instance);
            instance.Initialise(gameObject);
        }
    }

    void InterruptAllAbilites(object sender, EventArgs e)
    {
        foreach (Ability a in m_ActiveAbilities) {
            if (a != null) {
                a.Interrupt();
            }
        }
    }
}
