using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInputManager)), RequireComponent(typeof(PlayerStatusManager))]
public class PlayerAbilityManager : MonoBehaviour
{
    [Tooltip("Light attack")]
    public LightAttack LightAttackPrefab;

    PlayerInputManager m_PlayerInputManager;
    PlayerStatusManager m_PlayerStatusManager;

    List<Ability> m_ActiveAbilities;

    void Start()
    {
        m_PlayerInputManager = GetComponent<PlayerInputManager>();
        m_PlayerStatusManager = GetComponent<PlayerStatusManager>();
        m_ActiveAbilities = new List<Ability>();

        m_PlayerStatusManager.AddStartListener(Status.Silenced, InterruptAllAbilites);
        m_PlayerStatusManager.AddStartListener(Status.Stunned, InterruptAllAbilites);
        m_PlayerStatusManager.AddStartListener(Status.Frozen, InterruptAllAbilites);
    }
    
    void Update()
    {
        if (m_PlayerStatusManager.Has(Status.Casting)) {
            return;
        }

        if (m_PlayerInputManager.GetInputDown("light_attack", true)) {
            Ability instance = Instantiate(LightAttackPrefab, transform.position, transform.rotation);
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
