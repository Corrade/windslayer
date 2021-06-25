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

        RaycastHit2D hit = Physics2D.Raycast(Foot.position, Vector2.down, GroundCheckDistance, LayerMask.GetMask("Ground"));

        if (hit.collider != null) {
            m_GroundNormal = hit.normal;
            m_GroundCollider = hit.collider;

            // if (Vector2.Dot(hit.normal, transform.up) > 0f && Vector2.Angle(transform.up, hit.normal) < 45f) {
            IsGrounded = true;

            // Snap to ground
            // Don't use hit.point as it returns things inside the other collider
            m_CurrMovePosition = Physics2D.ClosestPoint(Foot.position, hit.collider) + (Vector2.up * m_BoxCollider2D.size.y / 2);

            Debug.DrawRay(Physics2D.ClosestPoint(Foot.position, hit.collider), hit.normal);

            // ground is on GroundUp but normal is for ground flat????

            Debug.Log("Grounded on " + hit.collider.name);
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

        RaycastHit2D groundHit = new RaycastHit2D();
        RaycastHit2D hit = new RaycastHit2D();

        // problem is: when you're at the intersection of two or more grounds, you have to pick one ground. this is important as the direction of the ground determines the axis of movement, and you always want to be moving ALONG the ground rather than running into it
        // if a movement would see your foot move into a ground, stop. go to the point of intersection. check if valid ground. if valid, reorientate movement to that ground and proceed.
        if (IsGrounded) {
            foreach (RaycastHit2D h in Physics2D.RaycastAll(
                Foot.position,
                Velocity.normalized,
                Velocity.magnitude * Time.fixedDeltaTime,
                LayerMask.GetMask("Ground")
            )) {
                if (h.collider != null) {
                    if (m_GroundCollider != null && h.collider.name == m_GroundCollider.name) {
                        continue;
                    }

                    groundHit = h;
                    break;
                }
            }

            if (groundHit.collider != null) {
                Debug.Log("switching from " + m_GroundCollider.name + " to " + groundHit.collider.name);

                // Move to the intersection between the two grounds
                m_CurrMovePosition = groundHit.point + (Vector2.up * m_BoxCollider2D.size.y / 2);
                Debug.DrawRay(groundHit.point, Vector2.up, Color.red, 3f);

                // Reorientate the remaining length of movement on the new ground
                float mag = Math.Abs(Velocity.magnitude) - groundHit.distance;
                mag = Velocity.x < 0 ? -mag : mag;
                Velocity = PerpendicularClockwise(groundHit.normal).normalized * mag;

                // Move the remaining distance
                Debug.DrawLine(groundHit.point, groundHit.point + Velocity * Time.fixedDeltaTime, Color.green, 3f);
                m_CurrMovePosition += Velocity * Time.fixedDeltaTime;
                return;
            }
        }

        foreach (RaycastHit2D h in Physics2D.BoxCastAll(
            m_RB2D.position,
            m_BoxCollider2D.size,
            0f,
            Velocity.normalized,
            Velocity.magnitude * Time.fixedDeltaTime,
            LayerMask.GetMask("Wall")
        )) {
            if (h.collider != null && h.collider != m_GroundCollider) {
                hit = h;
                break;
            }
        }

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
