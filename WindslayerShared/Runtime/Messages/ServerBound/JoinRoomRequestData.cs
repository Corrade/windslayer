using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;

namespace Windslayer
{
    public struct JoinRoomRequestData : IDarkRiftSerializable
    {
        public string RoomName;

        public JoinRoomRequestData(string name)
        {
            RoomName = name;
        }

        public void Deserialize(DeserializeEvent e)
        {
            RoomName = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(RoomName);
        }
    }
}
