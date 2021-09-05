using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;

using Windslayer;

namespace Windslayer.Server
{
    [RequireComponent(typeof(PlayerConnectionManager))]
    public class PlayerInputManager : MonoBehaviour
    {
        PlayerConnectionManager m_PlayerConnectionManager;

        List<bool> m_InputStatesBuffer = new List<bool>(InputIDs.Count);
        List<bool> m_InputStates = new List<bool>(InputIDs.Count);

        void Awake()
        {
            m_PlayerConnectionManager = GetComponent<PlayerConnectionManager>();

            for (int i = 0; i < m_InputStatesBuffer.Count; ++i) {
                m_InputStatesBuffer[i] = false;
                m_InputStates[i] = false;
            }

            m_PlayerConnectionManager.Client.MessageReceived += MessageReceived;
        }

        void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.PlayerInput)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        ushort inputID = reader.ReadUInt16();
                        m_InputStatesBuffer[inputID] = true;
                    }
                }
            }
        }

        // Should run at the end of each tick. Set script execution order accordingly
        void FixedUpdate()
        {
            for (int i = 0; i < m_InputStatesBuffer.Count; ++i) {
                m_InputStates[i] = m_InputStatesBuffer[i];
                m_InputStatesBuffer[i] = false;
            }
        }

        public float GetMoveInput()
        {
            float res = 0f;

            if (m_InputStates[InputIDs.MoveLeft]) {
                res -= 1f;
            }

            if (m_InputStates[InputIDs.MoveRight]) {
                res += 1f;
            }

            return res;
        }

        public bool IsActive(ushort inputID)
        {
            return m_InputStates[inputID];
        }
    }
}
