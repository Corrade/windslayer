using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

using Windslayer;

namespace Windslayer.Client
{
    public class LobbyManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        Transform roomListContainerTransform;

        [Header("Prefabs")]
        [SerializeField]
        RoomListObject roomListPrefab;

        void Start()
        {
            ConnectionManager.Instance.Client.MessageReceived += OnMessage;
            RefreshRooms(ConnectionManager.Instance.LobbyInfoData);
        }

        void OnDestroy()
        {
            ConnectionManager.Instance.Client.MessageReceived -= OnMessage;
        }

        void OnMessage(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                switch ((Tags)message.Tag) {
                    case Tags.LobbyJoinRoomDenied:
                        OnRoomJoinDenied(message.Deserialize<LobbyInfoData>());
                        break;
                    case Tags.LobbyJoinRoomAccepted:
                        OnRoomJoinAccepted();
                        break;
                }
            }
        }

        public void SendJoinRoomRequest(string roomName)
        {
            using (Message message = Message.Create(
                (ushort)Tags.LobbyJoinRoomRequest,
                new JoinRoomRequestData(roomName)
            )) {
                ConnectionManager.Instance.Client.SendMessage(message, SendMode.Reliable);
            }
        }

        void RefreshRooms(LobbyInfoData data)
        {
            RoomListObject[] roomObjects = roomListContainerTransform.GetComponentsInChildren<RoomListObject>();

            if (roomObjects.Length > data.Rooms.Length) {
                for (int i = data.Rooms.Length; i < roomObjects.Length; i++) {
                    Destroy(roomObjects[i].gameObject);
                }
            }

            for (int i = 0; i < data.Rooms.Length; i++) {
                RoomData d = data.Rooms[i];

                if (i < roomObjects.Length) {
                    roomObjects[i].Set(this, d);
                } else {
                    RoomListObject roomListObject = Instantiate(roomListPrefab, roomListContainerTransform);
                    roomListObject.Set(this, d);
                }
            }
        }

        void OnRoomJoinDenied(LobbyInfoData data)
        {
            RefreshRooms(data);
        }

        void OnRoomJoinAccepted()
        {
            SceneManager.LoadScene("Game");
        }
    }
}
