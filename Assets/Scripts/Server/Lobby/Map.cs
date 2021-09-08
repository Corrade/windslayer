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
            public List<Vector3> SpawnPoints = new List<Vector3>();
        }

        [SerializeField]
        List<SpawnPointArray> TeamSpawnSets = new List<SpawnPointArray>( new SpawnPointArray[TeamIDs.Count] );
    }
}
