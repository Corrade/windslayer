using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;

namespace Windslayer
{
    public class TeamSelectMsg : IDarkRiftSerializable
    {
        public ushort ClientID;
        public ushort TeamID;

        public TeamSelectMsg() {}

        public TeamSelectMsg(ushort clientID, ushort teamID)
        {
            ClientID = clientID;
            TeamID = teamID;
        }

        public void Deserialize(DeserializeEvent e)
        {
            ClientID = e.Reader.ReadUInt16();
            TeamID = e.Reader.ReadUInt16();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ClientID);
            e.Writer.Write(TeamID);
        }
    }
}
