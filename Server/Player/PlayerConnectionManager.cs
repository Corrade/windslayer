using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;

using Windslayer;

namespace Windslayer.Server
{
    public class PlayerConnectionManager : MonoBehaviour
    {
        public ushort ID { get; set; }
        public IClient Client { get; set; }
        public XmlUnityServer XmlServer { get; set; }
    }
}
