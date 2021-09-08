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
    public class Team : MonoBehaviour
    {
        public event EventHandler OnTeamSizeChange;

        XmlUnityServer m_XmlServer;

        Dictionary<IClient, PlayerConnectionManager> m_Players = new Dictionary<IClient, PlayerConnectionManager>();

        void Awake()
        {
            m_XmlServer.Server.ClientManager.ClientDisconnected += ClientDisconnected;
        }

        public int Size()
        {
            return m_Players.Keys.Count;
        }

        public int TotalKills()
        {
            int total = 0;

            foreach (PlayerConnectionManager player in m_Players.Values) {
                PlayerStatManager stat = player.GetComponent<PlayerStatManager>();
                total += stat.Kills;
            }

            return total;
        }

        public void Add(IClient client, PlayerConnectionManager player)
        {
            m_Players.Add(client, player);
            OnTeamSizeChange?.Invoke(this, EventArgs.Empty);
        }

        public void Remove(IClient client, PlayerConnectionManager player)
        {
            m_Players.Remove(client);
            OnTeamSizeChange?.Invoke(this, EventArgs.Empty);
        }

        void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            m_Players.Remove(e.Client);
            OnTeamSizeChange?.Invoke(this, EventArgs.Empty);
        }
    }
}
