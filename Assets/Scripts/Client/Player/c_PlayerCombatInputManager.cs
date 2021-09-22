using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

using Windslayer;

namespace Windslayer.Client
{
    public class c_PlayerCombatInputManager : MonoBehaviour
    {
        c_PlayerConnectionManager m_PlayerConnectionManager;

        protected Dictionary<ushort, KeyCode> m_Binds = new Dictionary<ushort, KeyCode>();

        void Awake()
        {
            SetupBinds();
            m_PlayerConnectionManager = GetComponent<c_PlayerConnectionManager>();
        }

        protected virtual void SetupBinds() {
            m_Binds.Add(CombatInputIDs.MoveLeft, KeyCode.A);
            m_Binds.Add(CombatInputIDs.MoveRight, KeyCode.D);
            m_Binds.Add(CombatInputIDs.Jump, KeyCode.W);
            m_Binds.Add(CombatInputIDs.Drop, KeyCode.S);
            m_Binds.Add(CombatInputIDs.LightAttack, KeyCode.Z);
            m_Binds.Add(CombatInputIDs.StrongAttack, KeyCode.C);
            m_Binds.Add(CombatInputIDs.Block, KeyCode.X);
            m_Binds.Add(CombatInputIDs.Dash, KeyCode.V);
        }

        void Update()
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                foreach (KeyValuePair<ushort, KeyCode> entry in m_Binds) {
                    if (Input.GetKey(entry.Value)) {
                        writer.Write(new PlayerCombatInputMsg(entry.Key));
                    }
                }

                using (Message message = Message.Create(Tags.PlayerCombatInput, writer)) {
                    m_PlayerConnectionManager.Client.SendMessage(message, SendMode.Unreliable);
                }
            }
        }
    }
}
