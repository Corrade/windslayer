using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Windslayer
{
    // BAD IMPLEMENTATION FOR SERVER SYNCING! SHOULD BE BASED ON TICKRATE, NOT FRAMERATE

    public class Sync : MonoBehaviour
    {
        public static readonly ushort Tickrate = 60;

        // runPerTick() takes in the number of frames that have elapsed since the start of this function. This function will therefore take in ints from 0 to (m_tickCount - 1).
        public static IEnumerator Delay(int tickCount, Action<int> runPerTick, Action callback)
        {
            int ticksElapsed = 0;

            while (ticksElapsed < tickCount) {
                runPerTick(ticksElapsed);
                ticksElapsed++;
                yield return null;
            }

            callback();
        }

        public static IEnumerator Delay(int tickCount, Action<int> runPerTick)
        {
            yield return Delay(tickCount, runPerTick, () => {});
        }

        public static IEnumerator Delay(int tickCount, Action callback)
        {
            yield return Delay(tickCount, (int x) => {}, callback);
        }

        public static IEnumerator Delay(int tickCount)
        {
            yield return Delay(tickCount, (int x) => {});
        }

        public static bool IsTickBefore(ushort Tick1, ushort Tick2)
        {
            // If Tick1 is large and Tick2 is small (with a generous, safe gap between those two thresholds), then assume Tick2 has wrapped around and hence Tick1 precedes it
            // Large = first 4 bits are set = at least 61440
            // Small = first 3 bits are unset = at most 8191
            if (((Tick1 >> 12) == 0b1111) && ((Tick2 >> 13) == 0b000)) {
                return true;
            } else if (((Tick2 >> 12) == 0b1111) && ((Tick1 >> 13) == 0b000)) {
                return false;
            } else {
                return Tick1 < Tick2;
            }
        }
    }
}
