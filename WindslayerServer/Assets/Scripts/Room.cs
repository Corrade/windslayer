using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;

using Windslayer;

namespace Windslayer.Server
{
    public class Room : MonoBehaviour
    {
        [Header("Public Fields")]
        public string Name;
        public List<ClientConnection> ClientConnections = new List<ClientConnection>();
        public byte MaxSlots;

        Scene scene;
        PhysicsScene physicsScene;

        public void Initialize(string name, byte maxSlots)
        {
            Name = name;
            MaxSlots = maxSlots;

            CreateSceneParameters csp = new CreateSceneParameters(LocalPhysicsMode.Physics3D);
            scene = SceneManager.CreateScene("Room_" + name, csp);
            physicsScene = scene.GetPhysicsScene();

            SceneManager.MoveGameObjectToScene(gameObject, scene);
        }

        public void AddPlayerToRoom(ClientConnection clientConnection)
        {
            ClientConnections.Add(clientConnection);
            clientConnection.Room = this;

            using (Message message = Message.CreateEmpty((ushort)Tags.LobbyJoinRoomAccepted)) {
                clientConnection.Client.SendMessage(message, SendMode.Reliable);
            }
        }

        public void RemovePlayerFromRoom(ClientConnection clientConnection)
        {
            ClientConnections.Remove(clientConnection);
            clientConnection.Room = null;
        }

        public void Close()
        {
            foreach (ClientConnection p in ClientConnections) {
                RemovePlayerFromRoom(p);
            }

            Destroy(gameObject);
        }
    }
}
