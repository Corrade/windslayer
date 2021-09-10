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
        GameObject PlayerClientPrefab;
        
        [SerializeField]
        List<Map> Maps = new List<Map>( new Map[MapIDs.Count] );

        XmlUnityServer m_XmlServer;

        LobbySettingsMsg m_Settings;
        Dictionary<IClient, GameObject> m_PlayerClients = new Dictionary<IClient, GameObject>();
        List<Team> m_Teams = new List<Team>( new Team[TeamIDs.CountNoSpec] );
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

            m_XmlServer.Server.ClientManager.ClientConnected += ClientConnected;
            m_XmlServer.Server.ClientManager.ClientDisconnected += ClientDisconnected;
        }

        // Replaces m_Settings with the valid fields of settings (a client should never send invalid settings unless they're malicious, so there's no need for error messages)
        void ReplaceLobbySettings(LobbySettingsMsg settings)
        {
            if (settings.MaxPlayers < 2 || settings.MaxPlayers % 2 != 0 || settings.MaxPlayers < m_PlayerClients.Count) {
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
                m_Teams[i].OnTotalKillsChange += CheckEndByKills;
                m_CurrentMap.MoveTeamToSpawn(i, m_Teams[i]);
            }

            m_GameStarted = true;
        }

        void Timeout()
        {
            if (!m_GameStarted) {
                return;
            }

            List<ushort> teamIDsWithMostKills = new List<ushort>();
            int mostKills = -1;

            for (ushort i = 0; i < m_Teams.Count; ++i) {
                if (m_Teams[i].TotalKills == mostKills) {
                    teamIDsWithMostKills.Add(i);
                } else if (m_Teams[i].TotalKills > mostKills) {
                    teamIDsWithMostKills.Clear();
                    teamIDsWithMostKills.Add(i);
                    mostKills = m_Teams[i].TotalKills;
                }
            }

            if (teamIDsWithMostKills.Count == 1) {
                EndGame(m_Teams[teamIDsWithMostKills[0]]);
            } else {
                EndGame(null);
            }
        }

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
            GameObject playerClient = Instantiate(PlayerClientPrefab, Vector2.zero, Quaternion.identity);

            if (player.activeSelf) {
                Debug.LogError("Player should not begin active");
            }

            PlayerConnectionManager conn = player.GetComponent<PlayerConnectionManager>();
            conn.Initialise(e.Client.ID, e.Client, m_XmlServer.Server);
            m_PlayerClients.Add(e.Client, player);

            playerClient.SetActive(true);

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
                foreach (GameObject p in m_PlayerClients.Values) {
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
            m_PlayerClients.Remove(e.Client);

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
