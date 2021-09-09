using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Windslayer
{
    // BAD IMPLEMENTATION FOR SERVER SYNCING! SHOULD BE BASED ON TICKRATE, NOT FRAMERATE

    public class Sync : MonoBehaviour
    {
        public static readonly uint Tickrate = 60;

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

        public static bool IsTickBefore(uint Tick1, uint Tick2)
        {
            // If Tick1 is large and Tick2 is small (with a generous, safe gap between those two thresholds), then assume Tick2 has wrapped around and hence Tick1 precedes it
            // Large = first 20 bits are set = at least 4294963200
            // Small = first 19 bits are unset = at most 8191
            if ((~(Tick1 >> 12) == 0b0) && ((Tick2 >> 13) == 0b0)) {
                return true;
            } else if ((~(Tick2 >> 12) == 0b0) && ((Tick1 >> 13) == 0b0)) {
                return false;
            } else {
                return Tick1 < Tick2;
            }
        }
    }
}
