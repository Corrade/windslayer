using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovementManager)), RequireComponent(typeof(SpriteRenderer))]
public class PlayerSpriteManager : MonoBehaviour
{
    PlayerMovementManager m_PlayerMovementManager;
    SpriteRenderer m_SpriteRenderer;

    void Start()
    {
        m_PlayerMovementManager = GetComponent<PlayerMovementManager>();
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (m_PlayerMovementManager.IsFacingLeft()) {
            m_SpriteRenderer.flipX = true;
        } else if (m_PlayerMovementManager.IsFacingRight()) {
            m_SpriteRenderer.flipX = false;
        }
    }
}
