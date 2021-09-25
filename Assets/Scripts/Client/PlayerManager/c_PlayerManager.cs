using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

using Windslayer;

namespace Windslayer.Client
{
    public class c_PlayerManager : MonoBehaviour
    {
        [SerializeField]
        GameObject PlayerPrefab;

        public UnityClient Client { get; private set; }

        ushort _TeamID = TeamIDs.InvalidTeamID;
        public ushort TeamID {
            get {
                return _TeamID;
            }

            set {
                _TeamID = value;

                if (PlayerIsSpawned()) {
                    c_PlayerConnectionData conn = Player.GetComponent<c_PlayerConnectionData>();
                    conn.TeamID = value;
                }
            }
        }

        public PlayerMetadataMsg Metadata { get; private set; } = null;
        public GameObject Player { get; private set; } = null;

        c_LobbyManager m_Lobby;

        public void Initialise(UnityClient client, c_LobbyManager lobby, PlayerMetadataMsg metadata)
        {
            Client = client;
            m_Lobby = lobby;
            Metadata = metadata;

            // Client.MessageReceived += MessageReceived;
        }

        public bool PlayerIsSpawned()
        {
            return Player != null;
        }

        public void Spawn(Vector3 position)
        {
            if (!TeamIDs.IsValid(TeamID)) {
                Debug.LogError("Player not without being in a team");
                return;
            }

            if (PlayerIsSpawned()) {
                Debug.Log("Trying to spawn a player that's already spawned");
                return;
            }

            Player = Instantiate(PlayerPrefab, position, Quaternion.identity);

            if (Player.activeSelf) {
                Debug.LogError("In-game player should not begin active");
            }

            c_PlayerConnectionData conn = Player.GetComponent<c_PlayerConnectionData>();
            conn.Initialise(Metadata.ClientID, Client, m_Lobby, TeamID);

            Player.SetActive(true);
        }

        public void Despawn()
        {
            if (!PlayerIsSpawned()) {
                Debug.Log("Trying to despawn a player that's already despawned");
                return;
            }

            Destroy(Player);
            Player = null;
        }
    }
}
