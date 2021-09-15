using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Windslayer;

namespace Windslayer.Server
{
    [RequireComponent(typeof(PlayerCombatInputManager)), RequireComponent(typeof(PlayerStatusManager)), RequireComponent(typeof(PlayerMovementManager))]
    public class PlayerAbilityManager : MonoBehaviour
    {
        [Tooltip("Light attack air")]
        [SerializeField]
        LightAttackAir LightAttackAirPrefab;

        [Tooltip("Light attack ground")]
        [SerializeField]
        LightAttackGround LightAttackGroundPrefab;

        [Tooltip("Strong attack air")]
        [SerializeField]
        StrongAttackAir StrongAttackAirPrefab;

        [Tooltip("Strong attack ground")]
        [SerializeField]
        StrongAttackGround StrongAttackGroundPrefab;

        [Tooltip("Block")]
        [SerializeField]
        Block BlockPrefab;

        [Tooltip("Dash")]
        [SerializeField]
        Dash DashPrefab;

        PlayerCombatInputManager m_PlayerCombatInputManager;
        PlayerStatusManager m_PlayerStatusManager;
        PlayerMovementManager m_PlayerMovementManager;

        List<Ability> m_ActiveAbilities = new List<Ability>();

        void Awake()
        {
            m_PlayerCombatInputManager = GetComponent<PlayerCombatInputManager>();
            m_PlayerStatusManager = GetComponent<PlayerStatusManager>();
            m_PlayerMovementManager = GetComponent<PlayerMovementManager>();

            m_PlayerStatusManager.AddStartListener(Status.Silenced, InterruptAllAbilites);
            m_PlayerStatusManager.AddStartListener(Status.Stunned, InterruptAllAbilites);
            m_PlayerStatusManager.AddStartListener(Status.Frozen, InterruptAllAbilites);
        }
        
        void FixedUpdate()
        {
            if (m_PlayerStatusManager.Is(Status.Silenced)) {
                return;
            }

            if (!m_PlayerCombatInputManager.IsActive(CombatInputIDs.Block)) {
                foreach (Ability a in m_ActiveAbilities) {
                    if (a != null && a is Block) {
                        Block block = (Block)a;
                        block.ToggleOff();
                    }
                }
            }

            if (m_PlayerStatusManager.Is(Status.Casting)) {
                return;
            }

            if (m_PlayerCombatInputManager.IsActive(CombatInputIDs.LightAttack)) {
                Ability instance;
                if (m_PlayerMovementManager.IsGrounded) {
                    instance = Instantiate(LightAttackGroundPrefab, transform);
                } else {
                    instance = Instantiate(LightAttackAirPrefab, transform);
                }

                m_ActiveAbilities.Add(instance);
                instance.Initialise(gameObject);
            }

            if (m_PlayerCombatInputManager.IsActive(CombatInputIDs.StrongAttack)) {
                Ability instance;
                if (m_PlayerMovementManager.IsGrounded) {
                    instance = Instantiate(StrongAttackGroundPrefab, transform);
                } else {
                    instance = Instantiate(StrongAttackAirPrefab, transform);
                }
        
                m_ActiveAbilities.Add(instance);
                instance.Initialise(gameObject);
            }

            if (m_PlayerCombatInputManager.IsActive(CombatInputIDs.Block)) {
                Ability instance = Instantiate(BlockPrefab, transform);
                m_ActiveAbilities.Add(instance);
                instance.Initialise(gameObject);
            }

            if (m_PlayerCombatInputManager.IsActive(CombatInputIDs.Dash)) {
                if (m_PlayerMovementManager.IsGrounded) {
                    Ability instance = Instantiate(DashPrefab, transform);
                    m_ActiveAbilities.Add(instance);
                    instance.Initialise(gameObject);
                }
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
}
