using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

using Windslayer;

namespace Windslayer.Client
{
    public class RoomListObject : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        Text nameText;
        [SerializeField]
        Text slotsText;
        [SerializeField]
        Button joinButton;

        public void Set(LobbyManager lobbyManager, RoomData data)
        {
            nameText.text = data.Name;
            slotsText.text = data.Slots + "/" + data.MaxSlots;
            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(delegate { lobbyManager.SendJoinRoomRequest(data.Name); });
        }
    }
}
