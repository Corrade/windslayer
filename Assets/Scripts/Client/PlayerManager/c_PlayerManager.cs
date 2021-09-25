using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

using Windslayer;

namespace Windslayer.Client
{
    public class c_PlayerManager : MonoBehaviour
    {
        /*
        [SerializeField]
        GameObject PlayerPrefab;

        public UnityClient Client { get; private set; }

        public ushort TeamID { get; set; }
        public PlayerMetadataMsg Metadata { get; private set; } = null;
        public GameObject Player { get; private set; } = null;

        LobbyManager m_Lobby;

        public void Initialise(IClient client, DarkRiftServer server, LobbyManager lobby)
        {
            Client = client;
            Server = server;
            m_Lobby = lobby;

            Client.MessageReceived += MessageReceived;
        }

        public bool PlayerIsSpawned()
        {
            return Player != null;
        }

        public void Spawn()
        {
            if (Metadata == null) {
                Debug.LogError("Player metadata not registered before spawning");
                return;
            }

            if (PlayerIsSpawned()) {
                Debug.Log("Trying to spawn a player that's already spawned");
                return;
            }

            Vector3 spawnPos = m_Lobby.CurrentMap.GetRandomSpawn(TeamID);
            Player = Instantiate(PlayerPrefab, spawnPos, Quaternion.identity);

            if (Player.activeSelf) {
                Debug.LogError("In-game player should not begin active");
            }

            PlayerConnectionData conn = Player.GetComponent<PlayerConnectionData>();
            conn.Initialise(Metadata.ClientID, Client, Server, m_Lobby);

            Player.SetActive(true);

            // Broadcast spawn
            using (Message msg = Message.Create(
                Tags.SpawnPlayer,
                new SpawnPlayerMsg(Metadata.ClientID, spawnPos)
            )) {
                foreach (IClient client in Server.ClientManager.GetAllClients()) {
                    client.SendMessage(msg, SendMode.Reliable);
                }
            }
        }

        public void Despawn()
        {
            if (Metadata == null) {
                Debug.LogError("Player metadata not registered before despawning");
                return;
            }

            if (!PlayerIsSpawned()) {
                Debug.Log("Trying to despawn a player that's already despawned");
                return;
            }

            Destroy(Player);
            Player = null;

            // Broadcast despawn
            using (Message msg = Message.Create(
                Tags.DespawnPlayer,
                new DespawnPlayerMsg(Metadata.ClientID)
            )) {
                foreach (IClient client in Server.ClientManager.GetAllClients()) {
                    client.SendMessage(msg, SendMode.Reliable);
                }
            }
        }

        void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message) {
                if (message.Tag == Tags.PlayerMetadata) {
                    MetadataUpdate(sender, e);
                } else if (message.Tag == Tags.LobbySettings) {
                    ConfigureLobby(sender, e);
                } else if (message.Tag == Tags.StartGame) {
                    StartGame(sender, e);
                } else if (message.Tag == Tags.TeamDeclaration) {
                    DeclareTeam(sender, e);
                }
            }
        }

        void MetadataUpdate(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                PlayerMetadataMsg msg = message.Deserialize<PlayerMetadataMsg>();

                // Ensure that the client is initialising themselves
                if (e.Client.ID != msg.ClientID || Metadata != null) {
                    return;
                }

                Metadata = msg;

                // Broadcast the new player to all existing players
                foreach (IClient client in Server.ClientManager.GetAllClients().Where(x => x != e.Client)) {
                    client.SendMessage(message, SendMode.Reliable);
                }

                // Send all game information to the new client (other players, teams, scores, etc.)
                // ...
            }
        }

        void ConfigureLobby(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                if (Metadata != null) {
                    LobbySettingsMsg msg = message.Deserialize<LobbySettingsMsg>();

                    m_Lobby.ProposeNewSettings(Metadata.ClientID, msg);
                }
            }
        }

        void StartGame(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                if (Metadata != null) {
                    m_Lobby.ProposeStartGame(Metadata.ClientID);
                }
            }
        }

        void DeclareTeam(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                TeamDeclarationMsg msg = message.Deserialize<TeamDeclarationMsg>();

                m_Lobby.ProposeTeamDeclare(this, msg.TeamID, message);
            }
        }
        */
    }
}
