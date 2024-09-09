using Zombies.Runtime.GameMeta.Chat;
using Zombies.Runtime.Player;

namespace Zombies.Runtime.GameMeta.Commands
{
    public class EquipCommand : IChatCommand
    {
        public string name => "equip";

        public string Perform(PlayerController sender, string[] args)
        {   
            sender.character.EquipWeapon(args[0]);
            return $"{sender.name} equipped {args[0]}";
        }
    }
}