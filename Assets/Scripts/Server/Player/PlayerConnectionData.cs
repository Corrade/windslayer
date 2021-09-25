using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;

using Windslayer;

namespace Windslayer.Server
{
    public class PlayerConnectionData : MonoBehaviour
    {
        public ushort ClientID { get; private set; }
        public IClient Client { get; private set; }
        public DarkRiftServer Server { get; private set; }
        public LobbyManager Lobby { get; private set; }
        public ushort TeamID { get; set; }

        public void Initialise(ushort clientID, IClient client, DarkRiftServer server, LobbyManager lobby, ushort teamID)
        {
            ClientID = clientID;
            Client = client;
            Server = server;
            Lobby = lobby;
            TeamID = teamID;
        }
    }
}
