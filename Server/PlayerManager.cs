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

            players.Add(e.Client, player);

            // at some point, broadcast new player to all existing players, and vice versa
        }

        void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            players.Remove(e.Client);
        }
    }
}
