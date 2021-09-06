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
        public ushort ClientID { get; private set; }
        public IClient Client { get; private set; }
        public DarkRiftServer Server { get; private set; }

        event EventHandler m_OnInit;
        bool m_Initialised = false;

        public void Initialise(ushort clientID, IClient client, DarkRiftServer server)
        {
            ClientID = clientID;
            Client = client;
            Server = server;
            m_Initialised = true;

            m_OnInit?.Invoke(this, EventArgs.Empty);
        }

        public void AddInitListener(EventHandler handler)
        {
            m_OnInit += handler;

            if (m_Initialised) {
                m_OnInit?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
