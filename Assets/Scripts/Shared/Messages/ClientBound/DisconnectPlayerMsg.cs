using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;

namespace Windslayer
{
    public class DisconnectPlayerMsg : IDarkRiftSerializable
    {
        public ushort ClientID { get; private set; }

        public DisconnectPlayerMsg() {}

        public DisconnectPlayerMsg(ushort clientID)
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
