using UnityEngine;
using Random = UnityEngine.Random;

namespace Zombies.Runtime.Player
{
    [RequireComponent(typeof(PlayerWeapon))]
    public class PlayerWeaponAnimator : MonoBehaviour
    {
        public Transform model;
        public Vector3 basePosition;
        public Vector3 baseRotation;
        public Vector3 aimPosition;
        public Vector3 aimRotation;
        public int framerate = 12;

        [Space]
        public Vector3 shootPositionOffset = new Vector3(-0.02f, 0.02f, -0.05f);
        public Vector3 shootRotationOffset = new Vector3(-15f, 5f, 0f);
        public int shootFrameCount = 3;
        public int shootHoldFrames = 2;

        [Space]
        public Vector3 reloadOffset;
        public int reloadFrameCount;

        private PlayerWeapon weapon;
        private int xSign;
        private float clock;

        private int shootFrame;
        private int reloadFrame;
        private Pose? overridePose;

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                basePosition = model.localPosition;
                baseRotation = model.eulerAngles;
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
            weapon.ReloadStartEvent += OnReload;
            weapon.ReloadEndEvent += OnReload;
        }

        private void OnDisable()
        {
            weapon.ShootEvent -= OnShoot;
            weapon.ReloadStartEvent -= OnReload;
            weapon.ReloadEndEvent -= OnReload;
        }


        private void OnShoot()
        {
            xSign = Random.value > 0.5f ? 1 : -1;
            shootFrame = shootFrameCount;
        }

        private void OnReload() { reloadFrame = reloadFrameCount; }

        private void LateUpdate()
        {
            if (clock > 1f / framerate)
            {
                model.localPosition = basePosition;
                model.localRotation = Quaternion.Euler(baseRotation);

                if (shootFrame > -shootHoldFrames)
                {
                    var positionOffset = shootPositionOffset;
                    var rotationOffset = shootRotationOffset;

                    positionOffset.x *= xSign;
                    rotationOffset.y *= xSign;

                    var t = Mathf.Max(0f, shootFrame / (float)shootFrameCount);
                    
                    overridePose = new Pose
                    {
                        position = positionOffset * t,
                        rotation = Quaternion.Euler(rotationOffset * t),
                    };
                    
                    shootFrame--;
                }
                else if (reloadFrame > 0 || weapon.isReloading)
                {
                    var t = Mathf.Max(0f, reloadFrame / (float)reloadFrameCount);

                    if (weapon.isReloading) t = 1f - t;

                    overridePose = new Pose
                    {
                        position = reloadOffset * t,
                        rotation = Quaternion.identity,
                    };

                    reloadFrame--;
                }
                else
                {
                    overridePose = weapon.aimPercent > float.Epsilon ? new Pose
                    {
                        position = Vector3.zero,
                        rotation = Quaternion.identity,
                    }: null;
                }

                clock -= 1f / framerate;
            }
            
            if (overridePose.HasValue)
            {
                var position = Vector3.Lerp(basePosition, aimPosition, weapon.aimPercent);
                var rotation = Quaternion.Slerp(Quaternion.Euler(baseRotation), Quaternion.Euler(aimRotation), weapon.aimPercent);
                
                model.position = weapon.player.head.TransformPoint(position + overridePose.Value.position);
                model.rotation = weapon.player.head.rotation * rotation * overridePose.Value.rotation;
            }
            else
            {
                model.localPosition = basePosition;
                model.localRotation = Quaternion.Euler(baseRotation);
            }
            
            clock += Time.deltaTime;
        }
    }
}