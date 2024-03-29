﻿using Framework.Runtime.Core;
using Framework.Runtime.Npc.StateMachines;
using Framework.Runtime.Projectiles;
using UnityEngine;

namespace Framework.Runtime.Npc.Enemies.States
{
    [System.Serializable]
    public class ShootAtAgro : State<BipedalNpc>
    {
        public int windUp = 40;
        public int volleyCount = 3;
        public int shootDelay = 5;
        public int windDown = 20;
        
        [Space]
        public ProjectileSpawnArgs projectileSpawnArgs;
        public Projectile projectile;
        public Transform spawnPoint;

        public Transition next;
        private GameObject agro;

        public int counter;
        public int timer;

        public override void Enter()
        {
            counter = 0;
            timer = 0;
            
            agro = Blackboard.Get<GameObject>("agro");
            if (!agro)
            {
                sm.ChangeState(next());
            }
        }

        private void SpawnProjectile()
        {
            var direction = (IPersonality.LookTargetOf(agro) - spawnPoint.position).normalized;
            projectile.SpawnFromPrefab(Target.gameObject, projectileSpawnArgs, spawnPoint.position, Target.body.velocity, direction);
        }

        public override void FixedUpdate()
        {
            if (counter == 0)
            {
                if (timer > windUp)
                {
                    timer = 0;
                    counter++;
                }
            }
            else if (counter - 1 < volleyCount)
            {
                if (timer == 1)
                {
                    SpawnProjectile();
                }
                
                if (timer > shootDelay)
                {
                    timer = 0;
                    counter++;
                }
            }
            else
            {
                if (timer > windDown)
                {
                    sm.ChangeState(next());
                }
            }
            
            timer++;
            
            Target.LookAt(agro);
        }
    }
}