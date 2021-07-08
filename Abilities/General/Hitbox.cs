using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class Hitbox : MonoBehaviour
{
    // two approaches: inherit from this and override IsTarget and Hit. OR always use this and pass in IsTarget and Hit as lambdas.

    // public UnityEvent<GameObject> OnHitConfirmed; // Called once if at least one enemy is hit
    public Curve Path;
    public float Speed;
    public float MaxDuration;
    public int MaxPierce;

    Collider2D m_Hitbox;
    // GameObject m_Attacker;
    GameObject m_Parent;
    Func<GameObject, bool> m_IsTarget; // f(collided game object) -> whether or not a hit should be registers, e.g. a team check
    float m_TimeElapsed;

    void Start()
    {
        m_Hitbox = GetComponent<Collider2D>();
    }

    void Initialise()
    {
        m_TimeElapsed = 0f;
        // set m_Attacker
        // set m_Parent
        // set m_IsTarget
    }

    void FixedUpdate()
    {
        RaycastHit2D[] results = new RaycastHit2D[]{};
        int targetsHit = 0;

        m_Hitbox.Cast(Vector2.zero, results, 0f, true);

        foreach (RaycastHit2D hit in results) {
            if (hit.collider == null) { // || !m_IsTarget(hit)
                continue;
            }

            targetsHit++;
            if (targetsHit > MaxPierce) {
                break;
            }

            // Hit(x);
        }

        if (targetsHit > 0) {
            // OnHitConfirmed?.Invoke(this, EventArgs.Empty);
            Destroy(gameObject);
        }

        // transform.position = m_Parent.transform.position + Speed * Path(m_TimeElapsed);

        m_TimeElapsed += Time.fixedDeltaTime;
        if (m_TimeElapsed >= MaxDuration) {
            Destroy(gameObject);
        }
    }

    // Consider: damage, hit stun, status effects, healing
    protected virtual void Hit(GameObject player) {}
}
