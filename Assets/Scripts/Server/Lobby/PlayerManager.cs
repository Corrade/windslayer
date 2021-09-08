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
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField]
        GameObject PlayerPrefab;
        
        [SerializeField]
        List<Map> Maps = new List<Map>( new Map[MapIDs.Count] );

        XmlUnityServer m_XmlServer;

        Dictionary<IClient, GameObject> m_Players = new Dictionary<IClient, GameObject>();
        List<Team> m_Teams = new List<Team>( new Team[TeamIDs.CountNoSpec] );
        Map m_CurrentMap;

        int m_TimeLeft;

        LobbySettingsMsg m_Settings;

        void Awake()
        {
            if (Maps.Count != MapIDs.Count) {
                Debug.LogError("Map(s) unimplemented");
                Application.Quit();
            }

            m_XmlServer = GetComponent<XmlUnityServer>();

            if (m_XmlServer == null) {
                Debug.LogError("Server unassigned in AgarPlayerManager.");
                Application.Quit();
            }

            if (m_XmlServer.Server == null) {
                Debug.LogError("Server not open yet - check script execution order for XmlUnityServer.");
                Application.Quit();
            }

            m_XmlServer.Server.ClientManager.ClientConnected += ClientConnected;
            m_XmlServer.Server.ClientManager.ClientDisconnected += ClientDisconnected;
        }

        void StartGame()
        {
            /*
            if (m_Teams[TeamIDs.Blue].Size() < 1 || m_Teams[TeamIDs.Red].Size() < 1) {
                return;
            }
            */

            m_CurrentMap = Instantiate(Maps[m_Settings.MapID], Vector2.zero, Quaternion.identity);

            m_CurrentMap.MoveTeamToSpawn(TeamIDs.Blue, m_Teams[TeamIDs.Blue]);

            TimeLeft = m_Settings.TimeLimit;

            foreach (GameObject player in m_Players.Values) {
                player.SetActive(true);
            }

            for (ushort i = 0; i < TeamIDs.CountNoSpec; ++i) {
                m_Teams[i].OnTotalKillsChange += ProcessKill();
            }

            // set timer
            // hook into kills. end game when kills or timer
        }

        void ProcessKill(object sender, EventArgs e)
        {
            int totalKills = 0;
            
            for (ushort i = 0; i < TeamIDs.CountNoSpec; ++i) {
                totalKills += m_Teams[i].TotalKills;
            }

            // Transmit

            if (totalKills >= LobbySettingsMsg.KillLimit) {
                EndGame();
            }
        }

        void EndGame()
        {

        }

        void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            GameObject player = Instantiate(PlayerPrefab, Vector2.zero, Quaternion.identity);

            if (player.activeSelf) {
                Debug.LogError("Player should not begin active");
            }

            PlayerConnectionManager conn = player.GetComponent<PlayerConnectionManager>();
            conn.Initialise(e.Client.ID, e.Client, m_XmlServer.Server);
            m_Players.Add(e.Client, player);

            // Broadcast the new player to all existing players
            using (Message msg = Message.Create(
                Tags.SpawnPlayer,
                new SpawnPlayerMsg(e.Client.ID, Vector3.zero)
            )) {
                foreach (IClient client in m_XmlServer.Server.ClientManager.GetAllClients().Where(x => x != e.Client)) {
                    client.SendMessage(msg, SendMode.Reliable);
                }
            }

            // Broadcast all players (including the new player itself) to the new player
            using (DarkRiftWriter w = DarkRiftWriter.Create()) {
                foreach (GameObject p in m_Players.Values) {
                    PlayerConnectionManager c = p.GetComponent<PlayerConnectionManager>();
                    w.Write(new SpawnPlayerMsg(c.ClientID, Vector3.zero));
                }

                using (Message msg = Message.Create(Tags.SpawnPlayer, w)) {
                    e.Client.SendMessage(msg, SendMode.Reliable);
                }
            }
        }

        void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            m_Players.Remove(e.Client);

            using (Message msg = Message.Create(
                Tags.DespawnPlayer,
                new DespawnPlayerMsg(e.Client.ID)
            )) {
                foreach (IClient client in m_XmlServer.Server.ClientManager.GetAllClients().Where(x => x != e.Client)) {
                    client.SendMessage(msg, SendMode.Reliable);
                }
            }
        }
    }
}
