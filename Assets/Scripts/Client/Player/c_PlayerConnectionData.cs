using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

using Windslayer;

namespace Windslayer.Client
{
    public class c_PlayerConnectionData : MonoBehaviour
    {
        public ushort ClientID { get; private set; }
        public UnityClient Client { get; private set; }
        public c_LobbyManager Lobby { get; private set; }
        public ushort TeamID { get; set; }

        public void Initialise(ushort clientID, UnityClient client, c_LobbyManager lobby, ushort teamID)
        {
            ClientID = clientID;
            Client = client;
            Lobby = lobby;
            TeamID = teamID;
        }
    }
}
