using System;
using System.Collections;
using Framework.Runtime.Data;
using Framework.Runtime.Utility;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Framework.Runtime.Player.Weapons
{
    public class Gun : Weapon
    {
        public int ammo;
        public UnityEvent ShootUnityEvent;

        [HideInInspector]
        public Transform viewportModelRoot;
        public Transform viewport;
        public Transform world;

        private PlayerController player;
        private float lastFireTime;

        private ParticleSystem flash;
        private ParticleSystem smoke;
        private float equipTime;
        
        private bool isAiming;

        private Vector2 recoilPosition;
        private Vector2 recoilVelocity;
        
        public event Action ShootEvent;

        public GunStatSheet StatSheet => (GunStatSheet)statSheet;
        public override string AmmoLabel => ammo >= 0 ? $"{ammo}/{StatSheet.maxAmmo}" : "--/--";
        public Vector3? MuzzlePosition => viewportModelRoot && StatSheet ? viewportModelRoot.TransformPoint(StatSheet.muzzleOffset * 0.01f) : null;
        public Quaternion? MuzzleOrientation => viewportModelRoot && StatSheet ? viewportModelRoot.rotation * Quaternion.LookRotation(StatSheet.muzzleForwardDirection).normalized : null;
        public Vector3? MuzzleDirection => MuzzleOrientation * Vector3.forward;
        public float AimPercent { get; private set; }
        public bool IsReloading { get; private set; }
        public static bool ToggleAim => Settings.Index<bool>("ToggleAim");
        
        protected override void Awake()
        {
            player = GetComponentInParent<PlayerController>();

            base.Awake();

            ammo = StatSheet.maxAmmo;

            StateChangedEvent += OnStateChanged;
        }

        private void OnStateChanged(WeaponState oldState, WeaponState newState)
        {
            switch (newState)
            {
                case WeaponState.Equipped:
                {
                    equipTime = Time.time;
                    break;
                }
                case WeaponState.Unequipped:
                {
                    StopCoroutine(nameof(ReloadRoutine));
                    IsReloading = false;
                    AimPercent = 0.0f;
                    isAiming = false;
                    break;
                }
                case WeaponState.OnGround:
                {
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
                }
            }
        }

        protected override void Update()
        {
            base.Update();

            if (player) player.Biped.viewFrameOffset += recoilPosition;
        }

        protected override void UpdateEquipped()
        {
            if (Time.time - equipTime > StatSheet.equipTime)
            {
                if (ToggleAim)
                {
                    if (Player.AimAction.WasPressedThisFrame()) isAiming = !isAiming;
                }
                else
                {
                    isAiming = Player.AimAction.IsPressed();
                }

                if (Player.ReloadAction.WasPressedThisFrame())
                {
                    StartReload();
                }
                else if (IsReloading)
                {
                    isAiming = false;
                }
                else
                {
                    if (StatSheet.singleFire ? Player.ShootAction.WasPressedThisFrame() : Player.ShootAction.IsPressed())
                    {
                        Shoot();
                    }
                }

                AimPercent += (isAiming ? 1 : -1) / StatSheet.aimTime * Time.deltaTime;
                AimPercent = Mathf.Clamp01(AimPercent);

                Player.Camera.FovOverride = GunStatSheet.AimFov;
                Player.Camera.FovOverrideBlend = AimPercent;
                Player.Camera.FunctionalZoom = Mathf.Lerp(1.0f, StatSheet.aimZoom, AimPercent);
            }
        }

        private void FixedUpdate()
        {
            UpdateRecoilKinematics();
        }

        private void UpdateRecoilKinematics()
        {
            var force = -recoilPosition * StatSheet.recoilSpring - recoilVelocity * StatSheet.recoilDamping;
            
            recoilPosition += recoilVelocity * Time.deltaTime;
            recoilVelocity += force * Time.deltaTime;
        }

        private void Shoot()
        {
            if (Time.time < lastFireTime + 60.0f / StatSheet.fireRate) return;
            if (ammo == 0) return;

            StatSheet.projectile.SpawnFromPrefab(player.gameObject, StatSheet.args, MuzzlePosition.Value, player.Biped.body.velocity, MuzzleDirection.Value);
            
            if (flash) flash.Play();
            if (smoke && !smoke.isPlaying) smoke.Play();

            recoilVelocity += new Vector2
            {
                x = RandomUtils.MedianVariance(StatSheet.viewRecoilMedian.x, StatSheet.viewRecoilVariance.x),
                y = RandomUtils.MedianVariance(StatSheet.viewRecoilMedian.y, StatSheet.viewRecoilVariance.y),
            };
            
            lastFireTime = Time.time;
            ammo--;

            ShootEvent?.Invoke();
            ShootUnityEvent?.Invoke();
        }

        private void StartReload()
        {
            if (ammo >= StatSheet.maxAmmo) return;
            if (IsReloading) return;

            StartCoroutine(nameof(ReloadRoutine));
        }

        private IEnumerator ReloadRoutine()
        {
            IsReloading = true;

            if (StatSheet.ammoReloadedPerLoop < 1)
            {
                ammo = 0;
                yield return new WaitForSeconds(StatSheet.reloadLoopDuration);
                ammo = StatSheet.maxAmmo;
            }
            else
            {
                yield return new WaitForSeconds(StatSheet.preReloadDuration);
                while (ammo < StatSheet.maxAmmo)
                {
                    yield return new WaitForSeconds(StatSheet.reloadLoopDuration);
                    ammo = Mathf.Min(StatSheet.maxAmmo, ammo + StatSheet.ammoReloadedPerLoop);
                }

                yield return new WaitForSeconds(StatSheet.postReloadDuration);
            }

            IsReloading = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            if (MuzzlePosition != null && MuzzleDirection != null && MuzzleOrientation != null)
            {
                Gizmos.DrawRay(MuzzlePosition.Value, MuzzleDirection.Value * 0.08f);
                Gizmos.DrawRay(MuzzlePosition.Value + MuzzleDirection.Value * 0.08f, MuzzleOrientation.Value * new Vector3(0.0f, 1.0f, -1.0f).normalized * 0.02f);
                Gizmos.DrawRay(MuzzlePosition.Value + MuzzleDirection.Value * 0.08f, MuzzleOrientation.Value * new Vector3(1.0f, -1.0f, -1.0f).normalized * 0.02f);
                Gizmos.DrawRay(MuzzlePosition.Value + MuzzleDirection.Value * 0.08f, MuzzleOrientation.Value * new Vector3(-1.0f, -1.0f, -1.0f).normalized * 0.02f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;

            if (MuzzlePosition != null && MuzzleOrientation != null && StatSheet)
            {
                Gizmos.DrawRay(MuzzlePosition.Value, MuzzleOrientation.Value * new Vector3(StatSheet.args.spread, 0.0f, 10.0f).normalized);
                Gizmos.DrawRay(MuzzlePosition.Value, MuzzleOrientation.Value * new Vector3(-StatSheet.args.spread, 0.0f, 10.0f).normalized);
                Gizmos.DrawRay(MuzzlePosition.Value, MuzzleOrientation.Value * new Vector3(0.0f, StatSheet.args.spread, 10.0f).normalized);
                Gizmos.DrawRay(MuzzlePosition.Value, MuzzleOrientation.Value * new Vector3(0.0f, -StatSheet.args.spread, 10.0f).normalized);
            }
        }
    }
}