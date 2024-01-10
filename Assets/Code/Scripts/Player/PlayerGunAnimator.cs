using System;
using UnityEngine;
using UnityEngine.Serialization;
using Zombies.Runtime.Core;
using Zombies.Runtime.Utility;
using Random = UnityEngine.Random;

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

        [Space]
        public float shootImpulse;
        public Vector3 shootTorque;

        [FormerlySerializedAs("pid")]
        public DampedSpring translationPid;
        public DampedSpring rotationPid;
        public bool reset;

        private PlayerGun gun;
        private Vector2 viewDelta;

        public PlayerController Player => gun.Player;
        public PlayerMovement Biped => Player.Biped;

        private void Awake() { gun = GetComponent<PlayerGun>(); }

        private void OnEnable() { PlayerGun.ShootEvent += OnGunShoot; }

        private void OnDisable() { PlayerGun.ShootEvent -= OnGunShoot; }

        private void OnGunShoot(PlayerGun gun)
        {
            if (gun != this.gun) return;

            var view = Biped.view;
            translationPid.force += -view.forward * shootImpulse;
            rotationPid.force +=
            (
                Vector3.right * shootTorque.x +
                Vector3.up * Random.Range(-1.0f, 1.0f) * shootTorque.y +
                Vector3.forward * Random.Range(-1.0f, 1.0f) * shootTorque.z
            ) / Time.fixedDeltaTime * Mathf.Rad2Deg;
        }

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

            var offset = Biped.body.velocity * translationSway;

            viewDelta = Vector3.zero;

            translationPid.force += offset;

            translationPid.Update(holdPosition, Time.deltaTime);
            rotationPid.Update(Biped.view.eulerAngles, Time.deltaTime);
        }

        private Vector3 GetFinalPosition()
        {
            var position = translationPid.InterpolatedPosition;
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