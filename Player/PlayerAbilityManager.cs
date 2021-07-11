using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInputManager))]
public class PlayerAbilityManager : MonoBehaviour
{
    PlayerInputManager m_PlayerInputManager;

    void Start()
    {
        m_PlayerInputManager = GetComponent<PlayerInputManager>();
    }
    
    void Update()
    {
        if (m_PlayerInputManager.GetInputDown("light_attack", true)) {
            ;
            // StartCoroutine(Sync.RunPerFrame(60, (int x) => { Debug.Log(x); }));
        }
    }
}
