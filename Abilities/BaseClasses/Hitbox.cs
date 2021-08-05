using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D)), RequireComponent(typeof(Rigidbody2D))]
public class Hitbox : MonoBehaviour
{
    public bool Destroyed { get; private set; }

    Collider2D m_Hitbox;
    Coroutine m_LifetimeCoroutine;
    int m_FramesElapsed;

    Attack m_Attack;
    Transform m_PositionParent;
    PlayerMovementManager m_ParentPlayerMovement;

    Vector3 m_PathStart;
    Curve m_Path;
    float m_Speed;
    int m_Lifetime;

    Func<GameObject, bool> m_IsTarget; // f(collided game object) -> whether or not the object should be hit, i.e. most often this will check the team of the hit player
    Action<GameObject> m_Hit; // f(hit game object)

    void Start()
    {
        m_Hitbox = GetComponent<Collider2D>();
        Destroyed = false;
    }

    // These variables are passed in from the calling attack. Regarding the delegates, an alternative implementation could define them in the interface of the attack and call them through a reference, but that would not support attacks that need multiple implementations of the same delegate, e.g. to create multiple hitboxes with different behaviours.
    // Hitboxes are responsible for destroying themselves as they may outlast the ability that created them. The ability caller may still destroy them early by called Interrupt().
    public void Initialise(
        Attack attack,
        Transform positionParent,
        Vector3 pathStart,
        Curve path,
        float speed,
        int lifetime,
        Func<GameObject, bool> isTarget,
        Action<GameObject> hit
    ) {
        m_Attack = attack;
        m_PositionParent = positionParent; // instead, just instantiate as a child of the appropriate parent
        m_ParentPlayerMovement = positionParent.GetComponent<PlayerMovementManager>(); // May return null

        m_PathStart = pathStart;
        m_Path = path;
        m_Speed = speed;
        m_Lifetime = lifetime;

        m_IsTarget = isTarget;
        m_Hit = hit;

        m_FramesElapsed = 0;
        m_LifetimeCoroutine = StartCoroutine(Sync.Delay(m_Lifetime, () => { End(); }));

        SetPosition();
        m_Hitbox.gameObject.SetActive(true);
    }

    public void Interrupt() {
        StopCoroutine(m_LifetimeCoroutine);
        End();
    }

    void Update()
    {
        m_FramesElapsed++;
    }

    void FixedUpdate()
    {
        CheckForHits();
        SetPosition();
    }

    void CheckForHits()
    {
        List<RaycastHit2D> results = new List<RaycastHit2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();

        m_Hitbox.Cast(Vector2.up, filter, results, 0.01f, true);

        foreach (RaycastHit2D hit in results) {
            if (hit.collider == null || !m_IsTarget(hit.collider.gameObject)) {
                continue;
            }

            m_Hit(hit.collider.gameObject);

            m_Attack.Pierced++;
            if (m_Attack.Pierced >= m_Attack.MaxPierce) {
                break;
            }
        }

        if (m_Attack.Pierced == m_Attack.MaxPierce) {
            Interrupt();
        }
    }

    void SetPosition()
    {
        transform.position = m_PositionParent.position;
        Vector3 offset = m_PathStart + m_Speed * m_Path.GetPoint(m_FramesElapsed);

        // If the position parent is a player, the hitbox should be facing in the same direction of the player. Hitboxes face right by default, so we flip the hitbox if the player is facing left.
        if (m_ParentPlayerMovement && m_ParentPlayerMovement.IsFacingLeft) {
            transform.position = m_PositionParent.position - offset;
        } else {
            transform.position = m_PositionParent.position + offset;
        }
    }

    void End() {
        Destroyed = true;
        Destroy(gameObject);
    }
}
