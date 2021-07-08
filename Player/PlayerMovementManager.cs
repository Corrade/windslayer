using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D)), RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(PlayerInputManager)), RequireComponent(typeof(PlayerStatusManager))]
public class PlayerMovementManager : MonoBehaviour
{
    [Tooltip("Force applied downward when in the air")]
    public float GravityDownForce = 80f;

    [Tooltip("Distance from the bottom of the character to raycast for the ground")]
    public float GroundCheckDistance = 0.05f;

    [Tooltip("The player will slide off ground planes steeper than this value (degrees)")]
    public float MaximumGroundAngle = 45f;

    [Tooltip("Horizontal speed")]
    public float GroundSpeed = 7f;

    [Tooltip("Air strafe speed")]
    public float AirStrafeSpeed = 7f;

    [Tooltip("Air strafe lerp speed")]
    public float AirStrafeInfluenceSpeed = 7f;

    [Tooltip("Force applied upwards when jumping")]
    public float JumpForce = 24f;

    [Tooltip("Maximum number of times the player can jump without landing")]
    public int MaxJumps = 2;

    public Vector2 CandidateVelocity { get; set; }
    public bool IsGrounded { get; private set; }

    BoxCollider2D m_CollisionCollider;
    Rigidbody2D m_RB2D;
    PlayerInputManager m_PlayerInputManager;
    PlayerStatusManager m_PlayerStatusManager;

    Vector2 m_GroundNormal;
    Collider2D m_GroundCollider;

    // Passed to m_RB2D.MovePosition() at the end of FixedUpdate() as MovePosition() is resolved during the next physics update
    Vector2 m_CandidatePosition;
    int m_JumpCounter = 0;

    void Start()
    {
        m_CollisionCollider = GetComponent<BoxCollider2D>();
        m_RB2D = GetComponent<Rigidbody2D>();
        m_PlayerInputManager = GetComponent<PlayerInputManager>();
        m_PlayerStatusManager = GetComponent<PlayerStatusManager>();
    }

    void FixedUpdate()
    {
        if (m_PlayerStatusManager.HasStatus(Status.Stunned)) {
            return;
        }

        if (m_PlayerStatusManager.HasStatus(Status.Suspended)) {
            CandidateVelocity = Vector2.zero;
            return;
        }
    
        m_CandidatePosition = m_RB2D.position;

        GroundCheck();
        ProposeVelocity();
        AdjustVelocityForObstructions();

        // Move along the final CandidateVelocity
        m_CandidatePosition += CandidateVelocity * Time.fixedDeltaTime;

        if (m_CandidatePosition != m_RB2D.position) {
            // Debug.Log("Moving from " + Debugger.Vector2Full(m_RB2D.position) + " -> " + Debugger.Vector2Full(m_CandidatePosition) + ", CandidateVelocity = " + Debugger.Vector2Full(CandidateVelocity));
            // Debugger.DrawRay(m_RB2D.position, CandidateVelocity * Time.fixedDeltaTime, Color.blue, 1f);
            m_RB2D.MovePosition(m_CandidatePosition);
        }
    }

    // Check whether or not the player is grounded and set the related variables appropriately
    void GroundCheck()
    {
        // Don't check grounding if ascending in the air, e.g. whilst in the upward section of jumping
        if (IsRising()) {
            // Debugger.Log("GroundCheck() skipped - airborne");
            return;
        }

        // Cast the collision collider down by GroundCheckDistance units
        foreach (RaycastHit2D hit in Physics2D.BoxCastAll(
            m_CandidatePosition,
            m_CollisionCollider.size,
            0f,
            Vector2.down,
            GroundCheckDistance,
            LayerMask.GetMask("Collidable")
        )) {
            // Ignore if nothing was obtained or if the ground is too steep
            if (hit.collider == null || IsTooSteep(hit.normal)) {
                continue;
            }

            // Debugger.Log("Grounded on " + hit.collider.name + ", normal=" + hit.normal);
            // Debugger.DrawRay(hit.point, hit.normal, Color.magenta, 1f);

            IsGrounded = true;
            m_GroundNormal = hit.normal;
            m_GroundCollider = hit.collider;
            m_JumpCounter = 0;

            // Snap to ground
            m_CandidatePosition = hit.centroid;

            // Take the first ground
            return;
        }

        // Debugger.Log("No ground detected");
        IsGrounded = false;
        m_GroundNormal = Vector2.up;
        m_GroundCollider = null;
    }

    // Sets CandidateVelocity based on input and grounding
    void ProposeVelocity()
    {
        float worldspaceMoveInput = GetMoveInput();

        if (IsGrounded && !IsTooSteep(m_GroundNormal)) {
            // Grounded movement
            float inputMagnitude = worldspaceMoveInput * GroundSpeed;
            CandidateVelocity = inputMagnitude * VectorAlongSurface(m_GroundNormal);

            // Grounded jump
            if (GetJumpInput()) {
                Jump();
            }
        } else {
            // Air jump
            if (GetJumpInput() && m_JumpCounter < MaxJumps) {
                Jump();
            } else {
                if (IsGrounded && IsTooSteep(m_GroundNormal)) {
                    // Sliding down a steep surface
                    // Debug.Log("Sliding down wall");
                    CandidateVelocity -= VectorAlongSurface(m_GroundNormal) * GravityDownForce * Time.fixedDeltaTime;
                } else {
                    // Air strafing
                    float inputMagnitude = worldspaceMoveInput * AirStrafeSpeed;

                    // Apply air strafing and gravity
                    CandidateVelocity = new Vector2(
                        Mathf.Lerp(CandidateVelocity.x, inputMagnitude, AirStrafeInfluenceSpeed * Time.fixedDeltaTime),
                        CandidateVelocity.y - GravityDownForce * Time.fixedDeltaTime
                    );
                }
            }
        }
    }

