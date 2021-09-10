using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;

namespace Windslayer
{
    public class PlayerCombatInputMsg : IDarkRiftSerializable
    {
        public ushort InputID { get; private set; }

        public PlayerCombatInputMsg() {}

        public PlayerCombatInputMsg(ushort inputID)
        {
            InputID = inputID;
        }

        public void Deserialize(DeserializeEvent e)
        {
            InputID = e.Reader.ReadUInt16();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(InputID);
        }
    }
}
