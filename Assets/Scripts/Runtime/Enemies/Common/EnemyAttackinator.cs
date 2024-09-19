using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zombies.Runtime.Health;

namespace Zombies.Runtime.Enemies.Common
{
    public class EnemyAttackinator : MonoBehaviour
    {
        public DamageArgs damage;

        public float attackRangeMin;
        public float attackRangeMax;
        
        [Space]
        public Animator animator;

        public static event Action<EnemyAttackinator> OnAttackStart;
        public static event Action<EnemyAttackinator, DamageArgs> OnAttackLand;
        public static event Action<EnemyAttackinator> OnAttackEnd;

        public GameObject target { get; set; }
        public bool isAttacking { get; private set; }
        public bool hasLanded { get; private set; }

        private void FixedUpdate()
        {
            if (!isAttacking && target != null && (target.transform.position - transform.position).magnitude < attackRangeMin)
            {
                Attack();
            }
        }

        public void Attack()
        {
            isAttacking = true;
            
            if (animator != null) animator.SetTrigger("attack");
            OnAttackStart?.Invoke(this);
        }

        public void LandAttack()
        {
            var isFrozen = animator.GetCurrentAnimatorStateInfo(0).IsTag("Freeze");
            if (target != null && (target.transform.position - transform.position).magnitude < attackRangeMax && !isFrozen)
            {
                var health = target.GetComponent<HealthController>();
                if (health != null)
                {
                    health.TakeDamage(damage.UpdateWithContext(gameObject, target.transform.position, (transform.position - target.transform.position).normalized, null));
                }
            }

            hasLanded = true;
            OnAttackLand?.Invoke(this, damage);
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, new Vector3(1f, 0f, 1f));

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Vector3.zero, attackRangeMin);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Vector3.zero, attackRangeMax);
        }

        public void EndAttack()
        {
            OnAttackEnd?.Invoke(this);
            isAttacking = false;
            hasLanded = false;
        }
    }
}