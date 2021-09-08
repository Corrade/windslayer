using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

using Windslayer;

namespace Windslayer.Client
{
    public class PlayerInputManager : MonoBehaviour
    {
        PlayerConnectionManager m_PlayerConnectionManager;

        protected Dictionary<ushort, KeyCode> m_Binds = new Dictionary<ushort, KeyCode>();

        void Awake()
        {
            SetupBinds();
            m_PlayerConnectionManager = GetComponent<PlayerConnectionManager>();
        }

        protected virtual void SetupBinds() {
            m_Binds.Add(InputIDs.MoveLeft, KeyCode.A);
            m_Binds.Add(InputIDs.MoveRight, KeyCode.D);
            m_Binds.Add(InputIDs.Jump, KeyCode.W);
            m_Binds.Add(InputIDs.Drop, KeyCode.S);
            m_Binds.Add(InputIDs.LightAttack, KeyCode.Z);
            m_Binds.Add(InputIDs.StrongAttack, KeyCode.C);
            m_Binds.Add(InputIDs.Block, KeyCode.X);
            m_Binds.Add(InputIDs.Dash, KeyCode.V);
        }

        void Update()
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                foreach (KeyValuePair<ushort, KeyCode> entry in m_Binds) {
                    if (Input.GetKey(entry.Value)) {
                        writer.Write(new PlayerInputMsg(entry.Key));
                    }
                }

                using (Message message = Message.Create(Tags.PlayerInput, writer)) {
                    m_PlayerConnectionManager.Client.SendMessage(message, SendMode.Unreliable);
                }
            }
        }
    }
}
