using System;
using System.Collections;
using System.Collections.Generic;

namespace Windslayer
{
    public enum Tags
    {
        LoginRequest,
        LoginRequestAccepted,
        LoginRequestDenied,

        LobbyJoinRoomRequest,
        LobbyJoinRoomDenied,
        LobbyJoinRoomAccepted,

        // Bidirectional
        StartGame,
        LobbySettings,
        PlayerMetadata,
        TeamDeclaration,

        // Client-bound
        DespawnPlayer,
        DisconnectPlayer,
        EndGame,
        MovePlayer,
        SpawnPlayer,
        WinConditionsState,

        // Server-bound
        PlayerCombatInput0,

        Count1,
    }
}
