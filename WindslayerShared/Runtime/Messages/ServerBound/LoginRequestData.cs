using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;

namespace Windslayer
{
    public struct LoginRequestData : IDarkRiftSerializable
    {
        public string Name;

        public LoginRequestData(string name)
        {
            Name = name;
        }

        public void Deserialize(DeserializeEvent e)
        {
            Name = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Name);
        }
    }
}
