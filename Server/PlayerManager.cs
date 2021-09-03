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
        PlayerConnectionManager playerPrefab;
        
        XmlUnityServer XmlServer;

        Dictionary<IClient, PlayerConnectionManager> players = new Dictionary<IClient, PlayerConnectionManager>();

        void Awake()
        {
            XmlServer = GetComponent<XmlUnityServer>();

            if (XmlServer == null)
            {
                Debug.LogError("Server unassigned in AgarPlayerManager.");
                Application.Quit();
            }

            if (XmlServer.Server == null)
            {
                Debug.LogError("Server not open yet - check script execution order for XmlUnityServer.");
                Application.Quit();
            }
        
            XmlServer.Server.ClientManager.ClientConnected += ClientConnected;
            XmlServer.Server.ClientManager.ClientDisconnected += ClientDisconnected;
        }

        void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            PlayerConnectionManager player = Instantiate(playerPrefab, Vector2.zero, Quaternion.identity) as PlayerConnectionManager;
            player.ID = e.Client.ID;
            player.client = e.Client;
            player.XmlServer = XmlServer;

            players.Add(e.Client, player);

            // at some point, broadcast new player to all existing players, and vice versa
        }

        void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            players.Remove(e.Client);
        }
    }
}
