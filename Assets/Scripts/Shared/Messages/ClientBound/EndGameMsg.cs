using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;

namespace Windslayer
{
    public class EndGameMsg : IDarkRiftSerializable
    {
        public ushort WinningTeamID { get; private set; }

        public EndGameMsg() {}

        public EndGameMsg(ushort winningTeamID)
        {
            WinningTeamID = winningTeamID;
        }

        public void Deserialize(DeserializeEvent e)
        {
            WinningTeamID = e.Reader.ReadUInt16();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(WinningTeamID);
        }
    }
}
