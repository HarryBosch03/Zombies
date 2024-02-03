using Framework.Runtime.Npc.StateMachines;
using Framework.Runtime.Player;
using UnityEngine;
using Framework.Runtime.Utility;

namespace Framework.Runtime.Npc.Enemies.States
{
    [System.Serializable]
    public class ChaseBehaviour : State<BipedalNpc>
    {
        public float attackRange = 1.0f;
        
        public Transition next;

        public override void FixedUpdate()
        {
            var agro = Blackboard.Get<GameObject>("agro");
            if (!agro)
            {
                agro = LookForAgro();
                if (!agro)
                {
                    return;
                }
                Blackboard.Set("agro", agro);
            }
            
            Target.PathTo(agro.transform.position);
            if ((Target.transform.position - agro.transform.position).magnitude < attackRange)
            {
                Target.ClearPath();
                sm.ChangeState(next());
            }
        }

        private GameObject LookForAgro()
        {
            var best = PlayerController.All.Best(p => (p.transform.position - Target.transform.position).magnitude);
            return best ? best.gameObject : null;
        }
    }
}