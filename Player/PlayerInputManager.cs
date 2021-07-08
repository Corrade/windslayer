using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    public class Key
    {
        public KeyCode KeyCode { get; set; }
        public bool IsDown { get; set; }
        public bool IsUp { get; set; }
        public bool IsCheckingDownUpFromFixedUpdate { get; set; }

        public Key(KeyCode keyCode, bool isCheckingFromFixedUpdate)
        {
            KeyCode = keyCode;
            IsDown = false;
            IsUp = false;
            IsCheckingDownUpFromFixedUpdate = isCheckingFromFixedUpdate;
        }

        public bool GetKey()
        {
            return Input.GetKey(KeyCode);
        }

        public bool GetKeyDown(bool fromUpdate)
        {
            return fromUpdate ? Input.GetKeyDown(KeyCode) : IsDown;
        }

        public bool GetKeyUp(bool fromUpdate)
        {
            return fromUpdate ? Input.GetKeyUp(KeyCode) : IsUp;
        }
    }

    Dictionary<string, Key> m_Binds;

    void Start()
    {
        m_Binds = new Dictionary<string, Key>();
        m_Binds.Add("left", new Key(KeyCode.LeftArrow, false));
        m_Binds.Add("right", new Key(KeyCode.RightArrow, false));
        m_Binds.Add("jump", new Key(KeyCode.Space, true));
        m_Binds.Add("crouch", new Key(KeyCode.DownArrow, true));
        m_Binds.Add("light_attack", new Key(KeyCode.S, false));
        m_Binds.Add("strong_attack", new Key(KeyCode.D, false));
        m_Binds.Add("block", new Key(KeyCode.A, false));
    }

    // Problem: normally, you can't reliably call GetKeyDown() or GetKeyUp() from FixedUpdate() since those events may fire off between fixed updates.
    // Solution: we use m_IsDown and m_IsUp. Each of these are set to be true in Update() if their corresponding events have occurred, but they are only set to be false at the end of FixedUpdate(). This way, anybody calling these functions from FixedUpdate() can catch the value. We ensure this script runs at the end of FixedUpdate() by setting this script's execution order to last.
    void Update()
    {
        foreach (KeyValuePair<string, Key> entry in m_Binds) {
            if (entry.Value.IsCheckingDownUpFromFixedUpdate) {
                if (entry.Value.GetKeyDown(true)) {
                    entry.Value.IsDown = true;
                }

                if (entry.Value.GetKeyDown(true)) {
                    entry.Value.IsUp = true;
                }
            }
        }
    }

    void FixedUpdate()
    {
        foreach (KeyValuePair<string, Key> entry in m_Binds) {
            if (entry.Value.IsCheckingDownUpFromFixedUpdate) {
                entry.Value.IsDown = false;
                entry.Value.IsUp = false;
            }
        }
    }

    public void AddBind(string name, KeyCode keyCode, bool isCheckingDownUpFromFixedUpdate)
    {
        
    }

    public void RemoveBind(string name)
    {

    }

    public void SwapBind(string name1, string name2)
    {

    }

    public void EditBind(string name, KeyCode keyCode, bool isCheckingDownUpFromFixedUpdate)
    {

    }

    public float GetMoveInput()
    {
        float res = 0f;

        if (m_Binds["left"].GetKey()) {
            res -= 1f;
        }

        if (m_Binds["right"].GetKey()) {
            res += 1f;
        }

        return res;
    }

    public bool GetInputHeld(string name)
    {
        return m_Binds[name].GetKey();
    }

    public bool GetInputDown(string name, bool fromUpdate)
    {
        return m_Binds[name].GetKeyDown(fromUpdate);
    }

    public bool GetInputUp(string name, bool fromUpdate)
    {
        return m_Binds[name].GetKeyUp(fromUpdate);
    }
}
