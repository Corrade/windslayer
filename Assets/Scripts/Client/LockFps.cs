using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Windslayer.Client
{
    public class LockFps : MonoBehaviour
    {
        void Start()
        {
            // 60 FPS lock
            Application.targetFrameRate = 60;
        }
    }
}
