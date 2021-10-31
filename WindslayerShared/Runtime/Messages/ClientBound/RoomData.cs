using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;

namespace Windslayer
{
    public struct RoomData : IDarkRiftSerializable
    {
        public string Name;
        public byte Slots;
        public byte MaxSlots;

        public RoomData(string name, byte slots, byte maxSlots)
        {
            Name = name;
            Slots = slots;
            MaxSlots = maxSlots;
        }

        public void Deserialize(DeserializeEvent e)
        {
            Name = e.Reader.ReadString();
            Slots = e.Reader.ReadByte();
            MaxSlots = e.Reader.ReadByte();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Name);
            e.Writer.Write(Slots);
            e.Writer.Write(MaxSlots);
        }
    }
}
