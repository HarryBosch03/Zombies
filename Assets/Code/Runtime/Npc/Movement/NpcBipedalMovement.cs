using System;
using UnityEngine;
using UnityEngine.AI;

namespace Framework.Runtime.Npc.Movement
{
    [RequireComponent(typeof(Rigidbody))]
    public class NpcBipedalMovement : NpcMovement
    {
        public float moveSpeed;
        public float accelerationTime;

        [Space]
        public float recalculatePathDistanceThreshold = 1.0f;
        public float pathCornerDistanceThreshold = 1.0f;

        private Rigidbody body;

        private NavMeshPath path = new();
        private int pathCornerIndex;

        private void Awake() { body = GetComponent<Rigidbody>(); }

        private void FixedUpdate() { Move(); }

        private void Move()
        {
            var target = GetDirectionToTargetPosition() * moveSpeed;
            var difference = Vector3.ClampMagnitude(target - body.velocity, moveSpeed);
            difference.y = 0.0f;

            var acceleration = 2.0f / accelerationTime;
            var force = difference * acceleration;

            body.AddForce(force);
        }

        private Vector3 GetDirectionToTargetPosition()
        {
            if (!targetPosition.HasValue) return Vector3.zero;

            if
            (
                path.corners == null ||
                pathCornerIndex >= path.corners.Length ||
                (targetPosition.Value - path.corners[^1]).magnitude > recalculatePathDistanceThreshold
            )
            {
                CalculateNewPath();
            }

            if (path.corners != null && path.corners.Length > pathCornerIndex)
            {
                var corner = path.corners[pathCornerIndex];
                if ((corner - transform.position).magnitude < pathCornerDistanceThreshold)
                {
                    pathCornerIndex++;
                }
                
                return (corner - transform.position).normalized;
            }

            return Vector3.zero;
        }

        private void CalculateNewPath()
        {
            pathCornerIndex = 0;
            if (!targetPosition.HasValue) return;
            NavMesh.CalculatePath(body.position, targetPosition.Value, ~0, path);
        }
    }
}