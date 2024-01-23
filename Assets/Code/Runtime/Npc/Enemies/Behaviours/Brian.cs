using Framework.Runtime.Npc.Enemies.States;
using Framework.Runtime.Npc.StateMachines;

namespace Framework.Runtime.Npc.Enemies.Behaviours
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