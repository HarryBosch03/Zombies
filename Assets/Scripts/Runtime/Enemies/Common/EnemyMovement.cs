using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Zombies.Runtime.Enemies.Common
{
    public class EnemyMovement : NetworkBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("moveSpeed")]
        private float baseMoveSpeed = 2f;
        public float pathUpdateFrequency = 2f;
        public float maintenanceDistance = 1f;
        public float reservationRadius = 1f;

        [Space]
        public Transform head;
        public LayerMask collisionMask = ~((0b1 << 6) & (0b1 << 7));
        
        private float fallingTime;
        private float pathUpdateTimer;
        private NavMeshPath path;
        private Vector3? targetPosition;
        private Vector3? pathedPosition;
        
        private Vector2 rotation;

        public float moveSpeed => baseMoveSpeed * globalSpeedModifier;
        public bool moving { get; private set; }
        public bool onGround { get; private set; }
        public Vector3 velocity { get; private set; }

        public static List<EnemyMovement> all = new();
        public static float globalSpeedModifier = 0f;

        private void Awake()
        {
            path = new NavMeshPath(); 
        }

        private void OnEnable()
        {
            all.Add(this);
        }

        private void OnDisable()
        {
            all.Remove(this);
        }

        private void FixedUpdate()
        {
            if (!IsServerStarted) return;
            
            pathUpdateTimer += Time.deltaTime;
            if (pathUpdateTimer > 1f / pathUpdateFrequency)
            {
                pathUpdateTimer -= 1f / pathUpdateFrequency;
                CalculatePath();
            }

            var ray = new Ray(transform.position + Vector3.up, Vector3.down);
            onGround = Physics.Raycast(ray, out var hit, 1.1f, collisionMask);

            if (onGround)
            {
                moving = targetPosition.HasValue && pathedPosition.HasValue;
                if (moving)
                {
                    var position = pathedPosition.Value;
                    if ((position - targetPosition.Value).magnitude < maintenanceDistance) position = (transform.position - position).normalized * maintenanceDistance + position;

                    var moveSpeed = this.moveSpeed;
                    moveSpeed += moveSpeed * globalSpeedModifier;
                    velocity = (Vector3.MoveTowards(transform.position, position, moveSpeed * Time.deltaTime) - transform.position) / Time.deltaTime;
                    transform.position += velocity * Time.deltaTime;

                    var normal = (targetPosition.Value - head.position + Vector3.up * 1.8f).normalized;
                    var ax = Mathf.Atan2(normal.x, normal.z) * Mathf.Rad2Deg;
                    var ay = Mathf.Asin(normal.y) * Mathf.Rad2Deg;
                    rotation = new Vector2(ax, -ay);
                }
                
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
                fallingTime = 0f;

                CollideWithOthers();
            }
            else
            {
                moving = false;
                transform.position += Physics.gravity * 2f * fallingTime * Time.deltaTime;
                fallingTime += Time.deltaTime;
            }
            
            transform.rotation = Quaternion.Euler(0f, rotation.x, 0f);
            if (head != null)
            {
                head.transform.rotation = Quaternion.Euler(-rotation.y, rotation.x, 0f);
            }

            SendStateToClients(new NetState
            {
                position = transform.position,
                rotation = rotation,
                velocity = velocity,
                fallingTime = fallingTime,
            });
        }

        [ObserversRpc(ExcludeServer = true)]
        private void SendStateToClients(NetState state)
        {
            transform.position = state.position;
            rotation = state.rotation;
            velocity = state.velocity;
            fallingTime = state.fallingTime;
        }

        private void CollideWithOthers()
        {
            foreach (var other in all)
            {
                if (other == this) continue;

                var vector = transform.position - other.transform.position;
                var distance = vector.magnitude;
                var totalRadius = reservationRadius + other.reservationRadius;
                if (distance > totalRadius) continue;
                var normal = distance > float.Epsilon ? vector / distance : Vector3.forward;
                var displacement = normal * (totalRadius - distance);
                transform.position += displacement * reservationRadius / totalRadius;
                other.transform.position -= displacement * other.reservationRadius / totalRadius;
            }
        }

        private void CalculatePath()
        {
            if (!targetPosition.HasValue)
            {
                path.ClearCorners();
                return;
            }

            NavMesh.CalculatePath(transform.position, targetPosition.Value, ~0, path);

            if (path.corners.Length > 1)
            {
                pathedPosition = path.corners[1];
            }
            else
            {
                pathedPosition = targetPosition.Value;
            }
        }
        
        public void ForceMoveTowards(Vector3 point)
        {
            targetPosition = point;
            pathedPosition = point;
        }
        
        public void ClearMovement() => targetPosition = null;

        public void PathTo(Vector3 position) => targetPosition = position;

        private void OnDrawGizmos()
        {
            if (path != null)
            {
                Gizmos.color = Color.yellow;
                for (var i = 0; i < path.corners.Length - 1; i++)
                {
                    var a = path.corners[i];
                    var b = path.corners[i + 1];
                    Gizmos.DrawLine(a, b);
                }
            }

            Gizmos.color = Color.green;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, new Vector3(1f, 0f, 1f));
            Gizmos.DrawWireSphere(Vector3.zero, reservationRadius);
        }

        public struct NetState
        {
            public Vector3 position;
            public Vector2 rotation;
            public Vector3 velocity;
            public float fallingTime;
        }
    }
}