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

        public SpawnPlayerMsg() {}

        public SpawnPlayerMsg(ushort clientID)
        {
            ClientID = clientID;
        }

        public void Deserialize(DeserializeEvent e)
        {
            ClientID = e.Reader.ReadUInt16();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ClientID);
        }
    }
}
