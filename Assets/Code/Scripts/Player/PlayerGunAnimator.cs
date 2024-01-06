using UnityEngine;
using Zombies.Runtime.Core;
using Zombies.Runtime.Utility;

namespace Zombies.Runtime.Player
{
    [RequireComponent(typeof(PlayerGun))]
    public class PlayerGunAnimator : MonoBehaviour
    {
        public Transform root;

        [Space]
        public Vector3 localHoldPosition;
        public float translationSway = -0.05f;
        public bool interpolate = true;

        public PidControllerV3 pid;
        public bool reset;

        private PlayerGun gun;
        private Vector2 viewDelta;
        
        private Vector3 lastPosition;

        public PlayerController Player => gun.Player;
        public BipedalMovement Biped => Player.Biped;

        private void Awake() { gun = GetComponent<PlayerGun>(); }

        private void FixedUpdate()
        {
            lastPosition = pid.position;
            var holdPosition = Biped.view.TransformVector(localHoldPosition);

            if (reset)
            {
                reset = false;
                pid.Reset();
            }
            
            var offset = Biped.body.GetPointVelocity(transform.position) * translationSway;

            viewDelta = Vector3.zero;

            pid.position += offset * Time.deltaTime;
            
            pid.Update(holdPosition, Time.deltaTime);
        }

        private Vector3 GetFinalPosition()
        {
            var position = pid.position;
            if (interpolate) position = Vector3.Lerp(lastPosition, position, (Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            position += root.parent.position;
            return position;
        }

        private void LateUpdate()
        {
            var finalPosition = GetFinalPosition();
            root.position = finalPosition;

            viewDelta += Player.ViewInput;
        }
    }
}