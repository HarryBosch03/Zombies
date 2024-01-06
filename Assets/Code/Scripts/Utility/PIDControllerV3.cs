using UnityEngine;

namespace Zombies.Runtime.Utility
{
    [System.Serializable]
    public class PidControllerV3
    {
        public float p, i, d;
        public int subframes = 1;

        [HideInInspector]
        public Vector3 position;
        [HideInInspector]
        public Vector3 velocity;
        [HideInInspector]
        public Vector3 integration;

        public void Update(Vector3 targetPosition, float deltaTime)
        {
            var subframes = Mathf.Max(this.subframes, 1);

            deltaTime /= subframes;
            for (var it = 0; it < subframes; it++)
            {
                var force = (targetPosition - position) * p + integration * i - velocity * d;

                position += velocity * deltaTime;
                velocity += force * deltaTime;

                integration += (targetPosition - position) * deltaTime;
            }
        }

        public void Reset()
        {
            position = Vector3.zero;
            velocity = Vector3.zero;
            integration = Vector3.zero;
        }

        public static implicit operator Vector3(PidControllerV3 controller) => controller.position;
    }
}