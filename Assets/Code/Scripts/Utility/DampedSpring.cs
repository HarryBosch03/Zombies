using UnityEngine;
using UnityEngine.Serialization;

namespace Zombies.Runtime.Utility
{
    [System.Serializable]
    public class DampedSpring
    {
        [FormerlySerializedAs("p")] public float spring;
        [FormerlySerializedAs("d")] public float damping;
        public float anticipation;
        public int subframes = 1;

        [HideInInspector]
        public bool isRotation;

        [HideInInspector]
        public Vector3 position;
        [HideInInspector]
        public Vector3 velocity;
        [HideInInspector]
        public Vector3 lastTargetPosition;

        public void Update(float targetPosition, float deltaTime) => Update(Vector3.right * targetPosition, deltaTime);
        public void Update(Vector2 targetPosition, float deltaTime) => Update((Vector3)targetPosition, deltaTime);
        public void Update(Vector3 targetPosition, float deltaTime)
        {
            var subframes = Mathf.Max(this.subframes, 1);

            deltaTime /= subframes;
            for (var it = 0; it < subframes; it++)
            {
                var error = CalculateError(targetPosition);
                var targetVelocity = ();
                var force = error * spring + integration * anticipation - velocity * damping;

                position += velocity * deltaTime;
                velocity += force * deltaTime;

                integration += error * deltaTime;
            }
        }

        private Vector3 CalculateError(Vector3 targetPosition)
        {
            if (!isRotation) return targetPosition - position;

            return new Vector3
            {
                x = Mathf.DeltaAngle(position.x, targetPosition.x),
                y = Mathf.DeltaAngle(position.y, targetPosition.y),
                z = Mathf.DeltaAngle(position.z, targetPosition.z),
            };
        }

        public void Reset()
        {
            position = Vector3.zero;
            velocity = Vector3.zero;
            integration = Vector3.zero;
        }

        public static implicit operator Vector3(DampedSpring controller) => controller.position;
    }
}