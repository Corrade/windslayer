using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;

namespace Windslayer
{
    public struct LoginInfoData : IDarkRiftSerializable
    {
        public ushort Id;
        public LobbyInfoData Data;

        public LoginInfoData(ushort id, LobbyInfoData data)
        {
            Id = id;
            Data = data;
        }

        public void Deserialize(DeserializeEvent e)
        {
            Id = e.Reader.ReadUInt16();
            Data = e.Reader.ReadSerializable<LobbyInfoData>();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Id);
            e.Writer.Write(Data);
        }
    }
}
