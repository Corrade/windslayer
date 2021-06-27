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

    [Tooltip("Get at most this many units to a wall (must be > 0 as to not get stuck inside a wall)")]
    public float WallRepel = 0.05f;

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
        HandleMovement();

        if (m_CandidatePosition != m_RB2D.position) {
            m_RB2D.MovePosition(m_CandidatePosition);
        }
    }

    // set IsGrounded and m_GroundNormal appropriately
    void GroundCheck()
    {
        // Don't check grounding if ascending in the air (e.g. whilst in the upward section of jumping)
        if (!IsGrounded && Velocity.y > 0f) {
            Debug.Log("GroundCheck() skipped - airborne");
            return;
        }

        ResetGrounding();

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            (m_BoxCollider2D.size.y / 2) + GroundCheckDistance,
            LayerMask.GetMask("Ground")
        );
        Debug.DrawRay(transform.position, Vector2.down * ((m_BoxCollider2D.size.y / 2) + GroundCheckDistance), Color.yellow, 1f);

        // If ground was detected
        if (hit.collider != null) {
            Debug.Log("Ground detected - " + hit.collider.name);

            IsGrounded = true;
            m_GroundNormal = hit.normal;
            m_GroundCollider = hit.collider;
            m_JumpCounter = 0;

            // Check steepness
            if (Vector2.Dot(hit.normal, Vector2.up) < Mathf.Cos(MaximumGroundAngle * Mathf.Deg2Rad)) {
                Debug.Log("Too steep");
            }

            // Snap to ground
            m_CandidatePosition = hit.point + (Vector2.up * m_BoxCollider2D.size.y / 2);
            Debug.DrawRay(hit.point, hit.normal, Color.magenta, 1f);
        } else {
            Debug.Log("No ground detected");
        }
    }

    void HandleMovement()
    {
        if (IsGrounded) {
            Debug.Log("Grounded");

            float worldspaceMoveInput = m_InputHandler.GetMoveInput();
            float magnitude = worldspaceMoveInput * Speed;

            // Reorientate on slope
            Vector2 directionOnSlope = PerpendicularClockwise(m_GroundNormal).normalized;

            Velocity = directionOnSlope * magnitude;

            Debug.DrawRay(Foot.position, Velocity, Color.red, 1f);

            // Basic jumping
            if (m_InputHandler.GetJumpInputHeld()) {
                Velocity = new Vector2(Velocity.x, 0f);
                Velocity += Vector2.up * JumpForce;
                m_JumpCounter++;
                Debug.Log("Jump #" + m_JumpCounter);

                ResetGrounding();
            }
        } else {
            Debug.Log("Not grounded");

            // Double jump
            if (m_InputHandler.GetJumpInputDown() && m_JumpCounter < MaxJumps) {
                Velocity = new Vector2(Velocity.x, 0f);
                Velocity += Vector2.up * JumpForce;
                m_JumpCounter++;
                Debug.Log("Jump #" + m_JumpCounter);
            } else {
                // Gravity
                Velocity += Vector2.down * GravityDownForce * Time.fixedDeltaTime;
            }
        }

        if (HandleObstructions()) {
            return;
        }

        if (TransitionBetweenGrounds()) {
            return;
        }

        m_CandidatePosition += Velocity * Time.fixedDeltaTime;
        Debug.DrawRay(m_RB2D.position, Velocity * Time.fixedDeltaTime, Color.blue, 1f);
    }

    // Returns whether or not
    bool TransitionBetweenGrounds()
    {
        if (!IsGrounded) {
            return false;
        }

        // problem is: when you're at the intersection of two or more grounds, you have to pick one ground. this is important as the direction of the ground determines the axis of movement, and you always want to be moving ALONG the ground rather than running into it
        // if a movement would see your foot move into a ground, stop. go to the point of intersection. check if valid ground. if valid, reorientate movement to that ground and proceed.

        RaycastHit2D chosenGroundHit = new RaycastHit2D();

        // Raycast the movement of the foot
        foreach (RaycastHit2D hit in Physics2D.RaycastAll(
            Foot.position,
            Velocity.normalized,
            Velocity.magnitude * Time.fixedDeltaTime,
            LayerMask.GetMask("Ground")
        )) {
            if (hit.collider != null) {
                if (m_GroundCollider != null && hit.collider.name == m_GroundCollider.name) {
                    continue;
                }

                chosenGroundHit = hit;
                break;
            }
        }

        if (chosenGroundHit.collider != null) {
            Debug.Log("Transitioning grounds from " + m_GroundCollider.name + " to " + chosenGroundHit.collider.name);

            // Move to the intersection between the two grounds
            m_CandidatePosition = chosenGroundHit.point + (Vector2.up * m_BoxCollider2D.size.y / 2);
            Debug.DrawRay(chosenGroundHit.point, Vector2.up, Color.white, 1f);

            // Reorientate the remaining length of movement on the new ground
            float mag = Math.Abs(Velocity.magnitude) - chosenGroundHit.distance;
            mag = Velocity.x < 0 ? -mag : mag;
            Velocity = PerpendicularClockwise(chosenGroundHit.normal).normalized * mag;

            // Move the remaining distance
            Debug.DrawLine(chosenGroundHit.point, chosenGroundHit.point + Velocity * Time.fixedDeltaTime, Color.green, 3f);
            m_CandidatePosition += Velocity * Time.fixedDeltaTime;

            return true;
        }

        return false;
    }

    // Checks for obstructions and sets m_CandidatePosition and Velocity appropriately if an obstruction is detected. Returns whether or not an obstruction is detected.
    bool HandleObstructions()
    {
        RaycastHit2D obstructionHit = new RaycastHit2D();

        // Boxcast from the current position to the candidate position
        foreach (RaycastHit2D hit in Physics2D.BoxCastAll(
            m_RB2D.position,
            m_BoxCollider2D.size,
            0f,
            Velocity.normalized,
            Velocity.magnitude * Time.fixedDeltaTime,
            LayerMask.GetMask("Wall")
        )) {
            if (hit.collider != null) {
                obstructionHit = hit;
                break;
            }
        }

        // Debug.Log("Candidate: " + m_RB2D.position.y + " -> " + (m_RB2D.position + Velocity * Time.fixedDeltaTime).y);
        if (obstructionHit.collider != null) {
            Debug.Log("Obstructed by " + obstructionHit.collider.name);

            // Set position to a specified distance from the obstruction following the direction of the current velocity
            m_CandidatePosition = obstructionHit.centroid - Velocity.normalized * WallRepel;

            // Cancel out momentum
            Velocity = Vector2.zero;
    
            return true;
        }

        return false;
    }

    void ResetGrounding()
    {
        IsGrounded = false;
        m_GroundNormal = Vector2.up;
        m_GroundCollider = null;
    }

    public Vector2 PerpendicularClockwise(Vector2 vec)
    {
        return new Vector2(vec.y, -vec.x);
    }
}
