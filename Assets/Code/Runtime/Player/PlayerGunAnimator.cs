using System;
using System.Collections;
using Framework.Runtime.Core;
using Framework.Runtime.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Framework.Runtime.Player
{
    [RequireComponent(typeof(PlayerGun))]
    public class PlayerGunAnimator : MonoBehaviour
    {
        public Transform root;

        [Space]
        public Vector3 localHoldPosition;
        public Vector3 localHoldRotation;
        
        public Vector3 localAimPosition;
        public Vector3 localAimRotation;
        
        public float translationSway = -0.3f;

        [Space]
        public float shootImpulse = 300.0f;
        public Vector3 shootTorque;

        [Range(0.0f, 1.0f)]
        public float cameraInfluence = 0.1f;

        [FormerlySerializedAs("pid")]
        public DampedSpring translationPid;
        public DampedSpring rotationPid;
        public bool reset;

        public string postShootAnimationRef;
        public float postShootAnimationDelay;

        private PlayerGun gun;
        private Animator animator;
        private Vector2 viewDelta;

        public PlayerController Player => gun.Player;
        public PlayerMovement Biped => Player.Biped;
        public PlayerCameraAnimator Camera { get; private set; }

        private void Awake()
        {
            gun = GetComponent<PlayerGun>();
            animator = GetComponentInChildren<Animator>(true);
        }

        private void Start() { Camera = Player.GetComponent<PlayerCameraAnimator>(); }

        private void OnEnable()
        {
            PlayerGun.ShootEvent += OnGunShoot;
            translationPid.position = Vector3.zero;
            rotationPid.position = Biped.view.eulerAngles;
        }

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

            if (!string.IsNullOrWhiteSpace(postShootAnimationRef) && animator)
            {
                StartCoroutine(PlayPostShootAnimation());
            }
        }

        private IEnumerator PlayPostShootAnimation()
        {
            yield return new WaitForSeconds(postShootAnimationDelay);
            animator.Play(postShootAnimationRef, 0, 0.0f);
        }

        private void FixedUpdate()
        {
            var localPosition = Vector3.Lerp(localHoldPosition, localAimPosition, gun.AimPercent);
            
            var holdPosition = localPosition;
            rotationPid.isRotation = true;

            if (reset)
            {
                reset = false;
                translationPid.Reset();
                rotationPid.Reset();
            }

            var offset = Biped.view.InverseTransformVector(Biped.body.velocity) * translationSway;

            viewDelta = Vector3.zero;

            translationPid.force += offset;

            translationPid.Update(holdPosition, Time.deltaTime);
            rotationPid.Update(Vector2.zero, Time.deltaTime);
        }

        private void Update()
        {
            if (gun.Equipped)
            {
                Camera.RotationOffset = Quaternion.Slerp(Quaternion.identity, rotationPid.position.Euler(), cameraInfluence);
            }
        }

        private Vector3 GetFinalPosition()
        {
            var position = Biped.view.TransformVector(translationPid.InterpolatedPosition);
            position += root.parent.position;
            return position;
        }

        private void LateUpdate()
        {
            var finalPosition = GetFinalPosition();
            root.position = finalPosition;

            viewDelta += Player.ViewInput;

            var localRotation = Vector3.Lerp(localHoldRotation, localAimRotation, gun.AimPercent);

            var rotation = Biped.view.rotation * Quaternion.Euler(rotationPid);
            root.rotation = rotation * Quaternion.Euler(localRotation);
        }
    }
}