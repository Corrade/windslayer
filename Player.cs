using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(InputHandler))]
public class Player : MonoBehaviour
{
    [Tooltip("Force applied downward when in the air")]
    public float GravityDownForce = 80f;

    [Tooltip("Distance from the bottom of the character to raycast for the ground")]
    public float GroundCheckDistance = 0.05f;

    [Tooltip("Horizontal speed")]
    public float Speed = 5f;

    [Tooltip("Force applied upwards when jumping")]
    public float JumpForce = 6f;

    [Tooltip("Maximum number of times the player can jump without landing")]
    public int MaxJumps = 2;

    [Tooltip("Ground planes steeper than this value (in degrees) will instead be considered as obstructions")]
    public float MaximumGroundAngle = 45f;

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
    Vector2 m_CandidatePosition;
    int m_JumpCounter = 0;

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
        m_CandidatePosition = m_RB2D.position;

        GroundCheck();

        ProposeVelocity();

        AdjustVelocityForObstructions();

        AdjustVelocityForGroundTransition();

        // Move along the final velocity
        m_CandidatePosition += Velocity * Time.fixedDeltaTime;
        Debugger.DrawRay(m_RB2D.position, Velocity * Time.fixedDeltaTime, Color.blue, 1f);

        if (m_CandidatePosition != m_RB2D.position) {
            m_RB2D.MovePosition(m_CandidatePosition);
        }
    }

    // set IsGrounded and m_GroundNormal appropriately
    void GroundCheck()
    {
        // Don't check grounding if ascending in the air (e.g. whilst in the upward section of jumping)
        if (!IsGrounded && Velocity.y > 0f) {
            Debugger.Log("GroundCheck() skipped - airborne");
            return;
        }

        ResetGrounding();

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            (m_BoxCollider2D.size.y / 2) + GroundCheckDistance,
            LayerMask.GetMask("Wall", "Ground")
        );
        Debugger.DrawRay(transform.position, Vector2.down * ((m_BoxCollider2D.size.y / 2) + GroundCheckDistance), Color.yellow, 1f);

        // If ground was detected
        if (hit.collider != null) {
            Debugger.Log("Grounded on " + hit.collider.name);

            IsGrounded = true;
            m_GroundNormal = hit.normal;
            m_GroundCollider = hit.collider;
            m_JumpCounter = 0;

            // Snap to ground
            m_CandidatePosition = hit.point + (Vector2.up * m_BoxCollider2D.size.y / 2);
            Debugger.DrawRay(hit.point, hit.normal, Color.magenta, 1f);
        } else {
            Debugger.Log("No ground detected");
        }
    }

    void ProposeVelocity()
    {
        if (IsGrounded) {
            float worldspaceMoveInput = m_InputHandler.GetMoveInput();
            float magnitude = worldspaceMoveInput * Speed;
            Velocity = magnitude * VectorAlongSurface(m_GroundNormal);

            Debugger.DrawRay(Foot.position, Velocity, Color.red, 1f);

            // Check for jump
            if (m_InputHandler.JumpInputDown) {
                m_InputHandler.JumpInputDown = false;

                SetVelocityToJump();
                ResetGrounding();
            }
        } else {
            // Check for airborne jump
            if (m_InputHandler.JumpInputDown && m_JumpCounter < MaxJumps) {
                m_InputHandler.JumpInputDown = false;

                SetVelocityToJump();
            } else {
                // Gravity
                Velocity += Vector2.down * GravityDownForce * Time.fixedDeltaTime;
            }
        }
    }

    // Checks for obstructions and sets m_CandidatePosition and Velocity appropriately if an obstruction is detected. Returns whether or not an obstruction is detected.
    void AdjustVelocityForObstructions()
    {
        RaycastHit2D obstructionHit = new RaycastHit2D();

        // Boxcast from the current position to the candidate position
        foreach (RaycastHit2D hit in Physics2D.BoxCastAll(
            m_CandidatePosition, // m_RB2D.position
            m_BoxCollider2D.size,
            0f,
            Velocity.normalized,
            Velocity.magnitude * Time.fixedDeltaTime,
            LayerMask.GetMask("Wall", "Ground")
        )) {
            if (hit.collider != null && IsTooSteep(hit.normal)) {
                obstructionHit = hit;
                break;
            }
        }

        if (obstructionHit.collider != null) {
            Debugger.Log("Obstructed by " + obstructionHit.collider.name);

            // Move to stop at the obstruction
            m_CandidatePosition = obstructionHit.centroid;

            // Reorientate the velocity to slide down the obstruction
            Velocity = -Velocity.magnitude * VectorAlongSurface(obstructionHit.normal);
        }
    }

    // Checks if the candidate movement (Velocity) would cause the character to switch from one ground to another. If so, the character's movement is reoriented along the ground transition.
    // This is necessary as normally the direction of the ground determines the direction of movement, but you don't to want into another ground
    void AdjustVelocityForGroundTransition()
    {
        if (!IsGrounded) {
            return;
        }

        RaycastHit2D nextGroundHit = new RaycastHit2D();

        // Raycast the movement of the foot
        foreach (RaycastHit2D hit in Physics2D.RaycastAll(
            m_CandidatePosition - (Vector2.up * m_BoxCollider2D.size.y / 2), // Foot.position,
            Velocity.normalized,
            Velocity.magnitude * Time.fixedDeltaTime,
            LayerMask.GetMask("Ground")
        )) {
            if (hit.collider != null) {
                if (m_GroundCollider != null && hit.collider.name == m_GroundCollider.name) {
                    continue;
                }

                nextGroundHit = hit;
                break;
            }
        }

        if (nextGroundHit.collider != null) {
            Debugger.Log("Transitioning grounds from " + m_GroundCollider.name + " to " + nextGroundHit.collider.name);

            // Move to the intersection between the two grounds
            m_CandidatePosition = nextGroundHit.point + (Vector2.up * m_BoxCollider2D.size.y / 2);
            Debugger.DrawRay(nextGroundHit.point, Vector2.up, Color.white, 1f);

            // Reorientate the remaining length of movement on the new ground
            float mag = Mathf.Sign(Velocity.x) * (Mathf.Abs(Velocity.magnitude) - nextGroundHit.distance);
            Velocity = VectorAlongSurface(nextGroundHit.normal) * mag;
        }
    }

    void ResetGrounding()
    {
        IsGrounded = false;
        m_GroundNormal = Vector2.up;
        m_GroundCollider = null;
    }

    Vector2 VectorAlongSurface(Vector2 normal)
    {
        return PerpendicularClockwise(normal).normalized;
    }

    Vector2 PerpendicularClockwise(Vector2 vec)
    {
        return new Vector2(vec.y, -vec.x);
    }

    void SetVelocityToJump()
    {
        Velocity = new Vector2(Velocity.x, 0f);
        Velocity += Vector2.up * JumpForce;

        m_JumpCounter++;
        Debugger.Log("Jump #" + m_JumpCounter);
    }

    // Given the normal of a ground, returns whether or not it's too steep
    bool IsTooSteep(Vector2 normal)
    {
        return Vector2.Dot(normal, Vector2.up) < Mathf.Cos(MaximumGroundAngle * Mathf.Deg2Rad);
    }
}
