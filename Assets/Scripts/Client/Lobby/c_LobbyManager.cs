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
        public bool GameStarted { get; private set; } = false;

        Dictionary<ushort, c_PlayerManager> m_PlayerManagers = new Dictionary<ushort, c_PlayerManager>();
        // List<c_PlayerManager> m_HostOrder = new List<c_PlayerManager>();
        List<c_Team> m_Teams = new List<c_Team>( new c_Team[TeamIDs.Count] );

        int m_TimeLeft;

        void Awake()
        {
            if (Maps.Count != MapIDs.Count) {
                Debug.LogError("Map(s) unimplemented");
                Application.Quit();
            }

            Client.MessageReceived += MessageReceived;

            for (int i = 0; i < TeamIDs.Count; ++i) {
                m_Teams[i] = new c_Team((ushort)i);
            }

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

                GameObject p;

                if (msg.ClientID == Client.ID) {
                    // Register self
                    p = Instantiate(ControllablePlayerPrefab, Vector2.zero, Quaternion.identity);
                } else {
                    // Register other
                    p = Instantiate(NonControllablePlayerPrefab, Vector2.zero, Quaternion.identity);
                }

                c_PlayerManager playerManager = p.GetComponent<c_PlayerManager>();
                playerManager.Initialise(Client, this, msg);

                m_PlayerManagers.Add(msg.ClientID, playerManager);
            }
        }

        void DisconnectPlayer(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                DisconnectPlayerMsg msg = message.Deserialize<DisconnectPlayerMsg>();

                c_PlayerManager playerManager = m_PlayerManagers[msg.ClientID];
                ushort teamID = playerManager.TeamID;

                if (TeamIDs.IsValid(teamID)) {
                    m_Teams[teamID].Remove(msg.ClientID);
                }

                m_PlayerManagers.Remove(msg.ClientID);
            }
        }

        void LobbySettings(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                LobbySettingsMsg msg = message.Deserialize<LobbySettingsMsg>();
                Settings = msg;
            }
        }

        void StartGame(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                StartGameMsg msg = message.Deserialize<StartGameMsg>();
                Debug.Log("Game started on client");

                GameStarted = true;
            }
        }

        void EndGame(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                EndGameMsg msg = message.Deserialize<EndGameMsg>();
                Debug.Log("Game ended on client");

                GameStarted = false;
            }
        }

        void TeamDeclaration(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                TeamDeclarationMsg msg = message.Deserialize<TeamDeclarationMsg>();

                // Remove from their current team if such a team exists
                if (TeamIDs.IsValid(m_PlayerManagers[msg.ClientID].TeamID)) {
                    m_Teams[msg.TeamID].Remove(msg.ClientID);
                }

                m_Teams[msg.TeamID].Add(m_PlayerManagers[msg.ClientID]);

                m_PlayerManagers[msg.ClientID].TeamID = msg.TeamID;
            }
        }

        void WinConditionsState(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                WinConditionsStateMsg msg = message.Deserialize<WinConditionsStateMsg>();

                for (int i = 0; i < msg.TeamTotalKills.Count(); ++i) {
                    m_Teams[i].TotalKills = msg.TeamTotalKills[i];
                }

                m_TimeLeft = msg.TimeLeft;
            }
        }

        void SpawnPlayer(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                SpawnPlayerMsg msg = message.Deserialize<SpawnPlayerMsg>();
                m_PlayerManagers[msg.ClientID].Spawn(msg.Position);
            }
        }

        void DespawnPlayer(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                DespawnPlayerMsg msg = message.Deserialize<DespawnPlayerMsg>();
                m_PlayerManagers[msg.ClientID].Despawn();
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
