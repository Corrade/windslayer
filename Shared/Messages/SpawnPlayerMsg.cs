using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;

namespace Windslayer
{
    public class SpawnPlayerMsg : IDarkRiftSerializable
    {
        public ushort ClientID { get; private set; }
        public Vector3 Position { get; private set; }

        public SpawnPlayerMsg() {}

        public SpawnPlayerMsg(ushort clientID, Vector3 position)
        {
            ClientID = clientID;
            Position = position;
        }

        public void Deserialize(DeserializeEvent e)
        {
            ClientID = e.Reader.ReadUInt16();
            Position = new Vector3(e.Reader.ReadSingle(), e.Reader.ReadSingle());
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ClientID);
            e.Writer.Write(Position.x);
            e.Writer.Write(Position.y);
        }
    }
}
