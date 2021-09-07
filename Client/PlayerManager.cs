using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

using Windslayer;

namespace Windslayer.Client
{
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField]
        PlayerConnectionManager OtherPlayerPrefab;

        [SerializeField]
        PlayerConnectionManager ThisPlayerPrefab;

        [SerializeField]
        MainCamera Cam;

        UnityClient m_Client;

        Dictionary<ushort, PlayerConnectionManager> m_Players = new Dictionary<ushort, PlayerConnectionManager>();

        void Awake()
        {
            m_Client = GetComponent<UnityClient>();
        
            m_Client.MessageReceived += MessageReceived;
        }

        void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.SpawnPlayer) {
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
                    
                    PlayerConnectionManager player;

                    if (msg.ClientID == m_Client.ID) {
                        player = Instantiate(ThisPlayerPrefab, msg.Position, Quaternion.identity);
                        Cam.Target = player.transform;
                    } else {
                        player = Instantiate(OtherPlayerPrefab, msg.Position, Quaternion.identity);
                    }

                    player.Initialise(m_Client);

                    m_Players.Add(msg.ClientID, player);
                }
            }
        }

        void DespawnPlayer(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {                    
                DespawnPlayerMsg msg = message.Deserialize<DespawnPlayerMsg>();

                Destroy(m_Players[msg.ClientID].gameObject);
                m_Players.Remove(msg.ClientID);
            }
        }

        void MovePlayer(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {                    
                MovePlayerMsg msg = message.Deserialize<MovePlayerMsg>();

                m_Players[msg.ClientID].gameObject.transform.position = msg.Position;
                if (msg.IsFacingLeft) {
                    m_Players[msg.ClientID].gameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
                } else {
                    m_Players[msg.ClientID].gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
                }
            }
        }
    }
}
