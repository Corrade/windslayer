using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Windslayer;

namespace Windslayer.Server
{
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

        [Tooltip("Block")]
        public Block BlockPrefab;

        [Tooltip("Dash")]
        public Dash DashPrefab;

        PlayerInputManager m_PlayerInputManager;
        PlayerStatusManager m_PlayerStatusManager;
        PlayerMovementManager m_PlayerMovementManager;

        List<Ability> m_ActiveAbilities = new List<Ability>();

        void Awake()
        {
            m_PlayerInputManager = GetComponent<PlayerInputManager>();
            m_PlayerStatusManager = GetComponent<PlayerStatusManager>();
            m_PlayerMovementManager = GetComponent<PlayerMovementManager>();

            m_PlayerStatusManager.AddStartListener(Status.Silenced, InterruptAllAbilites);
            m_PlayerStatusManager.AddStartListener(Status.Stunned, InterruptAllAbilites);
            m_PlayerStatusManager.AddStartListener(Status.Frozen, InterruptAllAbilites);
        }
        
        void FixedUpdate()
        {
            if (m_PlayerStatusManager.HasAny(Status.Stunned, Status.Silenced)) {
                return;
            }

            if (!m_PlayerInputManager.IsActive(InputIDs.Block)) {
                foreach (Ability a in m_ActiveAbilities) {
                    if (a != null && a is Block) {
                        Block block = (Block)a;
                        block.ToggleOff();
                    }
                }
            }

            if (m_PlayerStatusManager.Has(Status.Casting)) {
                return;
            }

            if (m_PlayerInputManager.IsActive(InputIDs.LightAttack)) {
                Ability instance;
                if (m_PlayerMovementManager.IsGrounded) {
                    instance = Instantiate(LightAttackGroundPrefab, transform);
                } else {
                    instance = Instantiate(LightAttackAirPrefab, transform);
                }

                m_ActiveAbilities.Add(instance);
                instance.Initialise(gameObject);
            }

            if (m_PlayerInputManager.IsActive(InputIDs.StrongAttack)) {
                Ability instance;
                if (m_PlayerMovementManager.IsGrounded) {
                    instance = Instantiate(StrongAttackGroundPrefab, transform);
                } else {
                    instance = Instantiate(StrongAttackAirPrefab, transform);
                }
        
                m_ActiveAbilities.Add(instance);
                instance.Initialise(gameObject);
            }

            if (m_PlayerInputManager.IsActive(InputIDs.Block)) {
                Ability instance = Instantiate(BlockPrefab, transform);
                m_ActiveAbilities.Add(instance);
                instance.Initialise(gameObject);
            }

            if (m_PlayerInputManager.IsActive(InputIDs.Dash)) {
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
