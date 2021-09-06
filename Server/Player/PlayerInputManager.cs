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

        List<bool> m_InputStatesBuffer = new List<bool>( new bool[InputIDs.Count] );
        List<bool> m_JustActivatedInputStates = new List<bool>( new bool[InputIDs.Count] );
        List<bool> m_InputStates = new List<bool>( new bool[InputIDs.Count] );

        void Awake()
        {
            m_PlayerConnectionManager = GetComponent<PlayerConnectionManager>();

            for (int i = 0; i < m_InputStatesBuffer.Count; ++i) {
                m_InputStatesBuffer[i] = false;
                m_JustActivatedInputStates[i] = false;
                m_InputStates[i] = false;
            }

            m_PlayerConnectionManager.AddInitListener(delegate(object sender, EventArgs e) {
                m_PlayerConnectionManager.Client.MessageReceived += MessageReceived;
            });
        }

        void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            using (DarkRiftReader reader = message.GetReader()) {
                if (message.Tag == Tags.PlayerInput) {
                    while (reader.Position < reader.Length) {
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
                bool oldInputState = m_InputStates[i];
                bool newInputState = m_InputStatesBuffer[i];

                m_JustActivatedInputStates[i] = !oldInputState && newInputState;
                m_InputStates[i] = newInputState;
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

        public bool IsJustActivated(ushort inputID)
        {
            return m_JustActivatedInputStates[inputID];
        }

        public bool IsActive(ushort inputID)
        {
            return m_InputStates[inputID];
        }
    }
}
