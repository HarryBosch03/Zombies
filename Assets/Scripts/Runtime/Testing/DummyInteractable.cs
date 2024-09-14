using UnityEngine;
using Zombies.Runtime.Interactive;
using Zombies.Runtime.Player;

namespace Zombies.Runtime.Testing
{
    public class DummyInteractable : MonoBehaviour, IInteractive
    {
        public int cost;
        public string displayText;

        public int GetCost(PlayerController player) => cost;
        public string GetDisplayText(PlayerController player) => displayText;
        public bool Interact(PlayerController player) { return true; }
    }
}