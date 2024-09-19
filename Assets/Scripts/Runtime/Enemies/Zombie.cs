using UnityEngine;
using Zombies.Runtime.Enemies.Common;
using Zombies.Runtime.Health;
using CharacterController = Zombies.Runtime.Player.CharacterController;
using Random = UnityEngine.Random;

namespace Zombies.Runtime.Enemies
{
    [RequireComponent(typeof(EnemyMovement))]
    public class Zombie : MonoBehaviour, IEnemyControl
    {
        private static readonly int HitReactProperty = Animator.StringToHash("hit react");
        
        private GameObject target;
        private EnemyMovement movement;
        private EnemyAttackinator attack;
        private HealthController health;

        private void Awake()
        {
            movement = GetComponent<EnemyMovement>();
            attack = GetComponent<EnemyAttackinator>();
            health = GetComponent<HealthController>();
        }

        private void OnEnable() { HealthController.OnTakeDamage += TakeDamageEvent; }

        private void OnDisable() { HealthController.OnTakeDamage -= TakeDamageEvent; }

        private void TakeDamageEvent(HealthController victim, HealthController.DamageReport report)
        {
            if (victim != health) return;
            movement.animator.SetTrigger(HitReactProperty);
        }

        private void FixedUpdate()
        {
            movement.enabled = true;
            attack.enabled = true;

            if (target == null || target.activeInHierarchy)
            {
                if (CharacterController.all.Count > 0)
                {
                    target = CharacterController.all[Random.Range(0, CharacterController.all.Count)].gameObject;
                }
            }

            if (target != null && !attack.isAttacking)
            {
                movement.PathTo(target.transform.position);
            }
            else
            {
                movement.ClearMovement();
            }

            attack.target = target;
        }

        public void PathTo(Vector3 position) => movement.PathTo(position);
        public void ForceAttack() => attack.Attack();
    }
}