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
    public class LoginManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        GameObject loginWindow;
        [SerializeField]
        InputField nameInput;
        [SerializeField]
        Button submitLoginButton;

        void Start()
        {
            ConnectionManager.Instance.OnConnected += StartLoginProcess;
            ConnectionManager.Instance.Client.MessageReceived += OnMessage;
            submitLoginButton.onClick.AddListener(OnSubmitLogin);
            loginWindow.SetActive(false);
        }

        void OnDestroy()
        {
            ConnectionManager.Instance.OnConnected -= StartLoginProcess;
            ConnectionManager.Instance.Client.MessageReceived -= OnMessage;
        }

        public void StartLoginProcess()
        {
            loginWindow.SetActive(true);
        }

        void OnMessage(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage()) {
                switch ((Tags)message.Tag) {
                    case Tags.LoginRequestDenied:
                        OnLoginDenied();
                        break;
                    case Tags.LoginRequestAccepted:
                        OnLoginAccepted(message.Deserialize<LoginInfoData>());
                        break;
                }
            }
        }

        public void OnSubmitLogin()
        {
            if (!String.IsNullOrEmpty(nameInput.text)) {
                loginWindow.SetActive(false);

                using (Message message = Message.Create(
                    (ushort)Tags.LoginRequest,
                    new LoginRequestData(nameInput.text)
                )) {
                    ConnectionManager.Instance.Client.SendMessage(message, SendMode.Reliable);
                }
            }
        }

        void OnLoginDenied()
        {
            loginWindow.SetActive(true);
            Debug.Log("Login failed.");
        }

        void OnLoginAccepted(LoginInfoData data)
        {
            ConnectionManager.Instance.PlayerId = data.Id;
            ConnectionManager.Instance.LobbyInfoData = data.Data;
            SceneManager.LoadScene("Lobby");
        }
    }
}
