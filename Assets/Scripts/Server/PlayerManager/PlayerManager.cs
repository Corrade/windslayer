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
    // The player doesn't have an in-game representation when they're in spectator mode (any other cases?). Yet in this state the server still needs to track that player's data (e.g. player name), receive messages from them (e.g. lobby configuration, team select) and spawn their in-game object. Therefore the player needs a class that takes responsibility for these things and is tied to the lifetime of their connection itself.
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField]
        GameObject PlayerPrefab;

        public ushort ClientID { get; private set; }
        public IClient Client { get; private set; }
        public DarkRiftServer Server { get; private set; }
        public GameObject Player { get; private set; } = null;

        LobbyManager m_Lobby;
        ushort m_teamID;

        public void Initialise(ushort clientID, IClient client, DarkRiftServer server, LobbyManager lobby)
        {
            ClientID = clientID;
            Client = client;
            Server = server;
            m_Lobby = lobby;
        }

        public void Spawn()
        {
            Player = Instantiate(PlayerPrefab, m_Lobby.CurrentMap.GetRandomSpawn(m_teamID), Quaternion.identity);

            if (Player.activeSelf) {
                Debug.LogError("In-game player should not begin active");
            }

            PlayerConnectionData conn = Player.GetComponent<PlayerConnectionData>();
            conn.Initialise(ClientID, Client, Server, m_Lobby);

            Player.SetActive(true);
        }

        public void Despawn()
        {
            Destroy(Player);
            Player = null;
        }
    }
}
