using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    /*
    Collider2D col;
    Transform m_Attacker;
    Transform m_Parent;
    Action<Transform> m_HitCallback; // Action<EntityHit>
    // ? targeting; // attacker, teammates, enemies, etc.

    Curve m_Curve;
    float m_MaxDuration;
    float m_TimeElapsed;

    Hitbox(Transform attacker, Action<Transform> hitCallback, Curve curve, float maxDuration, Transform parent, float speed) {
        m_Attacker = attacker;
        m_Parent = parent;
        m_HitCallback = hitCallback;
        m_Curve = curve;
        m_MaxDuration = maxDuration;
        // m_Speed = speed;
    }

    FixedUpdate() {
        check for collisions obeying the specified targeting
        suppose you collide with player x { // (or foreach player collided with)
            Hit(x);
            m_HitCallback(x); // possible attacker stun
        }

        position = parent.position + speed * curve(timeElapsed);
        timeElapsed += Time.FixedDeltaTime;
        if (timeElapsed >= maxDuration) {
            destroy self
        }
    }

    Hit(Transform player) {
        // both will be modified by the victim if they're blocking. call a different function to bypass blocking
        player.TakeDamage(dmg, dmg type, hit stun)
        // destroy unless pierce or aoe
    }

    spawn hitbox that moves given a 
        & an optional speed modifier s*t

    // and how to handle hitboxes that change size?
    */
}
