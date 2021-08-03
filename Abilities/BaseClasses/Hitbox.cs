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
    GameObject m_Player;
    Transform m_PositionParent;

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
    public void Initialise(
        Attack attack,
        GameObject player,
        Transform positionParent,
        Curve path,
        float speed,
        int lifetime,
        Func<GameObject, bool> isTarget,
        Action<GameObject> hit
    ) {
        m_Attack = attack;
        m_Player = player;
        m_PositionParent = positionParent;

        m_Path = path;
        m_Speed = speed;
        m_Lifetime = lifetime;

        m_IsTarget = isTarget;
        m_Hit = hit;

        m_FramesElapsed = 0;
        m_LifetimeCoroutine = StartCoroutine(Sync.Delay(m_Lifetime, () => { End(); }));
    }

    void Update()
    {
        m_FramesElapsed++;
    }

    void FixedUpdate()
    {
        List<RaycastHit2D> results = new List<RaycastHit2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();

        m_Hitbox.Cast(Vector2.up, filter, results, 0.1f, true);

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

        transform.position = m_PositionParent.position + m_Speed * (Vector3)m_Path.GetPoint(m_FramesElapsed);
    }

    public void Interrupt() {
        StopCoroutine(m_LifetimeCoroutine);
        End();
    }

    void End() {
        Destroyed = true;
        Destroy(gameObject);
    }
}
