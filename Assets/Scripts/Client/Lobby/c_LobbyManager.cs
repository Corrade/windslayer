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
    public class c_LobbyManager : MonoBehaviour
    {
        [SerializeField]
        c_PlayerConnectionManager OtherPlayerPrefab;

        [SerializeField]
        c_PlayerConnectionManager ThisPlayerPrefab;

        [SerializeField]
        c_MainCamera Cam;

        [SerializeField]
        UnityClient Client;

        /*
        [SerializeField]
        GameObject PlayerManagerPrefab;
        
        [SerializeField]
        List<Map> Maps = new List<Map>( new Map[MapIDs.Count] );

        public LobbySettingsMsg Settings { get; private set; }
        public Map CurrentMap { get; private set; }

        Dictionary<IClient, PlayerManager> m_PlayerManagers = new Dictionary<IClient, PlayerManager>();
        List<PlayerManager> m_HostOrder = new List<PlayerManager>();
        List<Team> m_Teams = new List<Team>( new Team[TeamIDs.Count] );

        int m_TimeLeftTicks;
        EventHandler m_TimerListener;
        bool m_GameStarted = false;
        */

        void Awake()
        {
            /*
            if (Maps.Count != MapIDs.Count) {
                Debug.LogError("Map(s) unimplemented");
                Application.Quit();
            }
            */

            Client.MessageReceived += MessageReceived;
        }

        void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message) {
                if (message.Tag == Tags.PlayerMetadata) {
                    ;
                } else if (message.Tag == Tags.DisconnectPlayer) {
                    ;
                } else if (message.Tag == Tags.LobbySettings) {
                    ;
                } else if (message.Tag == Tags.StartGame) {
                    ;
                } else if (message.Tag == Tags.EndGame) {
                    ;
                } else if (message.Tag == Tags.TeamDeclaration) {
                    ;
                } else if (message.Tag == Tags.WinConditionsState) {
                    ;
                } else if (message.Tag == Tags.SpawnPlayer) {
                    SpawnPlayer(sender, e);
                } else if (message.Tag == Tags.DespawnPlayer) {
                    DespawnPlayer(sender, e);
                } else if (message.Tag == Tags.MovePlayer) {
                    MovePlayer(sender, e);
                }
            }
        }

        void SpawnPlayer(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage())
            using (DarkRiftReader reader = message.GetReader())
            {
                // Multiple players are combined into one message, so loop
                while (reader.Position < reader.Length)
                {
                    SpawnPlayerMsg msg = reader.ReadSerializable<SpawnPlayerMsg>();
                    
                    c_PlayerConnectionManager player;

                    if (msg.ClientID == Client.ID) {
                        player = Instantiate(ThisPlayerPrefab, Vector3.zero, Quaternion.identity);
                        Cam.Target = player.transform;
                    } else {
                        player = Instantiate(OtherPlayerPrefab, Vector3.zero, Quaternion.identity);
                    }

                    player.Initialise(Client);

                    // m_Players.Add(msg.ClientID, player);
                }
            }
        }

        void DespawnPlayer(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {                    
                DespawnPlayerMsg msg = message.Deserialize<DespawnPlayerMsg>();

                // Destroy(m_Players[msg.ClientID].gameObject);
                // m_Players.Remove(msg.ClientID);
            }
        }

        void MovePlayer(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {                    
                MovePlayerMsg msg = message.Deserialize<MovePlayerMsg>();

                /*
                m_Players[msg.ClientID].gameObject.transform.position = msg.Position;
                if (msg.IsFacingLeft) {
                    m_Players[msg.ClientID].gameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
                } else {
                    m_Players[msg.ClientID].gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
                }
                */
            }
        }
    }
}
