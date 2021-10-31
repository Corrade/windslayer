using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;

namespace Windslayer
{
    // Acts as an initialising message for players
    public class PlayerMetadataMsg : IDarkRiftSerializable
    {
        public ushort ClientID { get; private set; }
        public string Username { get; private set; }

        public PlayerMetadataMsg() {}

        public PlayerMetadataMsg(ushort clientID, string username)
        {
            ClientID = clientID;
            Username = username;
        }

        public void Deserialize(DeserializeEvent e)
        {
            ClientID = e.Reader.ReadUInt16();
            Username = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ClientID);
            e.Writer.Write(Username);
        }
    }
}
