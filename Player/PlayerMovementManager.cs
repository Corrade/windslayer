using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D)), RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(PlayerInputManager)), RequireComponent(typeof(PlayerStatusManager))]
public class PlayerMovementManager : MonoBehaviour
{
    [Tooltip("Force applied downward when in the air")]
    public float GravityDownForce = 120f;

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

    [Tooltip("This velocity is immediately applied to the player when they jump")]
    public float JumpStartVelocity = 5f;

    [Tooltip("The amount of force applied to the player during their jump each physics tick")]
    public float JumpAcceleration = 210f;

    [Tooltip("The maximum amount of time the player can hold down jump whilst accelerating their jump")]
    public float JumpMaxTime = 0.1f;

    [Tooltip("Maximum number of times the player can jump without landing")]
    public int MaxJumps = 2;

    [Tooltip("The player will be unable to stand on platforms they've dropped from for this many frames")]
    public int IgnorePlatformDroppedDuration = 10;

    public Vector2 CandidateVelocity { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsFacingLeft { get; private set; }

    BoxCollider2D m_CollisionCollider;
    Rigidbody2D m_RB2D;
    PlayerInputManager m_PlayerInputManager;
    PlayerStatusManager m_PlayerStatusManager;

    Vector2 m_GroundNormal;
    Collider2D m_GroundCollider;

    List<Collider2D> m_PlatformsRecentlyDropped;

    // Passed to m_RB2D.MovePosition() at the end of FixedUpdate() as MovePosition() is resolved during the next physics update
    Vector2 m_CandidatePosition;
    int m_JumpCounter;
    float m_JumpTime;
    bool m_Jumping;
    float m_PreviousMoveInput;

    void Start()
    {
        m_CollisionCollider = GetComponent<BoxCollider2D>();
        m_RB2D = GetComponent<Rigidbody2D>();
        m_PlayerInputManager = GetComponent<PlayerInputManager>();
        m_PlayerStatusManager = GetComponent<PlayerStatusManager>();
        m_JumpCounter = 0;
        m_JumpTime = 0f;
        m_Jumping = false;
        m_PlatformsRecentlyDropped = new List<Collider2D>();
    }

    void FixedUpdate()
    {
        if (m_PlayerStatusManager.HasAny(Status.Stunned, Status.Suspended)) {
            // But do not reset CandidateVelocity as to preserve it once these statuses clear
            return;
        }
    
        m_CandidatePosition = m_RB2D.position;

        GroundCheck();
        DropPlatformCheck();
        ProposeVelocity();
        AdjustVelocityForObstructions();

        // Move along the final CandidateVelocity
        m_CandidatePosition += CandidateVelocity * Time.fixedDeltaTime;

        if (m_CandidatePosition != m_RB2D.position) {
            // Debug.Log("Moving from " + Debugger.Vector2Full(m_RB2D.position) + " -> " + Debugger.Vector2Full(m_CandidatePosition) + ", CandidateVelocity = " + Debugger.Vector2Full(CandidateVelocity));
            // Debugger.DrawRay(m_RB2D.position, CandidateVelocity * Time.fixedDeltaTime, Color.blue, 1f);
            m_RB2D.MovePosition(m_CandidatePosition);
        }

        if (CandidateVelocity.x < 0) {
            IsFacingLeft = true;
            transform.localScale = new Vector3(-1f, 1f, 1f);
        } else if (CandidateVelocity.x > 0) {
            IsFacingLeft = false;
            transform.localScale = new Vector3(1f, 1f, 1f);
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

        // Reset grounding variables
        IsGrounded = false;
        m_GroundNormal = Vector2.up;
        m_GroundCollider = null;

        // Cast the collision collider down by GroundCheckDistance units
        foreach (RaycastHit2D hit in Physics2D.BoxCastAll(
            m_CandidatePosition,
            m_CollisionCollider.size,
            0f,
            Vector2.down,
            GroundCheckDistance,
            LayerMask.GetMask("Obstruction", "Platform")
        )) {
            // Ignore if nothing was collided with
            if (hit.collider == null) {
                continue;
            }

            // Ignore climbing back onto any platform recently dropped from
            if (m_PlatformsRecentlyDropped.Contains(hit.collider)) {
                continue;
            }

            // The collider is a line spanning the whole height of the character. Hence, it could incorrectly detect a platform that only the character's mid/upper body is colliding with, which would be considered a ceiling.
            if (IsCeiling(hit.normal)) {
                continue;
            }

            // Debugger.Log("Grounded on " + hit.collider.name + ", normal=" + hit.normal);
            // Debugger.DrawRay(hit.point, hit.normal, Color.magenta, 1f);

            IsGrounded = true;
            m_GroundNormal = hit.normal;
            m_GroundCollider = hit.collider;
            m_CandidatePosition = hit.centroid;

            // Prevent scaling steep walls with jump resets
            if (!IsTooSteep(hit.normal)) {
                m_JumpCounter = 0;

                // Prioritise picking a flat ground so that if the player is at an intersection between flat and steep grounds, they will not be grounded on the steeper surface and forced to slide into the flat ground indefinitely
                return;
            }
        }
    }

    // Temporarily ignore ground checks for the current platform if the player is inputting to drop
    void DropPlatformCheck()
    {
        if (IsGrounded && GetDropInput() && m_GroundCollider.gameObject.layer == LayerMask.NameToLayer("Platform")) {
            Collider2D currentPlatform = m_GroundCollider;
            m_PlatformsRecentlyDropped.Add(currentPlatform);

            StartCoroutine(Sync.Delay(IgnorePlatformDroppedDuration, () => {
                m_PlatformsRecentlyDropped.Remove(currentPlatform);
            }));
        }
    }

    // Sets CandidateVelocity based on input and grounding
    void ProposeVelocity()
    {
        float moveInput = GetMoveInput();
        m_PreviousMoveInput = moveInput;

        if (IsGrounded && !IsTooSteep(m_GroundNormal)) {
            // Grounded movement
            float input = moveInput * GroundSpeed;
            CandidateVelocity = input * VectorAlongSurface(m_GroundNormal);

            // Grounded jump
            if (GetJumpInput()) {
                StartJump();
            }
        } else {
            // Air jump
            if (GetJumpInput() && m_JumpCounter < MaxJumps) {
                StartJump();
            } else {
                if (IsGrounded && IsTooSteep(m_GroundNormal)) {
                    // Sliding down a steep surface
                    CandidateVelocity += VectorDownSurface(m_GroundNormal) * GravityDownForce * Time.fixedDeltaTime;
                    Debug.Log("Sliding");
                } else {
                    // Air strafing
                    float input = moveInput * AirStrafeSpeed;

                    // Apply air strafing
                    CandidateVelocity = new Vector2(
                        Mathf.Lerp(CandidateVelocity.x, input, AirStrafeInfluenceSpeed * Time.fixedDeltaTime),
                        CandidateVelocity.y
                    );

                    // Apply gravity
                    if (!m_Jumping) {
                        CandidateVelocity = new Vector2(
                            CandidateVelocity.x,
                            CandidateVelocity.y - GravityDownForce * Time.fixedDeltaTime
                        );
                    }
                }
            }
        }
        
        if (m_Jumping) {
            HoldJump();

            if (m_PlayerInputManager.GetInputUp("jump", false)) {
                StopJump();
            }
        }

        Debugger.DrawRay(m_RB2D.position, CandidateVelocity.normalized, Color.blue, 1f);
    }

    float GetMoveInput()
    {
        if (m_PlayerStatusManager.HasAny(Status.Stunned, Status.Suspended)) {
            return 0.0f;
        } else if (m_PlayerStatusManager.HasAny(Status.Rooted, Status.Casting)) {
            if (!IsGrounded) {
                return m_PreviousMoveInput;
            } else {
                return 0.0f;
            }
        }

        return m_PlayerInputManager.GetMoveInput();
    }

    bool GetDropInput()
    {
        if (m_PlayerStatusManager.HasAny(Status.Rooted, Status.Stunned, Status.Suspended, Status.Casting)) {
            return false;
        }

        return m_PlayerInputManager.GetInputHeld("drop");
    }

    bool GetJumpInput()
    {
        if (m_PlayerStatusManager.HasAny(Status.Rooted, Status.Stunned, Status.Suspended, Status.Casting)) {
            return false;
        }

        return m_PlayerInputManager.GetInputDown("jump", false);
    }

    void StartJump()
    {
        m_JumpCounter++;
        m_Jumping = true;
        m_JumpTime = 0f;
        IsGrounded = false;

        CandidateVelocity = new Vector2(
            CandidateVelocity.x,
            JumpStartVelocity
        );

        // Debugger.Log("Jump #" + m_JumpCounter);
    }

    void HoldJump()
    {
        CandidateVelocity = new Vector2(
            CandidateVelocity.x,
            CandidateVelocity.y + JumpAcceleration * Time.fixedDeltaTime
        );

        m_JumpTime += Time.fixedDeltaTime;

        if (m_JumpTime >= JumpMaxTime) {
            StopJump();
        }
    }

    void StopJump()
    {
        m_Jumping = false;
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
            LayerMask.GetMask("Obstruction", "Platform")
        )) {
            // Ignore if nothing was collided with
            if (hit.collider == null) {
                continue;
            }

            // Don't bump into platforms when rising
            if (IsRising() && hit.collider.gameObject.layer == LayerMask.NameToLayer("Platform")) {
                continue;
            }

            // Ignore if the collider is a platform that was recently dropped from
            if (m_PlatformsRecentlyDropped.Contains(hit.collider)) {
                continue;
            }

            // Ignore if the collider isn't actually being moved into, i.e. if the player is just inside/on the collider
            if (!IsMovingInto(CandidateVelocity.normalized, hit.normal)) {
                continue;
            }

            Debugger.Log("Obstructed by " + hit.collider.name + " with CandidateVelocity=" + CandidateVelocity.normalized + ", normal=" + hit.normal + ", Dot()=" + Vector2.Dot(-hit.normal, CandidateVelocity.normalized) + ", IsGrounded=" + IsGrounded + ", IsTooSteep()=" + IsTooSteep(hit.normal));
            // Debugger.DrawRay(hit.point, VectorAlongSurface(hit.normal), Color.blue, 1f);

            // Snap to the obstruction
            m_CandidatePosition = hit.centroid;

            // Subtract the distance that was moved by snapping
            float remainingMagnitude = (CandidateVelocity.x < 0 ? -1 : 1) * Mathf.Abs(Mathf.Abs(CandidateVelocity.magnitude) - hit.distance);

            if (IsGrounded) {
                if (IsTooSteep(hit.normal)) {
                    // Moving from ground -> steep ground: stop motion
                    CandidateVelocity = Vector2.zero;
                } else {
                    if (!IsTooSteep(m_GroundNormal)) {
                        // Moving from regular ground -> regular ground: reorientate movement along the next ground
                        CandidateVelocity = remainingMagnitude * VectorAlongSurface(hit.normal);
                    } else {
                        // Moving from steep ground -> regular ground: stop motion
                        CandidateVelocity = Vector2.zero;
                    }
                }
            } else {
                if (!IsCeiling(hit.normal) && CandidateVelocity.y > 0 && IsMovingInto(CandidateVelocity, hit.normal)) {
                    // Running into a non-ceiling ground while rising in the air: ignore horizontal movement and just keep rising
                    CandidateVelocity = new Vector2(0f, CandidateVelocity.y);
                } else {
                    // Moving from air -> ground: stop motion
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

    // Returns the vector going right along the surface with the given normal
    Vector2 VectorAlongSurface(Vector2 normal)
    {
        return PerpendicularClockwise(normal).normalized;
    }

    // Returns the vector going down along the surface with the given normal
    Vector2 VectorDownSurface(Vector2 normal)
    {
        if (normal.x <= 0) { // Surface is like /
            return PerpendicularAntiClockwise(normal).normalized;
        } else { // Surface is like \
            return PerpendicularClockwise(normal).normalized;
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

    // Given the normal of a ground, returns whether or not it's too steep
    bool IsTooSteep(Vector2 normal)
    {
        return Vector2.Dot(normal, Vector2.up) < Mathf.Cos(MaximumGroundAngle * Mathf.Deg2Rad);
    }

    // Given the normal of a ground, returns whether or not it's a ceiling
    bool IsCeiling(Vector2 normal)
    {
        return normal.y < 0;
    }

    // Returns whether or not the character is in the air and moving upwards
    bool IsRising()
    {
        return !IsGrounded && CandidateVelocity.y > 0f;
    }
}
