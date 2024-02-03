using System.Collections;
using FishNet.Object;
using UnityEngine;
using Framework.Runtime.Utility;

namespace Framework.Runtime.Player
{
    public class PlayerGun : PlayerWeapon
    {
        public int ammo;

        [HideInInspector]
        public float fieldOfView = 50.0f;

        public GunStatSheet stats;

        private PlayerController player;
        private bool shootFlag;
        private bool reloadFlag;
        private float lastFireTime;

        private ParticleSystem flash;
        private ParticleSystem smoke;
        private float equipTime;

        public static event System.Action<PlayerGun> ShootEvent;

        public override string AmmoLabel => ammo >= 0 ? $"{ammo}/{stats.maxAmmo}" : "--/--";
        public Vector3 MuzzlePosition => (MainCam ? MainCam.transform : transform).position;
        public float AimPercent { get; private set; }
        public bool IsReloading { get; private set; }

        public override float ViewportFieldOfView => fieldOfView;

        protected override void Awake()
        {
            player = GetComponentInParent<PlayerController>();

            base.Awake();

            flash = viewport.Find<ParticleSystem>("Flash");
            smoke = viewport.Find<ParticleSystem>("Smoke");

            ammo = stats.maxAmmo;
        }

        public override void OnEquip() { equipTime = Time.time; }

        public override void OnUnequip()
        {
            StopCoroutine(nameof(ReloadRoutine));
            IsReloading = false;
            AimPercent = 0.0f;
        }

        protected override void UpdateEquipped()
        {
            if (Time.time - equipTime > stats.equipTime)
            {
                if (stats.singleFire)
                {
                    if (Player.ShootAction.WasPressedThisFrame()) shootFlag = true;
                }
                else shootFlag = Player.ShootAction.IsPressed();
                
                var aiming = Player.AimAction.IsPressed();
                if (reloadFlag)
                {
                    StartReload();
                }
                else if (IsReloading)
                {
                    aiming = false;
                }
                else
                {
                    if (shootFlag)
                    {
                        Shoot();
                    }
                }

                AimPercent += (aiming ? 1 : -1) / stats.aimTime * Time.deltaTime;
                AimPercent = Mathf.Clamp01(AimPercent);
                
                if (Player.ReloadAction.WasPressedThisFrame()) reloadFlag = true;
            }
        }

        private void FixedUpdate()
        {
            ResetFlags();
        }

        private void Shoot()
        {
            if (!IsOwner) return;
            if (Time.time < lastFireTime + 60.0f / stats.fireRate) return;
            if (ammo == 0) return;

            var direction = MainCam.transform.forward;
            stats.projectile.SpawnFromPrefab(player.gameObject, stats.args, MuzzlePosition, direction);
            ServerRpcShoot(MuzzlePosition, direction);

            if (flash) flash.Play();
            if (smoke && !smoke.isPlaying) smoke.Play();

            lastFireTime = Time.time;
            ammo--;

            ShootEvent?.Invoke(this);
        }

        private void StartReload()
        {
            if (ammo >= stats.maxAmmo) return;
            if (IsReloading) return;

            StartCoroutine(nameof(ReloadRoutine));
        }

        private IEnumerator ReloadRoutine()
        {
            IsReloading = true;

            if (stats.ammoReloadedPerLoop < 1)
            {
                ammo = 0;
                yield return new WaitForSeconds(stats.reloadLoopDuration);
                ammo = stats.maxAmmo;
            }
            else
            {
                yield return new WaitForSeconds(stats.preReloadDuration);
                while (ammo < stats.maxAmmo)
                {
                    yield return new WaitForSeconds(stats.reloadLoopDuration);
                    ammo = Mathf.Min(stats.maxAmmo, ammo + stats.ammoReloadedPerLoop);
                }

                yield return new WaitForSeconds(stats.postReloadDuration);
            }

            IsReloading = false;
        }

        [ServerRpc]
        private void ServerRpcShoot(Vector3 position, Vector3 direction)
        {
            ClientRpcShoot(position, direction);
            if (IsOwner) return;
            stats.projectile.SpawnFromPrefab(player.gameObject, stats.args, position, direction);
        }

        [ObserversRpc]
        private void ClientRpcShoot(Vector3 position, Vector3 direction)
        {
            if (IsOwner) return;
            stats.projectile.SpawnFromPrefab(player.gameObject, stats.args, position, direction);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.yellow;

            Gizmos.DrawRay(MuzzlePosition, new Vector3(stats.args.spread, 0.0f, 10.0f).normalized);
            Gizmos.DrawRay(MuzzlePosition, new Vector3(-stats.args.spread, 0.0f, 10.0f).normalized);
            Gizmos.DrawRay(MuzzlePosition, new Vector3(0.0f, stats.args.spread, 10.0f).normalized);
            Gizmos.DrawRay(MuzzlePosition, new Vector3(0.0f, -stats.args.spread, 10.0f).normalized);
        }

        private void ResetFlags()
        {
            shootFlag = false;
            reloadFlag = false;
        }
    }
}