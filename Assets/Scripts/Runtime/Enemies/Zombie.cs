using System;
using UnityEngine;
using Zombies.Runtime.Enemies.Common;
using Zombies.Runtime.Health;
using CharacterController = Zombies.Runtime.Player.CharacterController;
using Random = UnityEngine.Random;

namespace Zombies.Runtime.Enemies
{
    [RequireComponent(typeof(EnemyMovement))]
    public class Zombie : MonoBehaviour
    {
        public float stunTimeOnHit = 1.5f;
        public float spawnDuration = 4f;
        public GameObject stunRing;
        public Animator animator;
        
        private GameObject target;
        private EnemyMovement movement;
        private EnemyAttackinator attack;
        private HealthController health;

        private float stunTime;
        private float spawnTimer;

        private void Awake()
        {
            movement = GetComponent<EnemyMovement>();
            attack = GetComponent<EnemyAttackinator>();
            health = GetComponent<HealthController>();
        }

        private void OnEnable()
        {
            HealthController.OnTakeDamage += TakeDamageEvent;
            spawnTimer = spawnDuration;

            movement.enabled = false;
            attack.enabled = false;
        }

        private void OnDisable()
        {
            HealthController.OnTakeDamage -= TakeDamageEvent;
        }

        private void TakeDamageEvent(HealthController victim, HealthController.DamageReport report)
        {
            if (victim != health) return;
            Stun(stunTimeOnHit);
        }

        public void Stun(float duration)
        {
            stunTime = Mathf.Max(stunTime, duration);
        }

        private void FixedUpdate()
        {
            if (spawnTimer > 0f)
            {
                spawnTimer -= Time.deltaTime;
                return;
            }

            movement.enabled = true;
            attack.enabled = true;
            
            if (stunTime >= 0f)
            {
                movement.ClearMovement();
                attack.Interrupt();
                stunTime -= Time.deltaTime;
                if (stunRing) stunRing.gameObject.SetActive(true);
                return;
            }
            if (stunRing) stunRing.gameObject.SetActive(false);
            
            if (target == null || target.activeInHierarchy)
            {
                target = CharacterController.all[Random.Range(0, CharacterController.all.Count)].gameObject;
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

        private void Update()
        {
            animator.SetFloat("move speed", movement.onGround ? movement.velocity.magnitude : 0f);
        }
    }
}