using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(InputHandler))]
public class Player : MonoBehaviour
{
    [Tooltip("Force applied downward when in the air")]
    public float GravityDownForce = 10f;

    [Tooltip("Distance from the bottom of the character to raycast for the ground")]
    public float GroundCheckDistance = 0.05f;

    [Tooltip("Horizontal speed")]
    public float Speed = 10f;

    [Tooltip("Get at most this many units to a wall (must be > 0 as to not get stuck inside a wall)")]
    public float WallRepel = 0.05f;

    [Tooltip("Force applied upwards when jumping")]
    public float JumpForce = 6f;

    [Tooltip("The empty child game object representing the point of contact between the player and the ground")]
    public Transform Foot;

    public Vector2 Velocity { get; set; }
    public bool IsGrounded { get; private set; }

    BoxCollider2D m_BoxCollider2D;
    Rigidbody2D m_RB2D;
    InputHandler m_InputHandler;

    Vector2 m_GroundNormal;
    Collider2D m_GroundCollider;

    // Passed to m_RB2D.MovePosition() at the end of FixedUpdate() as MovePosition() is resolved during the next physics update
    Vector2 m_CurrMovePosition;


    // Start is called before the first frame update
    void Start()
    {
        m_BoxCollider2D = GetComponent<BoxCollider2D>();
        m_RB2D = GetComponent<Rigidbody2D>();
        m_InputHandler = GetComponent<InputHandler>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        m_CurrMovePosition = m_RB2D.position;

        GroundCheck();
        HandleMovement();

        if (m_CurrMovePosition != m_RB2D.position) {
            m_RB2D.MovePosition(m_CurrMovePosition);
        }
    }

    // set IsGrounded and m_GroundNormal appropriately
    void GroundCheck()
    {
        IsGrounded = false;
        m_GroundNormal = Vector2.up;
        m_GroundCollider = null;

        /*
        RaycastHit2D[] results = new RaycastHit2D[5];
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Ground"));
        Physics2D.BoxCast(
            transform.position,
            m_BoxCollider2D.size,
            0f,
            Vector2.down,
            filter,
            results,
            GroundCheckDistance
        );
        Array.Sort<RaycastHit2D>(results, delegate(RaycastHit2D x, RaycastHit2D y) {
            if (x.collider == null && y.collider == null) {
                return 0;
            } else if (x.collider == null) {
                return -1;
            } else if (y.collider == null) {
                return 1;
            } else {
                return Vector2.Distance(transform.position, x.point) < Vector2.Distance(transform.position, y.point) ? -1 : 1;
            }
        });*/

        RaycastHit2D hit = new RaycastHit2D();
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            transform.position,
            m_BoxCollider2D.size,
            0f,
            Vector2.down,
            GroundCheckDistance,
            LayerMask.GetMask("Ground")
        );

        Array.Sort<RaycastHit2D>(hits, delegate(RaycastHit2D x, RaycastHit2D y) {
            if (x.collider == null && y.collider == null) {
                return 0;
            } else if (x.collider == null) {
                return -1;
            } else if (y.collider == null) {
                return 1;
            } else {
                Debug.DrawRay(x.point, x.normal);
                Debug.DrawRay(y.point, y.normal);
                return Physics2D.Distance(m_BoxCollider2D, x.collider).distance < Physics2D.Distance(m_BoxCollider2D, y.collider).distance ? -1 : 1;
            }
        });

        /*
        string x = "Grounds: ";
        foreach (RaycastHit2D h in hits) {
            if (h.collider != null) {
                x += h.collider.name + ", ";
            }
        }
        Debug.Log(x);
        */

        foreach (RaycastHit2D h in hits) {
            if (h.collider != null) {
                hit = h;
                m_GroundCollider = h.collider;
                break;
            }
        }

        // INSTEAD: foot collider. just one point (or a line) beneath the player. the player is therefore centered on the platform they're on (dot) or they edge on it (line). then, collider for obstacles. so two separate things.

        if (hit.collider != null) {
            m_GroundNormal = hit.normal;

            // if (Vector2.Dot(hit.normal, transform.up) > 0f && Vector2.Angle(transform.up, hit.normal) < 45f) {
            IsGrounded = true;

            // Snap to ground
            Vector2 to = Physics2D.ClosestPoint(m_RB2D.position + Vector2.down *  m_BoxCollider2D.size.y / 2f, hit.collider) + hit.normal * m_BoxCollider2D.size.y / 2f;
            m_CurrMovePosition.y = to.y;

            Debug.Log("Grounded - name: " + hit.collider.name + ", height: " + hit.centroid.y);
            // Debug.DrawRay(m_RB2D.position, hit.normal);
        } else {
            Debug.Log("Not grounded");
        }

        /*
        // only try to detect ground if it's been a short amount of time since last jump; otherwise we may snap to the ground instantly after we try jumping
        if (Time.time >= m_LastTimeJumped + k_JumpGroundingPreventionTime) {
        }
        */
    }

    void HandleMovement()
    {
        if (IsGrounded) {
            float worldspaceMoveInput = m_InputHandler.GetMoveInput();
            float magnitude = worldspaceMoveInput * Speed;

            // Reorientate on slope
            Vector2 directionOnSlope = PerpendicularClockwise(m_GroundNormal).normalized;

            Velocity = directionOnSlope * magnitude;

            /*
                // jumping
                if (IsGrounded && m_InputHandler.GetJumpInputDown()) {
                    // force the crouch state to false
                        // then, add the jumpSpeed value upwards
                        CharacterVelocity += Vector3.up * JumpForce;

                        // remember last time we jumped because we need to prevent snapping to ground for a short time
                        m_LastTimeJumped = Time.time;

                        // Force grounding to false
                        IsGrounded = false;
                        m_GroundNormal = Vector3.up;
                    }
                }
            */
        } else {
            // Air strafing
            // ...

            // Gravity
            Velocity += Vector2.down * GravityDownForce * Time.fixedDeltaTime;
        }

        RaycastHit2D hit = new RaycastHit2D();

        foreach (RaycastHit2D h in Physics2D.BoxCastAll(
            m_RB2D.position,
            m_BoxCollider2D.size,
            0f,
            Velocity.normalized,
            Velocity.magnitude * Time.fixedDeltaTime,
            IsGrounded ? LayerMask.GetMask("Wall") : LayerMask.GetMask("Ground", "Wall")
        )) {
            if (h.collider != null && hit.collider != m_GroundCollider) {
                hit = h;
                break;
            }
        } // remove collider of the ground

        // Debug.Log("Candidate: " + m_RB2D.position.y + " -> " + (m_RB2D.position + Velocity * Time.fixedDeltaTime).y);
        if (hit.collider == null) {
            m_CurrMovePosition += Velocity * Time.fixedDeltaTime;
            // Debug.Log("Success");
        } else {
            m_CurrMovePosition = hit.centroid - Velocity.normalized * WallRepel;

            // Cancel out momentum - relevant if not grounded
            Velocity = Vector2.zero;
    
            Debug.Log("Obstructed - name: " + hit.collider.name + ", normal: " + hit.normal);
            Debug.DrawRay(hit.centroid, hit.normal);
        }
    }

    public Vector2 PerpendicularClockwise(Vector2 vec)
    {
        return new Vector2(vec.y, -vec.x);
    }
}
