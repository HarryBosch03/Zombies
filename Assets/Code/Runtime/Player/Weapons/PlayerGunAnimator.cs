using System;
using System.Collections;
using Framework.Runtime.Core;
using Framework.Runtime.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Framework.Runtime.Player.Weapons
{
    [RequireComponent(typeof(PlayerGun))]
    public class PlayerGunAnimator : MonoBehaviour
    {
        public Transform root;

        [Space]
        public PlayerGunPose idlePose;
        public PlayerGunPose aimPose;
        public PlayerGunPose dropPose;
        public PlayerGunPose forcePose;
        public Vector3 rotationCorrection;
        public bool dropOnReload = true;

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
            gun.ShootEvent += OnGunShoot;
            gun.EquipEvent += OnGunEquip;
            translationPid.position = dropPose.position;
            rotationPid.position = dropPose.eulerAngles;
        }

        private void OnDisable()
        {
            gun.ShootEvent -= OnGunShoot; 
            gun.EquipEvent -= OnGunEquip;
        }
        
        private void OnGunEquip()
        {
            translationPid.position = dropPose.position;
            rotationPid.position = dropPose.eulerAngles;
        }

        private void OnGunShoot()
        {
            if (gun != this.gun) return;

            translationPid.force += -Vector3.forward * shootImpulse;
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
            var localPosition = GetPropertyFromPose(p => p.position, Vector3.Lerp);
            var localRotation = GetPropertyFromPose(p => p.eulerAngles, Vector3.Lerp);
            rotationPid.isRotation = true;

            if (reset)
            {
                reset = false;
                translationPid.Reset();
                rotationPid.Reset();
            }

            var translationLag = GetPropertyFromPose(p => p.translationLag, Mathf.Lerp);
            var offset = Biped.view.InverseTransformVector(Biped.body.velocity) * translationLag;
            translationPid.force += offset;

            viewDelta /= Time.deltaTime;
            
            var translationSway = GetPropertyFromPose(p => p.translationSway, Mathf.Lerp);
            translationPid.force += new Vector3(viewDelta.x, viewDelta.y, 0.0f) * translationSway;
            
            var rotationSway = GetPropertyFromPose(p => p.rotationSway, Mathf.Lerp);
            rotationPid.force += new Vector3(-viewDelta.y, viewDelta.x, 0.0f) * rotationSway;
            
            viewDelta = Vector3.zero;
            
            translationPid.Update(localPosition, Time.deltaTime);
            rotationPid.Update(localRotation, Time.deltaTime);
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
            
            root.rotation = Biped.view.rotation * Quaternion.Euler(rotationPid) * Quaternion.Euler(rotationCorrection);

            var fieldOfView = Mathf.Lerp
            (
                idlePose.fieldOfView,
                aimPose.fieldOfView,
                gun.AimPercent
            );
            gun.fieldOfView = fieldOfView;
        }
        
        T GetPropertyFromPose<T>(Func<PlayerGunPose, T> get, Func<T, T, float, T> lerp)
        {
            if (forcePose) return get(forcePose);
                
            if (gun.IsReloading && dropOnReload) return get(dropPose);
            return lerp(get(idlePose), get(aimPose), gun.AimPercent);
        }
    }
}