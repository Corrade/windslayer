using System;
using System.Net;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

using Windslayer;

// https://gamedev.stackexchange.com/questions/116009/in-unity-how-do-i-correctly-implement-the-singleton-pattern

namespace Windslayer.Client
{
    [RequireComponent(typeof(UnityClient))]
    public class ConnectionManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        string ipAddress;
        [SerializeField]
        int port;

        public static ConnectionManager Instance { get { return _instance; } }
        public UnityClient Client { get; private set; }
        public delegate void OnConnectedDelegate();
        public event OnConnectedDelegate OnConnected;
        public ushort PlayerId { get; set; }
        public LobbyInfoData LobbyInfoData { get; set; }

        static ConnectionManager _instance;

        void Awake()
        {
            if (_instance != null && _instance != this) {
                Destroy(this.gameObject);
            } else {
                _instance = this;
            }

            DontDestroyOnLoad(this);

            Client = GetComponent<UnityClient>();
        }

        void Start()
        {
            Client.ConnectInBackground(IPAddress.Parse(ipAddress), port, true, ConnectCallback);
        }

        void ConnectCallback(Exception exception)
        {
            if (Client.ConnectionState == ConnectionState.Connected) {
                OnConnected?.Invoke();
            } else {
                Debug.LogError("Unable to connect to server.");
            }
        }
    }
}
