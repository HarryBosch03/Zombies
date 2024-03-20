using Framework.Runtime.Core;
using Framework.Runtime.Vitality;
using UnityEngine;
using UnityEngine.AI;

namespace Framework.Runtime.Npc
{
    [RequireComponent(typeof(PlayerMovement), typeof(HealthController))]
    [SelectionBase, DisallowMultipleComponent]
    public class BipedalNpc : MonoBehaviour
    {
        private const float PathingThreshold = 1.0f;

        private NavMeshPath navPath;
        private int navPathIndex;

        private bool pathActive => navPathIndex < (navPath?.corners.Length ?? 0);

        public PlayerMovement movement { get; private set; }
        public HealthController health { get; private set; }
        public Rigidbody body => movement.body;

        public bool faceMovement { get; set; } = true;
        public float moveSpeed { get; set; } = 1.0f;
        
        private void Awake()
        {
            movement = GetComponent<PlayerMovement>();
            health = GetComponent<HealthController>();

            navPath = new NavMeshPath();
        }

        private void FixedUpdate()
        {
            UpdatePath();
            Move();
        }

        private void UpdatePath()
        {
            if (!pathActive) return;

            var corner = navPath.corners[navPathIndex];
            if ((corner - transform.position).magnitude < PathingThreshold)
            {
                navPathIndex++;
            }
        }

        private void Move()
        {
            var targetPosition = pathActive ? navPath.corners[navPathIndex] : transform.position;

            var direction = targetPosition - transform.position;
            direction.y = 0.0f;
            movement.moveInput = direction * moveSpeed;

            if (faceMovement && direction.magnitude > 0.1f)
            {
                LookDirection(direction.normalized);
            }
        }

        public void ClearPath()
        {
            navPath.ClearCorners();
        }
        
        public void PathTo(Vector3 position)
        {
            if ((position - transform.position).magnitude < PathingThreshold)
            {
                navPath.ClearCorners();
                return;
            }
            
            if (pathActive)
            {
                var end = navPath.corners[^1];
                if ((end - position).magnitude < PathingThreshold) return;
            }

            NavMesh.CalculatePath(transform.position, position, ~0, navPath);
            navPathIndex = 0;
        }

        public void LookAt(GameObject gameObject)
        {
            var center = IPersonality.LookTargetOf(gameObject);
            LookDirection(center - movement.view.transform.position);
        }
        
        public void LookDirection(Vector3 direction)
        {
            direction.Normalize();
            movement.viewRotation = new Vector2
            {
                x = Mathf.Atan2(direction.x, direction.z),
                y = Mathf.Asin(direction.y)
            } * Mathf.Rad2Deg;
        }

        private void OnDrawGizmosSelected()
        {
            if (pathActive)
            {
                Gizmos.color = Color.yellow;
                
                for (var i = navPathIndex; i < navPath.corners.Length - 1; i++)
                {
                    Gizmos.DrawLine(navPath.corners[i], navPath.corners[i + 1]);
                }
                
                for (var i = navPathIndex; i < navPath.corners.Length; i++)
                {
                    Gizmos.DrawSphere(navPath.corners[i], 0.2f);
                }
            }
        }
    }
}