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
    public class PlayerConnectionManager : MonoBehaviour
    {
        // public ushort ClientID { get; private set; }
        public UnityClient Client { get; private set; }

        event EventHandler m_OnInit;
        bool m_Initialised = false;

        public void Initialise(UnityClient client)
        {
            Client = client;
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
