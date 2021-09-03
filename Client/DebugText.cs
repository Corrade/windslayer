using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class DebugText : MonoBehaviour
{
    /*
    public PlayerStatManager PlayerStatManager;
    public PlayerStatusManager PlayerStatusManager;
    
    Text m_Text;

    // Start is called before the first frame update
    void Start()
    {
        m_Text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        m_Text.text = "";
        m_Text.text += "Health: " + PlayerStatManager.Health + "\n";
        m_Text.text += "Mana: " + PlayerStatManager.Mana + "\n";
        m_Text.text += "\n";

        foreach (Status status in Enum.GetValues(typeof(Status))) {
            m_Text.text += status + ": " + PlayerStatusManager.GetRemainingFrames(status) + "\n";
        }
    }
    */
}
