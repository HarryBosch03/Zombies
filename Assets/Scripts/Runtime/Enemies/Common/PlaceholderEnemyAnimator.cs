using System;
using UnityEngine;

namespace Zombies.Runtime.Enemies.Common
{
    public class PlaceholderEnemyAnimator : MonoBehaviour
    {
        public Transform root;
        public Transform leftArm;
        public Transform rightArm;
        public Transform leftLeg;
        public Transform rightLeg;
        public Transform head;

        [Space]
        [Range(0f, 1f)]
        public float headMovement = 0.8f;
        public float walkingFrequency;
        public float walkingAmplitude;
        public bool zombieArms;
        public float zombieAmplitude;
        public float zombieFrequency;
        public AnimationCurve attackRotationCurve;
        
        private EnemyMovement movement;
        private EnemyAttackinator attack;
        private float distance;
        private Vector3 lastPosition;
        private Vector3 velocity;
        private float attackTimer;

        private void Awake()
        {
            movement = GetComponent<EnemyMovement>();
            attack = GetComponent<EnemyAttackinator>();
        }

        private void OnEnable()
        {
            EnemyAttackinator.OnAttackStart += OnAttackStart;
        }

        private void OnDisable()
        {
            EnemyAttackinator.OnAttackStart -= OnAttackStart;
        }

        private void OnAttackStart(EnemyAttackinator caller)
        {
            if (caller != attack) return;
            attackTimer = 1f;
        }

        private void FixedUpdate()
        {
            velocity = (transform.position - lastPosition) / Time.deltaTime;
            velocity.y = 0f;
            lastPosition = transform.position;
        }

        private void LateUpdate()
        {
            var speed = movement.onGround ? velocity.magnitude : 0f;
            distance += speed * Time.deltaTime;
            var angle = Mathf.Sin(distance * Mathf.PI * walkingFrequency) * walkingAmplitude * speed / movement.moveSpeed;

            leftLeg.localRotation = Quaternion.Euler(angle, 0f, 0f);
            rightLeg.localRotation = Quaternion.Euler(-angle, 0f, 0f);
            if (!zombieArms)
            {
                leftArm.localRotation = rightLeg.localRotation;
                rightArm.localRotation = leftLeg.localRotation;
            }
            else
            {
                var angles = new Vector2(Mathf.Sin(Time.time * Mathf.PI * zombieFrequency), Mathf.Cos(Time.time * Mathf.PI * zombieFrequency)) * zombieAmplitude;
                
                leftArm.localRotation = Quaternion.Euler(angles.x - 90f, angles.y, 0f);
                rightArm.localRotation = Quaternion.Euler(-angles.x - 90f, -angles.y, 0f);
                
                if (attackTimer > 0f)
                {
                    var p = attackRotationCurve.Evaluate(1f - attackTimer);
                    leftArm.localRotation = Quaternion.Euler(p * -40f, 0f, 0f) * leftArm.localRotation;
                    rightArm.localRotation = Quaternion.Euler(p * -40f, 0f, 0f) * rightArm.localRotation;
                    attackTimer -= Time.deltaTime * 2f;
                }
            }

            var eulerAngles = movement.head.eulerAngles;
            if (eulerAngles.x > 180f) eulerAngles.x -= 360f;
            head.rotation = Quaternion.Euler(eulerAngles.x * headMovement, eulerAngles.y, 0f);
        }
    }
}