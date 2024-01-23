using Framework.Runtime.Npc.StateMachines;
using Framework.Runtime.Vitality;
using UnityEngine;
using Framework.Runtime.Utility;

namespace Framework.Runtime.Npc.Enemies.States
{
    [System.Serializable]
    public class MeleeAttackBehaviour : State<BipedalNpc>
    {
        public float windUp = 0.4f;
        public float windDown = 0.8f;
        public DamageArgs damage;
        public float range;
        public float radius;

        public Transition next;
        
        private GameObject agro;
        private float timer;
        private Vector3 direction;

        public override void Enter()
        {
            agro = Blackboard.Get<GameObject>("agro");
            if (!agro)
            {
                sm.ChangeState(next());
                return;
            }

            timer = 0.0f;
            Target.LookAt(agro);
            direction = (agro.transform.position - Target.transform.position).normalized;
        }

        public override void FixedUpdate()
        {
            if (timer < windUp && timer + Time.deltaTime >= windUp)
            {
                Attack();
            }

            if (timer > windUp + windDown)
            {
                timer -= windUp + windDown;
                if (!agro || (agro.transform.position - Target.transform.position).magnitude > range)
                {
                    sm.ChangeState(next());
                }
            }

            timer += Time.deltaTime;
        }

        private void Attack()
        {
            var ray = new Ray(Target.transform.position, direction);
            var list = Physics.SphereCastAll(ray, radius, range);
            foreach (var e in list)
            {
                e.collider.Damage(new DamageInstance(damage, e.collider.transform.position, direction));
            }
        }
    }
}