using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;

namespace Windslayer
{
    public class MovePlayerMsg : IDarkRiftSerializable
    {
        public ushort ClientID { get; private set; }
        public Vector3 Position { get; private set; }
        public bool IsFacingLeft { get; private set; }

        public MovePlayerMsg() {}

        public MovePlayerMsg(ushort clientID, Vector3 position, bool isFacingLeft)
        {
            ClientID = clientID;
            Position = position;
            IsFacingLeft = isFacingLeft;
        }

        public void Deserialize(DeserializeEvent e)
        {
            ClientID = e.Reader.ReadUInt16();
            Position = new Vector3(e.Reader.ReadSingle(), e.Reader.ReadSingle());
            IsFacingLeft = e.Reader.ReadBoolean();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ClientID);
            e.Writer.Write(Position.x);
            e.Writer.Write(Position.y);
            e.Writer.Write(IsFacingLeft);
        }
    }
}
