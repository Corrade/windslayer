using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(InputHandler))]
public class Player : MonoBehaviour
{
    [Tooltip("Force applied downward when in the air")]
    public float GravityDownForce = 10f;

    [Tooltip("Distance from the bottom of the character to raycast for the ground")]
    public float GroundCheckDistance = 0.05f;

    [Tooltip("Horizontal speed")]
    public float Speed = 10f;

    [Tooltip("Force applied upwards when jumping")]
    public float JumpForce = 6f;

    public Vector2 Velocity { get; set; }
    public bool IsGrounded { get; private set; }

    BoxCollider2D m_BoxCollider2D;
    Rigidbody2D m_RB2D;
    InputHandler m_InputHandler;
    Vector2 m_GroundNormal;

    // Start is called before the first frame update
    void Start()
    {
        m_BoxCollider2D = GetComponent<BoxCollider2D>();
        m_RB2D = GetComponent<Rigidbody2D>();
        m_InputHandler = GetComponent<InputHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        GroundCheck();
        HandleMovement();
    }

    // set IsGrounded and m_GroundNormal appropriately
    void GroundCheck()
    {
        IsGrounded = false;
        m_GroundNormal = Vector2.up;

        RaycastHit2D hit = Physics2D.BoxCast(
            transform.position,
            m_BoxCollider2D.size,
            0f,
            Vector2.down,
            GroundCheckDistance,
            LayerMask.GetMask("Ground")
        );

        if (hit.collider != null) {
            m_GroundNormal = hit.normal;

            if (Vector2.Dot(hit.normal, transform.up) > 0f && Vector2.Angle(transform.up, hit.normal) < 45f) {
                IsGrounded = true;

                // Snap to ground
                m_RB2D.MovePosition(m_RB2D.position + Vector2.down * hit.distance);

                Debug.Log("[Ground] name: " + hit.collider.name + ", normal: " + m_GroundNormal);
                Debug.DrawRay(hit.point, hit.normal);
            }
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

        Vector2 playerOrigin = transform.position;
        playerOrigin.y += 0.1f;
        RaycastHit2D hit = Physics2D.BoxCast(
            playerOrigin,
            m_BoxCollider2D.size,
            0f,
            Velocity.normalized,
            Velocity.magnitude * Time.fixedDeltaTime,
            LayerMask.GetMask("Ground", "Wall")
        );

        Debug.Log("Velocity: " + Velocity + ", m_GroundNormal: " + m_GroundNormal);

        if (hit.collider == null) {
            m_RB2D.MovePosition(m_RB2D.position + Velocity * Time.fixedDeltaTime);
        } else {
            Debug.Log("[Wall] name: " + hit.collider.name + ", normal: " + hit.normal);
            Debug.DrawRay(hit.point, hit.normal);
            Velocity = Vector2.zero;
        }
    }

    public Vector2 PerpendicularClockwise(Vector2 vec)
    {
        return new Vector2(vec.y, -vec.x);
    }
}
