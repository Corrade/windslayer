using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

using Windslayer;

namespace Windslayer.Client
{
    public class c_PlayerConnectionData : MonoBehaviour
    {
        // public ushort ClientID { get; private set; }
        public UnityClient Client { get; private set; }

        public void Initialise(UnityClient client)
        {
            Client = client;
        }
    }
}
