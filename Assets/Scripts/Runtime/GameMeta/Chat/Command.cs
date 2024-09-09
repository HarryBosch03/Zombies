using Zombies.Runtime.Player;

namespace Zombies.Runtime.GameMeta.Chat
{
    public interface IChatCommand
    {
        public string name { get; }
        public string Perform(PlayerController sender, string[] args);
    }
}