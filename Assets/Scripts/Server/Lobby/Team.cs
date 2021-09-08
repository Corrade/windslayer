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
        public Dictionary<IClient, GameObject> Players { get; private set; } = new Dictionary<IClient, GameObject>();
        public int TotalKills { get; private set; } = 0;

        public event EventHandler OnTeamSizeChange;
        public event EventHandler OnTotalKillsChange;

        XmlUnityServer m_XmlServer;

        void Awake()
        {
            m_XmlServer.Server.ClientManager.ClientDisconnected += ClientDisconnected;
        }

        public void ResetKills()
        {
            TotalKills = 0;
        }

        public int Size()
        {
            return Players.Keys.Count;
        }

        public void Add(IClient client, GameObject player)
        {
            Players.Add(client, player);
            OnTeamSizeChange?.Invoke(this, EventArgs.Empty);

            PlayerStatManager stat = player.GetComponent<PlayerStatManager>();
            stat.OnDeath += AddDeath;
        }

        public void Remove(IClient client, GameObject player)
        {
            Players.Remove(client);
            OnTeamSizeChange?.Invoke(this, EventArgs.Empty);

            PlayerStatManager stat = player.GetComponent<PlayerStatManager>();
            stat.OnDeath -= AddDeath;
        }

        void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            Players.Remove(e.Client);
            OnTeamSizeChange?.Invoke(this, EventArgs.Empty);
        }
        
        void AddDeath(object sender, EventArgs e)
        {
            ++TotalKills;
            OnTotalKillsChange?.Invoke(this, EventArgs.Empty);
        }
    }
}
