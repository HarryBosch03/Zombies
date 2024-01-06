using UnityEngine;
using Zombies.Runtime.Core;

namespace Zombies.Runtime.Player
{
    [RequireComponent(typeof(PlayerGun))]
    public class PlayerGunAnimator : MonoBehaviour
    {
        public Transform root;

        [Space]
        public Vector3 localHoldPosition;
        public float translationSway;
        public float lookSway;

        public float spring;
        public float damper;
        public int subframes = 4;
        public bool reset;

        private PlayerGun gun;

        private Vector3 position;
        private Vector3 velocity;
        private Vector3 force;

        private Vector2 viewDelta;

        private float deltaTime = 0.02f;

        public PlayerController Player => gun.Player;
        public BipedalMovement Biped => Player.Biped;

        private void Awake() { gun = GetComponent<PlayerGun>(); }

        private void FixedUpdate()
        {
            var holdPosition = Biped.view.TransformVector(localHoldPosition);

            if (reset)
            {
                reset = false;
                position = Vector3.zero;
                velocity = Vector3.zero;
                force = Vector3.zero;
            }

            deltaTime = Time.deltaTime / subframes;
            for (var i = 0; i < Mathf.Max(1, subframes); i++)
            {
                force += (holdPosition - position) * spring - velocity * damper;
                force += Biped.body.GetPointVelocity(transform.position) * translationSway;

                var view = Biped.view;
                force += (view.right * viewDelta.x + view.up * viewDelta.y) * lookSway;
                viewDelta = Vector3.zero;

                Integrate();
            }
        }

        private void Integrate()
        {
            position += velocity * deltaTime;
            velocity += force * deltaTime;
            force = Vector3.zero;

            viewDelta += Player.ViewInput;
        }

        private Vector3 GetFinalPosition()
        {
            var position = this.position;
            position += root.parent.position;
            position += velocity * (Time.time - Time.fixedTime);
            return position;
        }

        private void LateUpdate()
        {
            var finalPosition = GetFinalPosition();
            root.position = finalPosition;
        }
    }
}