using Zombies.Runtime.Player;

namespace Zombies.Runtime.Interactive
{
    public interface IInteractive
    {
        public bool isActiveAndEnabled { get; }
        
        public int GetCost(PlayerController player);
        public string GetDisplayText(PlayerController player);
        public bool Interact(PlayerController player);
    }
}