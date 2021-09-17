using System;
using System.Collections;
using System.Collections.Generic;

namespace Windslayer
{
    static class TeamIDs
    {
        public static readonly ushort Red = 0;
        public static readonly ushort Blue = 1;

        public static readonly ushort Count = 2;

        public static bool IsValid(ushort teamID)
        {
            return teamID >= 0 && teamID < Count;
        }
    }
}
