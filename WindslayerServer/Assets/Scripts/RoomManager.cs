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
    public sealed class RoomManager : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField]
        Room roomPrefab;

        public static RoomManager Instance { get { return _instance; } }

        static RoomManager _instance;
        Dictionary<string, Room> rooms = new Dictionary<string, Room>();

        void Awake()
        {
            if (_instance != null && _instance != this) {
                Destroy(this.gameObject);
            } else {
                _instance = this;
            }

            DontDestroyOnLoad(this);

            CreateRoom("Main1", 25);
            CreateRoom("Main2", 15);
        }

        public void CreateRoom(string roomName, byte maxSlots)
        {
            Room room = Instantiate(roomPrefab);
            room.Initialize(roomName, maxSlots);
            rooms.Add(roomName, room);
        }

        public void RemoveRoom(string name)
        {
            Room r = rooms[name];
            r.Close();
            rooms.Remove(name);
        }

        public RoomData[] GetRoomDataList()
        {
            RoomData[] data = new RoomData[rooms.Count];
            int i = 0;

            foreach (KeyValuePair<string, Room> kvp in rooms) {
                Room r = kvp.Value;
                data[i] = new RoomData(r.Name, (byte) r.ClientConnections.Count, r.MaxSlots);
                ++i;
            }

            return data;
        }

        public void TryJoinRoom(IClient client, JoinRoomRequestData data)
        {
            bool canJoin = ServerManager.Instance.Players.TryGetValue(client.ID, out var clientConnection);

            if (!rooms.TryGetValue(data.RoomName, out var room)) {
                canJoin = false;
            } else if (room.ClientConnections.Count >= room.MaxSlots) {
                canJoin = false;
            }

            if (canJoin) {
                room.AddPlayerToRoom(clientConnection);
            } else {
                using (Message message = Message.Create(
                    (ushort)Tags.LobbyJoinRoomDenied,
                    new LobbyInfoData(GetRoomDataList())
                )) {
                    client.SendMessage(message, SendMode.Reliable);
                }
            }
        }
    }
}
