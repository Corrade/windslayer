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
        public ushort MaxPlayers { get; private set; } = 12;
        // 0 = no kill limit
        public ushort KillLimit { get; private set; } = 50;
        // (Seconds) 0 = no time limit
        public int TimeLimit { get; private set; } = 300;
        // (Seconds)
        public ushort RespawnTime { get; private set; } = 8;
        public ushort MapID { get; private set; } = MapIDs.Popola;

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
