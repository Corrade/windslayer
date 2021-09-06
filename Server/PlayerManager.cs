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
        PlayerConnectionManager PlayerPrefab;
        
        XmlUnityServer m_XmlServer;

        Dictionary<IClient, PlayerConnectionManager> players = new Dictionary<IClient, PlayerConnectionManager>();

        void Awake()
        {
            m_XmlServer = GetComponent<XmlUnityServer>();

            if (m_XmlServer == null)
            {
                Debug.LogError("Server unassigned in AgarPlayerManager.");
                Application.Quit();
            }

            if (m_XmlServer.Server == null)
            {
                Debug.LogError("Server not open yet - check script execution order for XmlUnityServer.");
                Application.Quit();
            }
        
            m_XmlServer.Server.ClientManager.ClientConnected += ClientConnected;
            m_XmlServer.Server.ClientManager.ClientDisconnected += ClientDisconnected;
        }

        void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            PlayerConnectionManager player = Instantiate(PlayerPrefab, Vector2.zero, Quaternion.identity) as PlayerConnectionManager;
            player.Initialise(e.Client.ID, e.Client, m_XmlServer.Server);

            // consider instantiating as inactive and then setting active, ensuring scripts will have conn initialised

            players.Add(e.Client, player);

            // Broadcast the new player to all existing players
            using (DarkRiftWriter w = DarkRiftWriter.Create()) // Use 'using' for performance reasons
            {
                w.Write(e.Client.ID);
                w.Write(0f);
                w.Write(0f);

                using (Message m = Message.Create(Tags.SpawnPlayer, w)) {
                    foreach (IClient client in m_XmlServer.Server.ClientManager.GetAllClients().Where(x => x != e.Client)) {
                        client.SendMessage(m, SendMode.Reliable);
                    }
                }
            }

            // Broadcast all players (including the new player itself) to the new player
            using (DarkRiftWriter w = DarkRiftWriter.Create()) {
                foreach (PlayerConnectionManager p in players.Values) {
                    w.Write(p.ClientID);
                    w.Write(0f);
                    w.Write(0f);
                }

                using (Message m = Message.Create(Tags.SpawnPlayer, w)) {
                    e.Client.SendMessage(m, SendMode.Reliable);
                }
            }
        }

        void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            players.Remove(e.Client);
        }
    }
}
