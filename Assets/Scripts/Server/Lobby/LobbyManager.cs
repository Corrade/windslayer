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
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField]
        GameObject PlayerManagerPrefab;
        
        [SerializeField]
        List<Map> Maps = new List<Map>( new Map[MapIDs.Count] );

        // Only accept changes when !m_GameStarted
        public LobbySettingsMsg Settings { get; private set; }
        public Map CurrentMap { get; private set; }

        XmlUnityServer m_XmlServer;

        Dictionary<IClient, PlayerManager> m_PlayerManagers = new Dictionary<IClient, PlayerManager>();
        List<PlayerManager> m_HostOrder = new List<PlayerManager>();
        List<Team> m_Teams = new List<Team>( new Team[TeamIDs.Count] );

        int m_TimeLeftTicks;
        EventHandler m_TimerListener;
        bool m_GameStarted = false;

        void Awake()
        {
            if (Maps.Count != MapIDs.Count) {
                Debug.LogError("Map(s) unimplemented");
                Application.Quit();
            }

            m_XmlServer = GetComponent<XmlUnityServer>();

            if (m_XmlServer == null) {
                Debug.LogError("Server unassigned");
                Application.Quit();
            }

            if (m_XmlServer.Server == null) {
                Debug.LogError("Server not open yet - check script execution order for XmlUnityServer");
                Application.Quit();
            }

            for (int i = 0; i < TeamIDs.Count; ++i) {
                m_Teams[i] = new Team();
                m_Teams[i].OnTotalKillsChange += CheckEndByKills;
            }

            m_XmlServer.Server.ClientManager.ClientConnected += ClientConnected;
            m_XmlServer.Server.ClientManager.ClientDisconnected += ClientDisconnected;
        }

        public void ProposeNewSettings(ushort clientID, LobbySettingsMsg newSettings)
        {
            if (IsClientHost(clientID)) {
                Settings.Replace(newSettings, m_PlayerManagers.Count);

                // Broadcast new settings
                using (Message msg = Message.Create(
                    Tags.LobbySettings,
                    Settings
                )) {
                    foreach (IClient client in m_XmlServer.Server.ClientManager.GetAllClients()) {
                        client.SendMessage(msg, SendMode.Reliable);
                    }
                }
            }
        }

        public void ProposeStartGame(ushort clientID)
        {
            if (IsClientHost(clientID)) {
                StartGame();
            }
        }
        
        public void ProposeTeamJoin(PlayerManager playerManager, ushort teamID, Message msg)
        {
            // Validate team ID and do nothing if the player is already on the specified team
            if (playerManager.TeamID == teamID || !TeamIDs.IsValid(teamID)) {
                return;
            }

            // if the difference between the new team size of the proposed team the player is joining and any other team is greater than whatever, don't allow it

            if (m_GameStarted) {
                playerManager.Despawn();
            }

            // Remove from their current team if such a team exists
            if (TeamIDs.IsValid(playerManager.TeamID)) {
                m_Teams[playerManager.TeamID].Remove(playerManager.Client);
            }

            m_Teams[teamID].Add(playerManager.Client, playerManager);

            playerManager.TeamID = teamID;

            // Broadcast team declaration
            foreach (IClient client in m_XmlServer.Server.ClientManager.GetAllClients()) {
                client.SendMessage(msg, SendMode.Reliable);
            }

            if (m_GameStarted) {
                // Perhaps place a delay here
                // ...

                playerManager.Spawn();
            }
        }

        void StartGame()
        {
            CurrentMap = Instantiate(Maps[Settings.MapID], Vector2.zero, Quaternion.identity);

            m_TimeLeftTicks = Settings.TimeLimit * (int)Sync.Tickrate;
            m_TimerListener = Clock.CallEveryTick(() => {
                --m_TimeLeftTicks;

                if (m_TimeLeftTicks <= 0) {
                    Debug.Log("Time ran out");
                    Timeout();
                }
            });

            foreach (Team team in m_Teams) {
                team.ResetKills();
                team.Spawn();
            }

            m_GameStarted = true;
        }

        // Should be called when times runs out. If there is a team with strictly the most kills, they win. Otherwise the game draws.
        void Timeout()
        {
            if (!m_GameStarted) {
                return;
            }

            List<Team> topTeams = new List<Team>();
            int mostKills = -1;

            foreach (Team team in m_Teams) {
                if (team.TotalKills == mostKills) {
                    topTeams.Add(team);
                } else if (team.TotalKills > mostKills) {
                    topTeams.Clear();
                    topTeams.Add(team);
                    mostKills = team.TotalKills;
                }
            }

            if (topTeams.Count == 1) {
                EndGame(topTeams[0]);
            } else {
                EndGame(null);
            }
        }

        // If there is only one team left with a player, they win. If there are no teams with any players, the game draws. Otherwise nothing happens.
        void CheckEndByDisconnect()
        {
            if (!m_GameStarted) {
                return;
            }

            List<Team> teamsWithPlayers = m_Teams.FindAll((Team team) => {
                return team.Count() >= 1;
            });

            if (teamsWithPlayers.Count == 1) {
                EndGame(teamsWithPlayers[0]);
            } else if (teamsWithPlayers.Count == 0) {
                EndGame(null);
            }
        }

        // If there is only one team that has exceeded the total kill limit, they win. If there are more teams that do this, the game draws. Otherwise nothing. Otherwise nothing happens.
        void CheckEndByKills(object sender, EventArgs e)
        {
            if (!m_GameStarted) {
                return;
            }

            List<Team> teamsWithEnoughKills = m_Teams.FindAll((Team team) => {
                return team.TotalKills >= Settings.KillLimit;
            });

            if (teamsWithEnoughKills.Count == 1) {
                EndGame(teamsWithEnoughKills[0]);
            } else if (teamsWithEnoughKills.Count > 1) {
                EndGame(null);
            }
        }

        void EndGame(Team winningTeam)
        {
            m_GameStarted = false;

            if (winningTeam == null) {
                Debug.Log("Draw");
            } else {
                Debug.Log("Won");
            }

            Clock.RemoveListener(m_TimerListener);

            foreach (Team team in m_Teams) {
                team.Despawn();
            }

            Debug.Log("Game ended");
        }

        void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            GameObject p = Instantiate(PlayerManagerPrefab, Vector2.zero, Quaternion.identity);

            PlayerManager manager = p.GetComponent<PlayerManager>();
            manager.Initialise(e.Client, m_XmlServer.Server, this);

            m_HostOrder.Add(manager);

            /*
            broadcast just the player client here

            // Broadcast the new player to all existing players
            using (Message msg = Message.Create(
                Tags.SpawnPlayer,
                new SpawnPlayerMsg(e.Client.ID)
            )) {
                foreach (IClient client in m_XmlServer.Server.ClientManager.GetAllClients().Where(x => x != e.Client)) {
                    client.SendMessage(msg, SendMode.Reliable);
                }
            }

            // Broadcast all players (including the new player itself) to the new player
            using (DarkRiftWriter w = DarkRiftWriter.Create()) {
                foreach (GameObject p in m_PlayerManagers.Values) {
                    PlayerConnectionManager c = p.GetComponent<PlayerConnectionManager>();
                    w.Write(new SpawnPlayerMsg(c.ClientID));
                }

                using (Message msg = Message.Create(Tags.SpawnPlayer, w)) {
                    e.Client.SendMessage(msg, SendMode.Reliable);
                }
            }
            */
        }

        void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            PlayerManager playerManager = m_PlayerManagers[e.Client];
            uint playerTeamID = playerManager.TeamID;

            if (playerTeamID >= 0) {
                m_Teams[(int)playerTeamID].Remove(e.Client);
            }

            // Inform all other clients of the disconnection
            using (Message msg = Message.Create(
                Tags.DisconnectPlayer,
                new DisconnectPlayerMsg(playerManager.Metadata.ClientID)
            )) {
                foreach (IClient client in m_XmlServer.Server.ClientManager.GetAllClients().Where(x => x != e.Client)) {
                    client.SendMessage(msg, SendMode.Reliable);
                }
            }

            m_HostOrder.Remove(playerManager);
            m_PlayerManagers.Remove(e.Client);

            CheckEndByDisconnect();
        }
        
        bool IsClientHost(ushort clientID)
        {
            if (m_HostOrder.Count == 0 || m_HostOrder[0].Metadata == null) {
                return false;
            }

            return m_HostOrder[0].Metadata.ClientID == clientID;
        }
    }
}