    float GetMoveInput()
    {
        if (
            m_PlayerStatusManager.HasStatus(Status.Rooted) ||
            m_PlayerStatusManager.HasStatus(Status.Stunned) ||
            m_PlayerStatusManager.HasStatus(Status.Suspended)
        ) {
            return 0.0f;
        }

        return m_PlayerInputManager.GetMoveInput();
    }

    bool GetJumpInput()
    {
        if (
            m_PlayerStatusManager.HasStatus(Status.Rooted) ||
            m_PlayerStatusManager.HasStatus(Status.Stunned) ||
            m_PlayerStatusManager.HasStatus(Status.Suspended)
        ) {
            return false;
        }

        return m_PlayerInputManager.GetInputDown("jump", false);
    }

    // Checks for obstructions and sets m_CandidatePosition and CandidateVelocity appropriately if an obstruction is detected
    void AdjustVelocityForObstructions()
    {
        // Cast the collision collider in the direction of CandidateVelocity
        foreach (RaycastHit2D hit in Physics2D.BoxCastAll(
            m_CandidatePosition,
            m_CollisionCollider.size,
            0f,
            CandidateVelocity.normalized,
            CandidateVelocity.magnitude * Time.fixedDeltaTime,
            LayerMask.GetMask("Collidable")
        )) {
            // Ignore if nothing was collided with
            if (hit.collider == null) {
                continue;
            }

            // Ignore if the collider isn't actually being moved into, i.e. if the player is just inside/on the collider
            if (!IsMovingInto(CandidateVelocity.normalized, hit.normal)) {
                continue;
            }

            // Debugger.Log("Obstructed by " + hit.collider.name + " with CandidateVelocity=" + CandidateVelocity.normalized + ", normal=" + hit.normal + ", Dot()=" + Vector2.Dot(-hit.normal, CandidateVelocity.normalized) + ", IsGrounded=" + IsGrounded + ", IsTooSteep()=" + IsTooSteep(hit.normal));

            // Snap to the obstruction
            m_CandidatePosition = hit.centroid;

            // Subtract the distance that was moved by snapping
            float remainingMagnitude = Mathf.Sign(CandidateVelocity.magnitude) * (Mathf.Abs(CandidateVelocity.magnitude) - hit.distance);

            if (IsGrounded) {
                if (IsTooSteep(hit.normal)) {
                    // Moving from ground -> steep ground: stop motion, don't allow players to walk onto steep surfaces
                    CandidateVelocity = Vector2.zero;
                } else {
                    // Moving from ground -> regular ground: reorientate movement along the next ground
                    CandidateVelocity = remainingMagnitude * VectorAlongSurface(hit.normal);
                }
            } else {
                if (IsTooSteep(hit.normal)) {
                    // Moving from air -> steep ground: reorientate movement to slide/fall down the ground
                    CandidateVelocity = remainingMagnitude * -VectorAlongSurface(hit.normal);
                } else {
                    // Moving from air -> regular ground: stop motion, player can take over
                    CandidateVelocity = Vector2.zero;
                }
            }
        }
    }

    // Returns whether or not direction is moving into the surface with the given normal. Assumes both parameters are normalized.
    bool IsMovingInto(Vector2 direction, Vector2 normal)
    {
        // If direction is within +-90 degrees of the vector moving into the surface
        return Vector2.Dot(-normal, direction) > 0.01f;
    }

    // Returns the vector pointing up the surface with the given normal
    Vector2 VectorAlongSurface(Vector2 normal)
    {
        if (normal.x <= 0) { // Left-facing
            return PerpendicularClockwise(normal).normalized;
        } else {
            return PerpendicularAntiClockwise(normal).normalized;
        }
    }

    Vector2 PerpendicularAntiClockwise(Vector2 vec)
    {
        return new Vector2(-vec.y, vec.x);
    }

    Vector2 PerpendicularClockwise(Vector2 vec)
    {
        return new Vector2(vec.y, -vec.x);
    }

    void Jump()
    {
        CandidateVelocity = new Vector2(CandidateVelocity.x, JumpForce);
        m_JumpCounter++;
        // Debugger.Log("Jump #" + m_JumpCounter);
    }

    // Given the normal of a ground, returns whether or not it's too steep
    bool IsTooSteep(Vector2 normal)
    {
        return Vector2.Dot(normal, Vector2.up) < Mathf.Cos(MaximumGroundAngle * Mathf.Deg2Rad);
    }

    // Returns whether or not the character is in the air and moving upwards
    bool IsRising()
    {
        return !IsGrounded && CandidateVelocity.y > 0f;
    }
}
