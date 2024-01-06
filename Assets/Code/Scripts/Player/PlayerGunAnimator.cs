using UnityEngine;
using UnityEngine.Serialization;
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
        public Vector3 localHoldRotation;
        public float translationSway = -0.05f;

        [FormerlySerializedAs("pid")]
        public DampedSpring translationPid;
        public DampedSpring rotationPid;
        public bool reset;

        private PlayerGun gun;
        private Vector2 viewDelta;

        public PlayerController Player => gun.Player;
        public BipedalMovement Biped => Player.Biped;

        private void Awake() { gun = GetComponent<PlayerGun>(); }

        private void FixedUpdate()
        {
            var holdPosition = Biped.view.TransformVector(localHoldPosition);
            rotationPid.isRotation = true;
            
            if (reset)
            {
                reset = false;
                translationPid.Reset();
                rotationPid.Reset();
            }

            var offset = Biped.body.GetPointVelocity(transform.position) * translationSway;

            viewDelta = Vector3.zero;

            translationPid.position += offset * Time.deltaTime;
            
            translationPid.Update(holdPosition, Time.deltaTime);
            rotationPid.Update(Biped.view.eulerAngles, Time.deltaTime);
        }

        private Vector3 GetFinalPosition()
        {
            var position = translationPid.position;
            position += root.parent.position;
            return position;
        }

        private void LateUpdate()
        {
            var finalPosition = GetFinalPosition();
            root.position = finalPosition;

            viewDelta += Player.ViewInput;

            var rotation = Quaternion.Euler(rotationPid);

            root.rotation = rotation * Quaternion.Euler(localHoldRotation);
        }
    }
}