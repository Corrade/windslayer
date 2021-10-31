using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;

namespace Windslayer
{
    public struct LobbyInfoData : IDarkRiftSerializable
    {
        public RoomData[] Rooms;

        public LobbyInfoData(RoomData[] rooms)
        {
            Rooms = rooms;
        }

        public void Deserialize(DeserializeEvent e)
        {
            Rooms = e.Reader.ReadSerializables<RoomData>();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Rooms);
        }
    }
}
