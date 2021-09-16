using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;

namespace Windslayer
{
    public class WinConditionsStateMsg : IDarkRiftSerializable
    {
        public List<int> TeamTotalKills { get; private set; } = new List<int>( new int[TeamIDs.Count] );
        public int TimeLeft { get; private set; }

        public WinConditionsStateMsg() {}

        public WinConditionsStateMsg(List<int> teamTotalKills, int timeLeft)
        {
            TeamTotalKills = teamTotalKills;
            TimeLeft = timeLeft;
        }

        public void Deserialize(DeserializeEvent e)
        {
            for (int i = 0; i < TeamIDs.Count; ++i) {
                TeamTotalKills[i] = e.Reader.ReadInt32();
            }

            TimeLeft = e.Reader.ReadInt32();
        }

        public void Serialize(SerializeEvent e)
        {
            for (int i = 0; i < TeamIDs.Count; ++i) {
                e.Writer.Write(TeamTotalKills[i]);
            }

            e.Writer.Write(TimeLeft);
        }
    }
}
