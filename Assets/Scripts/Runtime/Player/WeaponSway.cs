using UnityEngine;

namespace Zombies.Runtime.Player
{
    public class WeaponSway : MonoBehaviour
    {
        public float swayFrequency;
        public float swayRadius;
        public float swayAnisotropy;
        public Vector3 basePosition;
        public float positionSmoothing;

        private float walkDistance;
        private float lastShootTime;
        private float walkSpeed;
        private PlayerController player;

        private void Awake()
        {
            player = GetComponentInParent<PlayerController>();
        }

        private void FixedUpdate()
        {
            var actualWalkSpeed = player.onGround ? Mathf.Sqrt(player.velocity.x * player.velocity.x + player.velocity.z * player.velocity.z) : 0f;
            walkSpeed = Mathf.Lerp(walkSpeed, actualWalkSpeed, Time.deltaTime / Mathf.Max(Time.deltaTime, positionSmoothing));
            walkDistance += walkSpeed * Time.deltaTime;
        }

        private void Update()
        {
            var position = basePosition;

            var t = walkDistance * swayFrequency;
            position += new Vector3(Mathf.Sin(t * Mathf.PI), -Mathf.Abs(Mathf.Cos(t * Mathf.PI)) * Mathf.Pow(2, -swayAnisotropy)) * swayRadius * walkSpeed / player.runSpeed;

            transform.localPosition = position;
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                basePosition = transform.localPosition;
            }
        }
    }
}