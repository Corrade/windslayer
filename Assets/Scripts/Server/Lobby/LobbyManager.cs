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

        XmlUnityServer m_XmlServer;

        LobbySettingsMsg m_Settings;

        Dictionary<IClient, GameObject> m_PlayerManagers = new Dictionary<IClient, GameObject>();
        List<IClient> m_HostOrder = new List<IClient>();

        List<Team> m_Teams = new List<Team>( new Team[TeamIDs.Count] );
        Team m_SpecTeam;

        Map m_CurrentMap;
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

        // Replaces m_Settings with the fields of settings that are valid. A client should never send invalid settings unless they're malicious, so there's no need for error messages. Also, since m_Settings begins with valid default values and only changes to valid values, it will always be valid.
        void ReplaceLobbySettings(LobbySettingsMsg settings)
        {
            if (settings.MaxPlayers < 2 || settings.MaxPlayers % 2 != 0 || settings.MaxPlayers < m_PlayerManagers.Count) {
                settings.MaxPlayers = m_Settings.MaxPlayers;
            }

            if (settings.MapID < 0 || settings.MapID >= MapIDs.Count) {
                settings.MapID = m_Settings.MapID;
            }

            if (settings.RespawnTime < 0) {
                settings.RespawnTime = m_Settings.RespawnTime;
            }

            if (settings.KillLimit < 0) {
                settings.KillLimit = m_Settings.KillLimit;
            }

            if (settings.TimeLimit < 0) {
                settings.TimeLimit = m_Settings.TimeLimit;
            }

            m_Settings = settings;
        }

        void StartGame()
        {
            m_CurrentMap = Instantiate(Maps[m_Settings.MapID], Vector2.zero, Quaternion.identity);

            m_TimeLimitEvent = Clock.Wait(Sync.Tickrate * (uint)m_Settings.TimeLimit, () => {
                Debug.Log("Time ran out");
                Timeout();
            });

            for (ushort i = 0; i < m_Teams.Count; ++i) {
                m_CurrentMap.MoveTeamToSpawn(i, m_Teams[i]);
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
                if (team.TotalKills >= m_Settings.KillLimit) {
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
                team.OnTotalKillsChange -= CheckEndByKills;
            }

            Debug.Log("Game ended");
        }

        void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            GameObject p = Instantiate(PlayerManagerPrefab, Vector2.zero, Quaternion.identity);

            PlayerManager manager = p.GetComponent<PlayerManager>();
            manager.Initialise(e.Client.ID, e.Client, m_XmlServer.Server);

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
