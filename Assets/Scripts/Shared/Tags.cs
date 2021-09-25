using System;
using System.Collections;
using System.Collections.Generic;

namespace Windslayer
{
    static class Tags
    {
        // Bidirectional
        public static readonly ushort StartGame = 0;
        public static readonly ushort LobbySettings = 1;
        public static readonly ushort PlayerMetadata = 2;
        public static readonly ushort TeamDeclaration = 3;

        // Client-bound
        public static readonly ushort DespawnPlayer = 4;
        public static readonly ushort DisconnectPlayer = 5;
        public static readonly ushort EndGame = 6;
        public static readonly ushort MovePlayer = 7;
        public static readonly ushort SpawnPlayer = 8;
        public static readonly ushort WinConditionsState = 9;

        // Server-bound
        public static readonly ushort PlayerCombatInput = 10;

        public static readonly ushort Count = 11;
    }
}
