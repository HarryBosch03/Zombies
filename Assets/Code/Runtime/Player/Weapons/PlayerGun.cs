using System;
using System.Collections;
using Framework.Runtime.Data;
using Framework.Runtime.Utility;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Framework.Runtime.Player.Weapons
{
    public class PlayerGun : PlayerWeapon
    {
        public int ammo;
        [FormerlySerializedAs("ShootUnityEvent")] public UnityEvent shootUnityEvent;

        [HideInInspector]
        public float fieldOfView = 50.0f;
        public Transform modelRoot;

        private float lastFireTime;

        private ParticleSystem flash;
        private ParticleSystem smoke;
        private float equipTime;
        
        private bool isAiming;

        private Vector2 recoilVelocity;
        
        public event Action EquipEvent;
        public event Action ShootEvent;

        public GunStatSheet gunStatSheet => (GunStatSheet)statSheet;
        public override string ammoLabel => ammo >= 0 ? $"{ammo}/{gunStatSheet.maxAmmo}" : "--/--";
        public Vector3? muzzlePosition => modelRoot && gunStatSheet ? modelRoot.TransformPoint(gunStatSheet.muzzleOffset * 0.01f) : null;
        public Quaternion? muzzleOrientation => modelRoot && gunStatSheet ? modelRoot.rotation * Quaternion.LookRotation(gunStatSheet.muzzleForwardDirection).normalized : null;
        public Vector3? muzzleDirection => muzzleOrientation * Vector3.forward;
        public float aimPercent { get; private set; }
        public bool isReloading { get; private set; }
        public static bool toggleAim => Settings.Index<bool>("ToggleAim");
        
        public override float viewportFieldOfView => fieldOfView;

        protected override void Awake()
        {
            base.Awake();

            flash = viewport.Find<ParticleSystem>("Flash");
            smoke = viewport.Find<ParticleSystem>("Smoke");

            ammo = gunStatSheet.maxAmmo;
        }

        public override void OnEquip()
        {
            equipTime = Time.time; 
            EquipEvent?.Invoke();
        }

        public override void OnUnequip()
        {
            StopCoroutine(nameof(ReloadRoutine));
            isReloading = false;
            aimPercent = 0.0f;
            isAiming = false;
        }

        protected override void Update()
        {
            base.Update();

            if (player) player.biped.viewRotation += recoilVelocity * Time.deltaTime;
        }

        protected override void UpdateEquipped()
        {
            if (Time.time - equipTime > gunStatSheet.equipTime)
            {
                if (toggleAim)
                {
                    if (player.aimAction.WasPressedThisFrame()) isAiming = !isAiming;
                }
                else
                {
                    isAiming = player.aimAction.IsPressed();
                }

                if (player.reloadAction.WasPressedThisFrame())
                {
                    StartReload();
                }
                else if (isReloading)
                {
                    isAiming = false;
                }
                else
                {
                    if (gunStatSheet.singleFire ? player.shootAction.WasPressedThisFrame() : player.shootAction.IsPressed())
                    {
                        Shoot();
                    }
                }

                aimPercent += (isAiming ? 1 : -1) / gunStatSheet.aimTime * Time.deltaTime;
                aimPercent = Mathf.Clamp01(aimPercent);

                player.camera.fovOverride = GunStatSheet.AimFov;
                player.camera.fovOverrideBlend = aimPercent;
                player.camera.functionalZoom = Mathf.Lerp(1.0f, gunStatSheet.aimZoom, aimPercent);
            }
        }

        private void FixedUpdate()
        {
            UpdateRecoilKinematics();
        }

        private void UpdateRecoilKinematics()
        {
            var force = -recoilVelocity * gunStatSheet.recoilDamping;
            recoilVelocity += force * Time.deltaTime;
        }

        private void Shoot()
        {
            if (Time.time < lastFireTime + 60.0f / gunStatSheet.fireRate) return;
            if (ammo == 0) return;

            gunStatSheet.projectile.SpawnFromPrefab(player.gameObject, gunStatSheet.args, muzzlePosition.Value, player.biped.body.velocity, muzzleDirection.Value);
            
            if (flash) flash.Play();
            if (smoke && !smoke.isPlaying) smoke.Play();

            recoilVelocity += new Vector2
            {
                x = RandomUtils.MedianVariance(gunStatSheet.viewRecoilMedian.x, gunStatSheet.viewRecoilVariance.x),
                y = RandomUtils.MedianVariance(gunStatSheet.viewRecoilMedian.y, gunStatSheet.viewRecoilVariance.y),
            };
            
            lastFireTime = Time.time;
            ammo--;

            ShootEvent?.Invoke();
            shootUnityEvent?.Invoke();
        }

        private void StartReload()
        {
            if (ammo >= gunStatSheet.maxAmmo) return;
            if (isReloading) return;

            StartCoroutine(nameof(ReloadRoutine));
        }

        private IEnumerator ReloadRoutine()
        {
            isReloading = true;

            if (gunStatSheet.ammoReloadedPerLoop < 1)
            {
                ammo = 0;
                yield return new WaitForSeconds(gunStatSheet.reloadLoopDuration);
                ammo = gunStatSheet.maxAmmo;
            }
            else
            {
                yield return new WaitForSeconds(gunStatSheet.preReloadDuration);
                while (ammo < gunStatSheet.maxAmmo)
                {
                    yield return new WaitForSeconds(gunStatSheet.reloadLoopDuration);
                    ammo = Mathf.Min(gunStatSheet.maxAmmo, ammo + gunStatSheet.ammoReloadedPerLoop);
                }

                yield return new WaitForSeconds(gunStatSheet.postReloadDuration);
            }

            isReloading = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            if (muzzlePosition != null && muzzleDirection != null && muzzleOrientation != null)
            {
                Gizmos.DrawRay(muzzlePosition.Value, muzzleDirection.Value * 0.08f);
                Gizmos.DrawRay(muzzlePosition.Value + muzzleDirection.Value * 0.08f, muzzleOrientation.Value * new Vector3(0.0f, 1.0f, -1.0f).normalized * 0.02f);
                Gizmos.DrawRay(muzzlePosition.Value + muzzleDirection.Value * 0.08f, muzzleOrientation.Value * new Vector3(1.0f, -1.0f, -1.0f).normalized * 0.02f);
                Gizmos.DrawRay(muzzlePosition.Value + muzzleDirection.Value * 0.08f, muzzleOrientation.Value * new Vector3(-1.0f, -1.0f, -1.0f).normalized * 0.02f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;

            if (muzzlePosition != null && muzzleOrientation != null && gunStatSheet)
            {
                Gizmos.DrawRay(muzzlePosition.Value, muzzleOrientation.Value * new Vector3(gunStatSheet.args.spread, 0.0f, 10.0f).normalized);
                Gizmos.DrawRay(muzzlePosition.Value, muzzleOrientation.Value * new Vector3(-gunStatSheet.args.spread, 0.0f, 10.0f).normalized);
                Gizmos.DrawRay(muzzlePosition.Value, muzzleOrientation.Value * new Vector3(0.0f, gunStatSheet.args.spread, 10.0f).normalized);
                Gizmos.DrawRay(muzzlePosition.Value, muzzleOrientation.Value * new Vector3(0.0f, -gunStatSheet.args.spread, 10.0f).normalized);
            }
        }
    }
}