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

        public LobbySettingsMsg Settings { get; private set; }
        public Map CurrentMap { get; private set; }

        XmlUnityServer m_XmlServer;

        Dictionary<IClient, PlayerManager> m_PlayerManagers = new Dictionary<IClient, PlayerManager>();
        List<IClient> m_HostOrder = new List<IClient>();

        List<Team> m_Teams = new List<Team>( new Team[TeamIDs.Count] );
        Team m_SpecTeam;

        EventHandler m_TimeLimitEvent;
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

        void StartGame()
        {
            CurrentMap = Instantiate(Maps[Settings.MapID], Vector2.zero, Quaternion.identity);

            m_TimeLimitEvent = Clock.Wait(Sync.Tickrate * (uint)Settings.TimeLimit, () => {
                Debug.Log("Time ran out");
                Timeout();
            });

            foreach (Team team in m_Teams) {
                team.ResetKills();

                foreach (PlayerManager playerManager in team.Players.Values) {
                    if (playerManager != null) {
                        Debug.Log("Player should not be instantiated before the start of the game");
                    }

                    playerManager.Spawn();
                }
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
                return team.Size() >= 1;
            });

            if (teamsWithPlayers.Count == 1) {
                EndGame(teamsWithPlayers[0]);
            } else if (teamsWithPlayers.Count == 0) {
                EndGame(null);
            }
        }

        // If a team has exceeded the total kill limit, they win. Otherwise nothing happens.
        void CheckEndByKills(object sender, EventArgs e)
        {
            if (!m_GameStarted) {
                return;
            }

            foreach (Team team in m_Teams) {
                if (team.TotalKills >= Settings.KillLimit) {
                    EndGame(team);
                }
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

            Clock.StopWait(m_TimeLimitEvent);

            foreach (Team team in m_Teams) {
                foreach (PlayerManager playerManager in team.Players.Values) {
                    if (playerManager == null) {
                        Debug.Log("Team should not have a null player at the end of a game");
                    }

                    playerManager.Despawn();
                }
            }

            Debug.Log("Game ended");
        }

        void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            GameObject p = Instantiate(PlayerManagerPrefab, Vector2.zero, Quaternion.identity);

            PlayerManager manager = p.GetComponent<PlayerManager>();
            manager.Initialise(e.Client, m_XmlServer.Server, this);

            m_HostOrder.Add(e.Client);

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
            m_Teams[m_PlayerManagers[e.Client].TeamID].RemoveByDisconnect(e.Client);
            m_PlayerManagers.Remove(e.Client);
            m_HostOrder.Remove(e.Client);

            using (Message msg = Message.Create(
                Tags.DespawnPlayer,
                new DespawnPlayerMsg(e.Client.ID)
            )) {
                foreach (IClient client in m_XmlServer.Server.ClientManager.GetAllClients().Where(x => x != e.Client)) {
                    client.SendMessage(msg, SendMode.Reliable);
                }
            }

            CheckEndByDisconnect();
        }
    }
}
