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
        
        public ushort MapID { get; private set; } = MapIDs.Popola;

        // (Seconds)
        public ushort RespawnTime { get; private set; } = 8;

        // 0 = no kill limit
        public ushort KillLimit { get; private set; } = 50;

        // (Seconds) 0 = no time limit
        public int TimeLimit { get; private set; } = 300;

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
        
        // Replaces fields with only the valid fields of another settings object. Since these settings are valid by default and can only change via this function, the settings will always be valid.
        void ReplaceLobbySettings(LobbySettingsMsg s, int nPlayersConnected)
        {
            if (s.MaxPlayers >= 2 && (s.MaxPlayers % 2 == 0) && s.MaxPlayers >= nPlayersConnected) {
                MaxPlayers = s.MaxPlayers;
            }

            if (s.MapID >= 0 && s.MapID < MapIDs.Count) {
                MapID = s.MapID;
            }

            if (s.RespawnTime >= 0) {
                RespawnTime = s.RespawnTime;
            }

            if (s.KillLimit >= 0) {
                KillLimit = s.KillLimit;
            }

            if (s.TimeLimit >= 0) {
                TimeLimit = s.TimeLimit;
            }
        }
    }
}
