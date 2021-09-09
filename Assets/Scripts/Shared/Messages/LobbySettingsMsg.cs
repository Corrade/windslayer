using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;

namespace Windslayer
{
    public class LobbySettingsMsg : IDarkRiftSerializable
    {
        // Multiple of 2, >= 2, >= number of players already connected
        public ushort MaxPlayers { get; set; } = 12;
        
        public ushort MapID { get; set; } = MapIDs.Popola;

        // (Seconds)
        public ushort RespawnTime { get; set; } = 8;

        // 0 = no kill limit
        public ushort KillLimit { get; set; } = 50;

        // (Seconds) 0 = no time limit
        public int TimeLimit { get; set; } = 300;

        public LobbySettingsMsg() {}

        public void Deserialize(DeserializeEvent e)
        {
            MaxPlayers = e.Reader.ReadUInt16();
            KillLimit = e.Reader.ReadUInt16();
            TimeLimit = e.Reader.ReadInt32();
            RespawnTime = e.Reader.ReadUInt16();
            MapID = e.Reader.ReadUInt16();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(MaxPlayers);
            e.Writer.Write(KillLimit);
            e.Writer.Write(TimeLimit);
            e.Writer.Write(RespawnTime);
            e.Writer.Write(MapID);
        }
    }
}
