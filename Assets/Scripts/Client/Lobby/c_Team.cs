using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

using Windslayer;

namespace Windslayer.Client
{
    public class c_Team
    {
        public ushort TeamID { get; private set; }
        public Dictionary<ushort, c_PlayerManager> Players { get; private set; } = new Dictionary<ushort, c_PlayerManager>();
        public int TotalKills { get; set; } = 0;

        public c_Team(ushort teamID)
        {
            TeamID = teamID;
        }

        public int Count()
        {
            return Players.Keys.Count;
        }

        public void Add(c_PlayerManager player)
        {
            Players.Add(player.Metadata.ClientID, player);
        }

        public void Remove(ushort clientID)
        {
            Players.Remove(clientID);
        }
    }
}
