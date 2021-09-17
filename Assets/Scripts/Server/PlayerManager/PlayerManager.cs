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
            Player = Instantiate(PlayerPrefab, m_Lobby.CurrentMap.GetRandomSpawn(TeamID), Quaternion.identity);

            if (Player.activeSelf) {
                Debug.LogError("In-game player should not begin active");
            }

            PlayerConnectionData conn = Player.GetComponent<PlayerConnectionData>();
            conn.Initialise(Client.ID, Client, Server, m_Lobby);

            Player.SetActive(true);

            // Broadcast
        }

        public void Despawn()
        {
            Destroy(Player);
            Player = null;

            // Broadcast
        }

        void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message) {
                if (message.Tag == Tags.PlayerMetadata) {
                    MetadataUpdate(sender, e);
                } else if (message.Tag == Tags.LobbySettings) {
                    ConfigureLobby(sender, e);
                } else if (message.Tag == Tags.GameStart) {
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

                // Ensure that the client is switching themselves
                if (e.Client.ID != msg.ClientID || Metadata != null) {
                    return;
                }

                m_Lobby.ProposeTeamJoin(this, msg.TeamID, message);
            }
        }
    }
}
