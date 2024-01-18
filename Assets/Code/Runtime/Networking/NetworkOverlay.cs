using FishNet.Managing;
using UnityEngine;

namespace Zombies.Runtime.Networking
{
    [RequireComponent(typeof(NetworkManager))]
    public class NetworkOverlay : MonoBehaviour
    {
        private NetworkManager netManager;

        private void Awake() { netManager = GetComponent<NetworkManager>(); }

        private void OnGUI()
        {
            using (new GUILayout.AreaScope(new Rect(0, 0, 300, Screen.height)))
            {
                var str = "";

                str += $"Is Server ({netManager.IsServer})\n";
                str += $"Is Client ({netManager.IsClient})\n";

                if (netManager.ClientManager.Started)
                {
                    str += $"Clients Connected: {netManager.ClientManager.Clients.Count}\n";
                }

                GUILayout.Label(str);
            }
        }
    }
}