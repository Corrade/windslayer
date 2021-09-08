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
    public class Map : MonoBehaviour
    {
        [System.Serializable] public class SpawnPointArray
        {
            public List<Transform> SpawnPoints = new List<Transform>();
        }

        [SerializeField]
        List<SpawnPointArray> TeamSpawnSets = new List<SpawnPointArray>( new SpawnPointArray[TeamIDs.CountNoSpec] );

        System.Random m_Rnd = new System.Random();

        public void MoveTeamToSpawn(ushort teamID, Team team)
        {
            List<Transform> spawnPoints = TeamSpawnSets[teamID].SpawnPoints;

            foreach (GameObject player in team.Players.Values) {
                player.transform.position = spawnPoints[m_Rnd.Next(spawnPoints.Count)].position;
            }
        }

        public void MovePlayerToSpawn(ushort teamID, GameObject player)
        {
            List<Transform> spawnPoints = TeamSpawnSets[teamID].SpawnPoints;

            player.transform.position = spawnPoints[m_Rnd.Next(spawnPoints.Count)].position;
        }
    }
}
