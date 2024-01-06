using UnityEngine;

namespace Zombies.Runtime.Utility
{
    [System.Serializable]
    public class PidControllerV3
    {
        public float p, i, d;

        [HideInInspector]
        public Vector3 position;
        public Vector3 velocity;

        private Vector3 lastTargetPosition;

        public void Update(Vector3 targetPosition, float deltaTime)
        {
            var targetVelocity = (targetPosition - lastTargetPosition) / deltaTime;
            Update(targetPosition, targetVelocity, deltaTime);
        }
        
        public void Update(Vector3 targetPosition, Vector3 targetVelocity, float deltaTime)
        {
            var force = (targetPosition - position) * p + targetVelocity * i + (targetVelocity - velocity) * d;

            position += velocity * deltaTime;
            velocity += force * deltaTime;
            
            lastTargetPosition = targetPosition;
        }

        public static implicit operator Vector3(PidControllerV3 controller) => controller.position;
    }
}