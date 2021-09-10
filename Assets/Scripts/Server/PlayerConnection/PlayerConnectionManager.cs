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
        [SerializeField]
        GameObject PlayerInGamePrefab;

        public ushort ClientID { get; private set; }
        public IClient Client { get; private set; }
        public DarkRiftServer Server { get; private set; }
        public GameObject InGamePlayer { get; private set; }

        public void Initialise(ushort clientID, IClient client, DarkRiftServer server)
        {
            ClientID = clientID;
            Client = client;
            Server = server;
        }

        public void Spawn(Vector3 position)
        {
            InGamePlayer = Instantiate(PlayerInGamePrefab, position, Quaternion.identity);

            if (InGamePlayer.activeSelf) {
                Debug.LogError("In-game player should not begin active");
            }

            PlayerStatManager stat = InGamePlayer.GetComponent<PlayerStatManager>();
            conn.Initialise(e.Client.ID, e.Client, m_XmlServer.Server);
            m_PlayerClients.Add(e.Client, player);

            InGamePlayer.SetActive(true);
            // set team variable
        }

        public void Despawn()
        {
            Destroy(InGamePlayer);
            InGamePlayer = null;
        }
    }
}
