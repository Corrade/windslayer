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
    public class c_LobbyManager : MonoBehaviour
    {
        [SerializeField]
        GameObject ControllablePlayerPrefab;
        [SerializeField]
        GameObject NonControllablePlayerPrefab;

        [SerializeField]
        c_MainCamera Cam;

        [SerializeField]
        UnityClient Client;

        [SerializeField]
        List<GameObject> Maps = new List<GameObject>( new GameObject[MapIDs.Count] );

        public LobbySettingsMsg Settings { get; private set; }
        public ushort CurrentMapId { get; private set; }

        Dictionary<ushort, c_PlayerManager> m_PlayerManagers = new Dictionary<ushort, c_PlayerManager>();
        // List<c_PlayerManager> m_HostOrder = new List<c_PlayerManager>();
        List<c_Team> m_Teams = new List<c_Team>( new c_Team[TeamIDs.Count] );

        int m_TimeLeft;
        bool m_GameStarted = false;

        void Awake()
        {
            if (Maps.Count != MapIDs.Count) {
                Debug.LogError("Map(s) unimplemented");
                Application.Quit();
            }

            Client.MessageReceived += MessageReceived;

            // Initialise self to server
            using (Message msg = Message.Create(
                Tags.PlayerMetadata,
                new PlayerMetadataMsg(Client.ID, "asdasdf")
            )) {
                Client.SendMessage(msg, SendMode.Reliable);
            }
        }

        void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message) {
                if (message.Tag == Tags.PlayerMetadata) {
                    PlayerMetadata(sender, e);
                } else if (message.Tag == Tags.DisconnectPlayer) {
                    DisconnectPlayer(sender, e);
                } else if (message.Tag == Tags.LobbySettings) {
                    LobbySettings(sender, e);
                } else if (message.Tag == Tags.StartGame) {
                    StartGame(sender, e);
                } else if (message.Tag == Tags.EndGame) {
                    EndGame(sender, e);
                } else if (message.Tag == Tags.TeamDeclaration) {
                    TeamDeclaration(sender, e);
                } else if (message.Tag == Tags.WinConditionsState) {
                    WinConditionsState(sender, e);
                } else if (message.Tag == Tags.SpawnPlayer) {
                    SpawnPlayer(sender, e);
                } else if (message.Tag == Tags.DespawnPlayer) {
                    DespawnPlayer(sender, e);
                } else if (message.Tag == Tags.MovePlayer) {
                    MovePlayer(sender, e);
                }
            }
        }

        void PlayerMetadata(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                PlayerMetadataMsg msg = message.Deserialize<PlayerMetadataMsg>();

                if (e.Client.ID == Client.ID) {
                    // Register self
                    GameObject p = Instantiate(ControllablePlayerPrefab, Vector2.zero, Quaternion.identity);

                    c_PlayerManager playerManager = p.GetComponent<c_PlayerManager>();
                    playerManager.Initialise(e.Client, this);

                    m_PlayerManagers.Add(e.Client.ID, playerManager);
                } else {
                    // Register other
                    GameObject p = Instantiate(NonControllablePlayerPrefab, Vector2.zero, Quaternion.identity);

                    c_PlayerManager playerManager = p.GetComponent<c_PlayerManager>();
                    playerManager.Initialise(e.Client, this);

                    m_PlayerManagers.Add(e.Client.ID, playerManager);
                }
            }
        }

        void DisconnectPlayer(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                DisconnectPlayerMsg msg = message.Deserialize<DisconnectPlayerMsg>();

                c_PlayerManager playerManager = m_PlayerManagers[msg.ClientID];
                ushort teamID = playerManager.TeamID;

                if (TeamIDs.IsValid(teamID)) {
                    m_Teams[teamID].Remove(e.Client);
                }

                m_PlayerManagers.Remove(msg.ClientID);
            }
        }

        void LobbySettings(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                LobbySettingsMsg msg = message.Deserialize<LobbySettingsMsg>();
            }
        }

        void StartGame(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                StartGameMsg msg = message.Deserialize<StartGameMsg>();
            }
        }

        void EndGame(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                EndGameMsg msg = message.Deserialize<EndGameMsg>();
            }
        }

        void TeamDeclaration(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                TeamDeclarationMsg msg = message.Deserialize<TeamDeclarationMsg>();
            }
        }

        void WinConditionsState(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                WinConditionsStateMsg msg = message.Deserialize<WinConditionsStateMsg>();
            }
        }

        void SpawnPlayer(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                SpawnPlayerMsg msg = message.Deserialize<SpawnPlayerMsg>();

                // Destroy(m_Players[msg.ClientID].gameObject);
                // m_Players.Remove(msg.ClientID);
            }
        }

        void DespawnPlayer(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                DespawnPlayerMsg msg = message.Deserialize<DespawnPlayerMsg>();

                // Destroy(m_Players[msg.ClientID].gameObject);
                // m_Players.Remove(msg.ClientID);
            }
        }

        void MovePlayer(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                MovePlayerMsg msg = message.Deserialize<MovePlayerMsg>();

                /*
                m_Players[msg.ClientID].gameObject.transform.position = msg.Position;
                if (msg.IsFacingLeft) {
                    m_Players[msg.ClientID].gameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
                } else {
                    m_Players[msg.ClientID].gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
                }
                */
            }
        }
    }
}
