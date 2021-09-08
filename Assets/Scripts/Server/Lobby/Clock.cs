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
    public class Clock : MonoBehaviour
    {
        // 0000 0000 0000 0000
        // 0 to 65,535
        public static ushort CurrentTick { get; private set; } = 0;

        // Should run at the end of each tick
        void FixedUpdate()
        {
            ++CurrentTick;
        }
    }
}
