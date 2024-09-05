using UnityEngine;
using Zombies.Runtime.Enemies.Common;
using Zombies.Runtime.Player;
using Random = UnityEngine.Random;

namespace Zombies.Runtime.Enemies
{
    [RequireComponent(typeof(EnemyMovement))]
    public class Zombie : MonoBehaviour
    {
        private GameObject target;
        private EnemyMovement movement;
        private EnemyAttackinator attack;

        private void Awake()
        {
            movement = GetComponent<EnemyMovement>();
            attack = GetComponent<EnemyAttackinator>();
        }

        private void FixedUpdate()
        {
            if (target == null || target.activeInHierarchy)
            {
                target = PlayerController.all[Random.Range(0, PlayerController.all.Count)].gameObject;
            }

            if (target != null && !attack.isAttacking)
            {
                movement.MoveTowards(target.transform.position);
            }
            else
            {
                movement.ClearMovement();
            }

            attack.target = target;
        }
    }
}