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

        [Tooltip("An array of each team's list of spawn points")]
        [SerializeField]
        List<SpawnPointArray> TeamSpawns = new List<SpawnPointArray>( new SpawnPointArray[TeamIDs.Count] );

        System.Random m_Rnd = new System.Random();

        public Vector3 GetRandomSpawn(ushort teamID)
        {
            List<Transform> spawns = TeamSpawns[teamID].SpawnPoints;
            return spawns[m_Rnd.Next(spawns.Count)].position;
        }
    }
}
