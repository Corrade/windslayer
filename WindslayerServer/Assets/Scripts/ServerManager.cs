using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;

using Windslayer;

namespace Windslayer.Server
{
    public class ServerManager : MonoBehaviour
    {
        public static ServerManager Instance { get { return _instance; } }
        public Dictionary<ushort, ClientConnection> Players = new Dictionary<ushort, ClientConnection>();
        public Dictionary<string, ClientConnection> PlayersByName = new Dictionary<string, ClientConnection>();

        static ServerManager _instance;
        XmlUnityServer xmlServer;
        DarkRiftServer server;

        void Awake()
        {
            if (_instance != null && _instance != this) {
                Destroy(this.gameObject);
            } else {
                _instance = this;
            }

            DontDestroyOnLoad(this);
        }

        void Start()
        {
            xmlServer = GetComponent<XmlUnityServer>();
            server = xmlServer.Server;
            server.ClientManager.ClientConnected += OnClientConnected;
            server.ClientManager.ClientDisconnected += OnClientDisconnected;
        }

        void OnDestroy()
        {
            server.ClientManager.ClientConnected -= OnClientConnected;
            server.ClientManager.ClientDisconnected -= OnClientDisconnected;
        }

        void OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            e.Client.MessageReceived += OnMessage;
        }

        void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            IClient client = e.Client;
            ClientConnection p;
            if (Players.TryGetValue(client.ID, out p)) {
                p.OnClientDisconnected(sender, e);
            }
        }

        void OnMessage(object sender, MessageReceivedEventArgs e)
        {
            IClient client = (IClient)sender;

            using (Message message = e.GetMessage()) {
                switch ((Tags)message.Tag) {
                    case Tags.LoginRequest:
                        OnClientLogin(client, message.Deserialize<LoginRequestData>());
                        break;
                }
            }
        }

        void OnClientLogin(IClient client, LoginRequestData data)
        {
            if (PlayersByName.ContainsKey(data.Name)) {
                using (Message message = Message.CreateEmpty((ushort)Tags.LoginRequestDenied)) {
                    client.SendMessage(message, SendMode.Reliable);
                }

                return;
            }

            // In the future the ClientConnection will handle its messages
            client.MessageReceived -= OnMessage;

            new ClientConnection(client, data);
        }
    }
}
