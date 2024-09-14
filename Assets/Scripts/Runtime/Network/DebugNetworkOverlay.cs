using System;
using FishNet.Managing;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Zombies.Runtime.Network
{
    [RequireComponent(typeof(NetworkManager))]
    public class DebugNetworkOverlay : MonoBehaviour
    {
        public string clientEndpoint = "127.0.0.1";
        
        private NetworkManager netManager;

        private void Awake()
        {
            netManager = GetComponent<NetworkManager>();
        }

        private void Update()
        {
            if (netManager.ServerManager.Started || netManager.ClientManager.Started) return;
            
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.hKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame) StartHost();
                if (kb.cKey.wasPressedThisFrame) StartClient();
            }
        }

        private void OnGUI()
        {
            using (new GUILayout.AreaScope(new Rect(10f, 10f, 100f, Screen.height - 20f)))
            {
                if (GUILayout.Button("Start Host")) StartHost();
                if (GUILayout.Button("Start Client")) StartClient();
                clientEndpoint = GUILayout.TextField(clientEndpoint);
            }
        }

        private void StartHost()
        {
            netManager.ServerManager.StartConnection();
            netManager.ClientManager.StartConnection("127.0.0.1");
        }

        private void StartClient() { netManager.ClientManager.StartConnection(clientEndpoint); }
    }
}