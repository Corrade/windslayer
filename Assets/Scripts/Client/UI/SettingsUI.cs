using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

using Windslayer;

namespace Windslayer.Client
{
    public class SettingsUI : MonoBehaviour
    {
        [SerializeField]
        c_LobbyManager Lobby;

        Text m_Text;

        void Awake()
        {
            m_Text = GetComponent<Text>();
            Render(this, EventArgs.Empty);
        }

        void Start()
        {
            Lobby.OnSettingsChange += Render;
        }

        void Render(object sender, EventArgs e)
        {
            string value = "Lobby Settings\n";

            value += "MaxPlayers = " + Lobby.Settings.MaxPlayers + "\n";
            value += "MapID = " + Lobby.Settings.MapID + "\n";
            value += "RespawnTime = " + Lobby.Settings.RespawnTime + "\n";
            value += "KillLimit = " + Lobby.Settings.KillLimit + "\n";
            value += "TimeLimit = " + Lobby.Settings.TimeLimit + "\n";

            m_Text.text = value;
        }
    }
}
