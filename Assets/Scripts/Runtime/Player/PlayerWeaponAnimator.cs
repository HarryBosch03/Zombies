using UnityEngine;
using Zombies.Runtime.Cameras;
using Random = UnityEngine.Random;

namespace Zombies.Runtime.Player
{
    [DefaultExecutionOrder(50)]
    [RequireComponent(typeof(PlayerWeapon))]
    public class PlayerWeaponAnimator : MonoBehaviour
    {
        public Transform model;
        public Vector3 basePosition;
        public Vector3 baseRotation;
        public Vector3 aimPosition;
        public Vector3 aimRotation;
        public Transform slide;
        public Vector3 slideForwardPosition;
        public Vector3 slideBackPosition;
        public int framerate = 12;

        [Space]
        public Vector3 shootPositionBase = new Vector3(-0.02f, 0.02f, -0.05f);
        public Vector3 shootPositionRange;
        public Vector3 shootRotationBase = new Vector3(-15f, 5f, 0f);
        public Vector3 shootRotationRange;
        public int shootFrameCount = 3;
        public int shootRotationFrameShift = 1;
        public int shootHoldFrames = 2;

        [Space]
        public Vector3 reloadOffset;
        public float reloadTransitionDuration;

        private PlayerWeapon weapon;
        private Vector3 shootPosition;
        private Quaternion shootRotation;
        private float clock;
        private float reloadOffsetPercent;

        private int shootFrame;

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                if (model != null)
                {
                    basePosition = model.localPosition;
                    baseRotation = model.eulerAngles;
                }

                if (slide != null)
                {
                    slideForwardPosition = slide.localPosition;
                }
            }
        }

        private void Awake()
        {
            weapon = GetComponent<PlayerWeapon>();
            foreach (var child in model.GetComponentsInChildren<Transform>())
            {
                child.gameObject.layer = 3;
            }
        }

        private void OnEnable()
        {
            weapon.ShootEvent += OnShoot;
            shootFrame = 0;
            reloadOffsetPercent = 1f;
            UpdatePose();
        }

        private void OnDisable() { weapon.ShootEvent -= OnShoot; }

        private void OnShoot()
        {
            var positionA = shootPositionBase - shootPositionRange;
            var positionB = shootPositionBase + shootPositionRange;

            var rotationA = Quaternion.Euler(shootRotationBase - shootRotationRange);
            var rotationB = Quaternion.Euler(shootRotationBase + shootRotationRange);

            var t = Random.value;
            shootPosition = Vector3.Lerp(positionA, positionB, t);
            shootRotation = Quaternion.Slerp(rotationA, rotationB, t);
            shootFrame = shootFrameCount + shootHoldFrames;
        }

        private void LateUpdate()
        {
            if (!weapon.character.isActiveViewer)
            {
                model.gameObject.SetActive(false);
                return;
            }

            model.gameObject.SetActive(true);

            if (slide != null)
            {
                slide.localPosition = shootFrame >= shootFrameCount + shootHoldFrames - 1 ? slideBackPosition : slideForwardPosition;
            }

            if (clock > 1f / framerate)
            {
                UpdatePose();

                if (shootFrame > 0) shootFrame--;
                clock -= 1f / framerate;
            }

            reloadOffsetPercent = Mathf.MoveTowards(reloadOffsetPercent, weapon.isReloading ? 1f : 0f, Time.deltaTime / reloadTransitionDuration);

            clock += Time.deltaTime;
        }

        private void UpdatePose()
        {
            var pose = Pose.identity;

            if (shootFrame > 0)
            {
                ViewportCamera.disableOffset = true;

                var tp = Mathf.Max(0f, (shootFrame - shootHoldFrames - Mathf.Min(0, shootRotationFrameShift)) / (float)shootFrameCount);
                var tr = Mathf.Max(0f, (shootFrame - shootHoldFrames + Mathf.Max(0, shootRotationFrameShift)) / (float)shootFrameCount);

                pose = new Pose
                {
                    position = shootPosition * tp,
                    rotation = Quaternion.Slerp(Quaternion.identity, shootRotation, tr),
                };
            }
            else
            {
                ViewportCamera.disableOffset = false;
            }

            var position = Vector3.Lerp(basePosition, aimPosition, weapon.aimPercent);
            var rotation = Quaternion.Slerp(Quaternion.Euler(baseRotation), Quaternion.Euler(aimRotation), weapon.aimPercent);

            model.position = weapon.character.head.TransformPoint(position + pose.position);
            model.rotation = weapon.character.head.rotation * rotation * pose.rotation;

            if (reloadOffsetPercent > float.Epsilon || weapon.isAiming) ViewportCamera.SetSpriteOffset(1, reloadOffset * reloadOffsetPercent);
            else ViewportCamera.ClearSpriteOffset(1);
        }
    }
}