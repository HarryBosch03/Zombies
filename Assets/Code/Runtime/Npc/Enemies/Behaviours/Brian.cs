using Zombies.Runtime.Npc.Enemies.States;
using Zombies.Runtime.Npc.StateMachines;

namespace Zombies.Runtime.Npc.Enemies.Behaviours
{
    public class Brian : NpcBehaviour<BipedalNpc>
    {
        public ChaseBehaviour chase;
        public MeleeAttackBehaviour attack;
        
        public override State<BipedalNpc> MakeTree()
        {
            chase.next = () => attack;
            attack.next = () => chase;
            
            return chase;
        }
    }
}