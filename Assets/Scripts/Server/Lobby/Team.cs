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
    public class Team
    {
        public Dictionary<IClient, PlayerManager> Players { get; private set; } = new Dictionary<IClient, PlayerManager>();
        public int TotalKills { get; private set; } = 0;

        public event EventHandler OnTeamCountChange;
        public event EventHandler OnTotalKillsChange;

        public int Count()
        {
            return Players.Keys.Count;
        }

        public void ResetKills()
        {
            TotalKills = 0;
        }

        public void Spawn()
        {
            foreach (PlayerManager playerManager in Players.Values) {
                if (playerManager.PlayerIsSpawned()) {
                    Debug.Log("Player should not be instantiated before the start of the game");
                }

                playerManager.Spawn();
            }
        }

        public void Despawn()
        {
            foreach (PlayerManager playerManager in Players.Values) {
                if (!playerManager.PlayerIsSpawned()) {
                    Debug.Log("Team should not have a null player at the end of a game");
                }

                playerManager.Despawn();
            }
        }
    
        public void Add(IClient client, PlayerManager player)
        {
            Players.Add(client, player);
            OnTeamCountChange?.Invoke(this, EventArgs.Empty);

            PlayerStatManager stat = player.GetComponent<PlayerStatManager>();
            stat.OnKillsChanged += AddKill;
        }

        // Must be called whenever a player leaves the team, voluntarily or via disconnect
        public void Remove(IClient client)
        {
            PlayerManager player = Players[client];
            if (player != null) {
                PlayerStatManager stat = player.GetComponent<PlayerStatManager>();
                    if (stat != null) {
                    stat.OnKillsChanged -= AddKill;
                }
            }

            Players.Remove(client);
            OnTeamCountChange?.Invoke(this, EventArgs.Empty);
        }

        void AddKill(object sender, EventArgs e)
        {
            ++TotalKills;
            OnTotalKillsChange?.Invoke(this, EventArgs.Empty);
        }
    }
}
