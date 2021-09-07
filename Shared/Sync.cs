using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Windslayer
{
    // BAD IMPLEMENTATION FOR SERVER SYNCING! SHOULD BE BASED ON TICKRATE, NOT FRAMERATE

    public class Sync : MonoBehaviour
    {
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
    }
}
