using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManagerTmp : PlayerInputManager
{
    protected override void SetupBinds() {
        m_Binds.Add("left", new Key(KeyCode.K, false));
        m_Binds.Add("right", new Key(KeyCode.Semicolon, false));
        m_Binds.Add("jump", new Key(KeyCode.O, true));
        m_Binds.Add("drop", new Key(KeyCode.L, true));
        m_Binds.Add("light_attack", new Key(KeyCode.Comma, false));
        m_Binds.Add("strong_attack", new Key(KeyCode.Slash, false));
        m_Binds.Add("block", new Key(KeyCode.Period, false));
    }
}
