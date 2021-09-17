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
        // 	0 to 4,294,967,295
        public static uint CurrentTick { get; private set; } = 0;

        static event EventHandler OnTickUpdate;

        // Should run at the end of each tick
        void FixedUpdate()
        {
            ++CurrentTick;
            OnTickUpdate?.Invoke(this, EventArgs.Empty);
        }

        // Calls callback() after tickCount.
        // Returns an event handler that can be passed into RemoveListener().
        public static EventHandler Wait(uint tickCount, Action callback)
        {
            uint endTick = CurrentTick + tickCount;

            EventHandler f = null;
            f = delegate(object sender, EventArgs e) {
                if (CurrentTick == endTick) {
                    callback();
                    OnTickUpdate -= f;
                }
            };

            OnTickUpdate += f;

            return f;
        }

        // Calls callback() every tick.
        // Returns an event handler that can be passed into RemoveListener().
        public static EventHandler CallEveryTick(Action callback)
        {
            EventHandler f = delegate(object sender, EventArgs e) {
                callback();
            };

            OnTickUpdate += f;

            return f;
        }

        // Calls callback() every tick where tickFilter(CurrentTick) returns true.
        // Returns an event handler that can be passed into RemoveListener().
        public static EventHandler CallEveryTick(Action callback, Func<uint, bool> tickFilter)
        {
            EventHandler f = delegate(object sender, EventArgs e) {
                if (tickFilter(CurrentTick)) {
                    callback();
                }
            };

            OnTickUpdate += f;

            return f;
        }

        public static void RemoveListener(EventHandler f)
        {
            OnTickUpdate -= f;
        }
    }
}
