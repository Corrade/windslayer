using System;
using System.Collections;
using System.Collections.Generic;

namespace Windslayer
{
    static class Tags
    {
        public static readonly ushort SpawnPlayer = 0;
        public static readonly ushort MovePlayer = 1;
        public static readonly ushort DespawnPlayer = 2;
        public static readonly ushort PlayerCombatInput = 3;
        public static readonly ushort LobbySettings = 4;

        public static readonly ushort Count = 5;
    }
}
