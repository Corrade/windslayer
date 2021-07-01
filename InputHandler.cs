using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    const string k_AxisNameHorizontal = "Horizontal";
    const string k_AxisNameVertical = "Vertical";
    const string k_ButtonNameJump = "Jump";

    bool m_JumpInputDown;

    void Update()
    {
        if (Input.GetButtonDown(k_ButtonNameJump)) {
            m_JumpInputDown = true;
        }
    }

    public bool CanProcessInput() => true;

    public float GetMoveInput()
    {
        if (!CanProcessInput()) {
            return 0f;
        }

        return Input.GetAxisRaw(k_AxisNameHorizontal);
    }

    public bool GetJumpInputHeld()
    {
        if (!CanProcessInput()) {
            return false;
        }
        
        return Input.GetButton(k_ButtonNameJump);
    }

    // Checks if the jump input has been pressed down since the last time this function was called. This is intended to be called every frame from FixedUpdate() since GetButtonDown() itself doesn't reliably work when called from FixedUpdate().
    public bool RetrieveJumpInputDown()
    {
        if (m_JumpInputDown) {
            m_JumpInputDown = false;
            return true;
        }

        return false;
    }
}
