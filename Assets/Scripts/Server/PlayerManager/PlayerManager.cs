using System;
using System.Linq;
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

        public IClient Client { get; private set; }
        public DarkRiftServer Server { get; private set; }

        ushort _TeamID = TeamIDs.InvalidTeamID;
        public ushort TeamID {
            get {
                return _TeamID;
            }

            set {
                _TeamID = value;

                if (PlayerIsSpawned()) {
                    PlayerConnectionData conn = Player.GetComponent<PlayerConnectionData>();
                    conn.TeamID = value;
                }
            }
        }

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

            if (!TeamIDs.IsValid(TeamID)) {
                Debug.LogError("Player not without being in a team");
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
            conn.Initialise(Metadata.ClientID, Client, Server, m_Lobby, TeamID);

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
                foreach (IClient client in Server.ClientManager.GetAllClients()) {
                    client.SendMessage(message, SendMode.Reliable);
                }

                // Send all game information to the new client (all players metadata (excluding the player itself as that was just communicated), start game, spawn all other players, all players' teams, win conditions, lobby settings)
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
    }
}
