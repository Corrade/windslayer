using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Windslayer;

namespace Windslayer.Server
{
    [RequireComponent(typeof(Collider2D)), RequireComponent(typeof(Rigidbody2D))]
    public class Hitbox : MonoBehaviour
    {
        public bool Destroyed { get; private set; }

        Collider2D m_Hitbox;
        int m_FramesElapsed = 0;

        Attack m_Attack;

        Vector3 m_InitPos;
        float m_Speed;
        Curve m_Path;

        int m_Lifetime;

        // An alternative implementation could define these functions in the interface of the attack and call them through a reference, but that would not support attacks that need multiple implementations of the same delegate, e.g. to create multiple hitboxes with different behaviours. Another option would be to define these delegates in a hitbox subclass, but that would require creating at least one hitbox subclass for each attack, which would be quite tedious.
        Func<GameObject, bool> m_IsTarget; // f(collided game object) -> whether or not the object should be hit, i.e. most often this will check the team of the hit player
        Action<GameObject> m_Hit; // f(hit game object)

        bool m_IsReady = false;
        bool m_HitboxAppearedOnce = false;

        void Start()
        {
            m_Hitbox = GetComponent<Collider2D>();
            Destroyed = false;
        }

        // These variables are passed in from the calling attack.
        public void Initialise(
            Attack attack,
            Vector3 initPos,
            float speed,
            Curve path,
            int lifetime,
            Func<GameObject, bool> isTarget,
            Action<GameObject> hit
        ) {
            m_Attack = attack;

            m_InitPos = initPos;
            m_Speed = speed;
            m_Path = path;

            m_Lifetime = lifetime;

            m_IsTarget = isTarget;
            m_Hit = hit;

            SetPosition();
            m_IsReady = true;
        }

        // Hitboxes are responsible for destroying themselves as they may outlast the ability that created them. The ability caller may still destroy them early by called Interrupt().
        public void Interrupt()
        {
            End();
        }

        void Update()
        {
            if (!m_IsReady) {
                return;
            }

            if (m_FramesElapsed++ >= m_Lifetime && m_HitboxAppearedOnce) {
                End();
            }
        }

        void FixedUpdate()
        {
            if (!m_IsReady) {
                return;
            }

            CheckForHits();
            SetPosition();

            m_HitboxAppearedOnce = true;
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
            transform.localPosition = m_InitPos + m_Speed * m_Path.GetPoint(m_FramesElapsed);
        }

        void End()
        {
            Destroyed = true;
            Destroy(gameObject);
        }
    }
}
