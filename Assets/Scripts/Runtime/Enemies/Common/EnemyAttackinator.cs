using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zombies.Runtime.Entities;
using Zombies.Runtime.Health;

namespace Zombies.Runtime.Enemies.Common
{
    public class EnemyAttackinator : MonoBehaviour
    {
        public DamageArgs damage;

        public float attackRangeMin;
        public float attackRangeMax;
        public float preDelay;
        public float postDelay;

        private Stack<IEnumerator> taskList = new();
        
        public event Action OnAttackStart;
        public event Action<DamageArgs> OnAttackLand;
        public event Action OnAttackEnd;

        public GameObject target { get; set; }
        public bool isAttacking { get; private set; }

        private void FixedUpdate()
        {
            if (taskList.Count == 0) taskList.Push(WaitUntilTargetInRange());
            var task = taskList.Peek();
            if (task.MoveNext())
            {
                if (task.Current != null) taskList.Push((IEnumerator)task.Current);
            }
            else
            {
                taskList.Pop();
            }
        }

        public void Interrupt()
        {
            taskList.Clear();
        }
        
        public void ForceAttack(Action<DamageArgs> onAttackLand, Action onAttackEnd)
        {
            taskList.Clear();
            taskList.Push(Attack(null, onAttackLand, onAttackEnd));
        }

        public IEnumerator WaitUntilTargetInRange()
        {
            while (target == null || (target.transform.position - transform.position).magnitude > attackRangeMin)
            {
                yield return null;
            }

            yield return Attack(OnAttackStart, LandAttack, OnAttackEnd);
        }

        public IEnumerator Attack(Action onAttackStart, Action<DamageArgs> onAttackLand, Action onAttackEnd)
        {
            isAttacking = true;
            onAttackStart?.Invoke();

            yield return Wait(preDelay);

            onAttackLand?.Invoke(damage);

            yield return Wait(postDelay);
            isAttacking = false;

            onAttackEnd?.Invoke();
        }

        public void LandAttack(DamageArgs args)
        {
            if ((target.transform.position - transform.position).magnitude < attackRangeMax)
            {
                var health = target.GetComponent<HealthController>();
                if (health != null)
                {
                    health.TakeDamage(damage.UpdateWithContext(gameObject, target.transform.position, (transform.position - target.transform.position).normalized, null));
                }
            }

            OnAttackLand?.Invoke(damage);
        }

        public static IEnumerator Wait(float duration)
        {
            var t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                yield return null;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, new Vector3(1f, 0f, 1f));

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Vector3.zero, attackRangeMin);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Vector3.zero, attackRangeMax);
        }
    }
}