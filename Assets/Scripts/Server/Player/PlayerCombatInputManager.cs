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
    [RequireComponent(typeof(PlayerConnectionData))]
    public class PlayerCombatInputManager : MonoBehaviour
    {
        PlayerConnectionData m_PlayerConnectionData;

        List<bool> m_InputStatesBuffer = new List<bool>( new bool[CombatInputIDs.Count] );
        List<bool> m_JustActivatedInputStates = new List<bool>( new bool[CombatInputIDs.Count] );
        List<bool> m_InputStates = new List<bool>( new bool[CombatInputIDs.Count] );

        void Awake()
        {
            m_PlayerConnectionData = GetComponent<PlayerConnectionData>();

            for (int i = 0; i < m_InputStatesBuffer.Count; ++i) {
                m_InputStatesBuffer[i] = false;
                m_JustActivatedInputStates[i] = false;
                m_InputStates[i] = false;
            }

            if (m_PlayerConnectionData.Client == null) {
                Debug.LogError("A player script executed before PlayerConnectionData - revise script ordering");
            }

            m_PlayerConnectionData.Client.MessageReceived += MessageReceived;
        }

        void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            using (DarkRiftReader reader = message.GetReader()) {
                if (message.Tag == Tags.PlayerCombatInput) {
                    while (reader.Position < reader.Length)
                    {
                        PlayerCombatInputMsg msg = reader.ReadSerializable<PlayerCombatInputMsg>();

                        m_InputStatesBuffer[msg.InputID] = true;
                    }
                }
            }
        }

        // Should run after all the player scripts. Set script execution order accordingly
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

            if (m_InputStates[CombatInputIDs.MoveLeft]) {
                res -= 1f;
            }

            if (m_InputStates[CombatInputIDs.MoveRight]) {
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
