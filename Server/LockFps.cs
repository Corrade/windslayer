using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Windslayer.Server
{
    public class GameManager : MonoBehaviour
    {
        void Start()
        {
            // 60 FPS lock
            Application.targetFrameRate = 60;
        }
    }
}
