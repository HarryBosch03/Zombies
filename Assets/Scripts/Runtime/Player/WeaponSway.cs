using UnityEngine;
using Zombies.Runtime.Cameras;

namespace Zombies.Runtime.Player
{
    public class WeaponSway : MonoBehaviour
    {
        public float swayFrequency;
        public float swayRadius;
        public float swayAnisotropy;
        public float positionSmoothing;

        private float walkDistance;
        private float lastShootTime;
        private float walkSpeed;
        private CharacterController character;

        private void Awake()
        {
            character = GetComponentInParent<CharacterController>();
        }

        private void FixedUpdate()
        {
            var actualWalkSpeed = character.onGround ? Mathf.Sqrt(character.velocity.x * character.velocity.x + character.velocity.z * character.velocity.z) : 0f;
            walkSpeed = Mathf.Lerp(walkSpeed, actualWalkSpeed, Time.deltaTime / Mathf.Max(Time.deltaTime, positionSmoothing));
            walkDistance += walkSpeed * Time.deltaTime;
        }

        private void Update()
        {
            var position = Vector2.zero;

            var t = walkDistance * swayFrequency;
            position += new Vector2(Mathf.Sin(t * Mathf.PI), -Mathf.Abs(Mathf.Cos(t * Mathf.PI)) * Mathf.Pow(2, -swayAnisotropy)) * swayRadius * walkSpeed / character.runSpeed;

            ViewportCamera.SetSpriteOffset(0, position);
        }
    }
}