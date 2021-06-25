using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    const string k_AxisNameHorizontal = "Horizontal";
    const string k_AxisNameVertical = "Vertical";
    const string k_ButtonNameJump = "Jump";

    public bool CanProcessInput() => true;

    public float GetMoveInput()
    {
        if (!CanProcessInput()) {
            return 0f;
        }

        return Input.GetAxisRaw(k_AxisNameHorizontal);
    }

    public bool GetJumpInputDown()
    {
        if (!CanProcessInput()) {
            return false;
        }
        
        return Input.GetButtonDown(k_ButtonNameJump);
    }

    public bool GetJumpInputHeld()
    {
        if (!CanProcessInput()) {
            return false;
        }
        
        return Input.GetButton(k_ButtonNameJump);
    }
}
