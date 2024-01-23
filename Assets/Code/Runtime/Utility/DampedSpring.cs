using UnityEngine;
using UnityEngine.Serialization;

namespace Framework.Runtime.Utility
{
    [System.Serializable]
    public class DampedSpring
    {
        [FormerlySerializedAs("p")] public float spring;
        [FormerlySerializedAs("d")] public float damping;
        public float anticipation;
        public int subframes = 1;
        public bool interpolate = true;
        public bool mute;

        [HideInInspector]
        public bool isRotation;

        [HideInInspector]
        public Vector3 position;
        [HideInInspector]
        public Vector3 velocity;
        [HideInInspector]
        public Vector3 force;

        private Vector3 lastTargetPosition;
        private Vector3 lastPosition;

        public Vector3 InterpolatedPosition
        {
            get
            {
                var position = this.position;
                if (interpolate) position = Vector3.Lerp(lastPosition, position, (Time.time - Time.fixedTime) / Time.fixedDeltaTime);
                return position;
            }
        }

        public void Update(float targetPosition, float deltaTime) => Update(Vector3.right * targetPosition, deltaTime);
        public void Update(Vector2 targetPosition, float deltaTime) => Update((Vector3)targetPosition, deltaTime);
        public void Update(Vector3 targetPosition, float deltaTime)
        {
            lastPosition = position;
            
            if (mute)
            {
                position = targetPosition;
                velocity = Vector3.zero;
                lastTargetPosition = targetPosition;
                return;
            }
            
            var subframes = Mathf.Max(this.subframes, 1);

            deltaTime /= subframes;
            for (var it = 0; it < subframes; it++)
            {
                var error = CalculateDifference(targetPosition, position);
                var targetVelocity = CalculateDifference(targetPosition, lastTargetPosition) / deltaTime;
                force += error * spring + targetVelocity * anticipation - velocity * damping;

                position += velocity * deltaTime;
                velocity += force * deltaTime;
                force = Vector3.zero;
                
                lastTargetPosition = targetPosition;
            }
        }

        private Vector3 CalculateDifference(Vector3 a, Vector3 b)
        {
            if (!isRotation) return a - b;

            return new Vector3
            {
                x = Mathf.DeltaAngle(b.x, a.x),
                y = Mathf.DeltaAngle(b.y, a.y),
                z = Mathf.DeltaAngle(b.z, a.z),
            };
        }

        public void Reset()
        {
            position = Vector3.zero;
            velocity = Vector3.zero;
        }

        public static implicit operator Vector3(DampedSpring controller) => controller.InterpolatedPosition;
    }
}