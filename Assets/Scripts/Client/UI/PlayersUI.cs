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
    public class PlayersUI : MonoBehaviour
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
            Lobby.OnPlayerManagersChange += Render;
        }

        void Render(object sender, EventArgs e)
        {
            string value = "Players\n";
            
            foreach (c_PlayerManager p in Lobby.PlayerManagers.Values) {
                value += p.Metadata.Username + "\n";
            }

            m_Text.text = value;
        }
    }
}
