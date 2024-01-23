using FishNet.Object;
using Framework.Runtime.Player;
using Framework.Runtime.Projectiles;
using UnityEngine;
using Framework.Runtime.Utility;

namespace Framework.Runtime.Player
{
    public class PlayerGun : PlayerWeapon
    {
        public Projectile projectile;
        public ProjectileSpawnArgs args;
        public bool singleFire = false;
        public float fireRate = 180.0f;
        public float aimTime = 0.15f;
        public float aimFov = 15.0f;
        public bool forceAim;

        [Space]
        public int ammo;
        public int maxAmmo = -1;

        [Space]
        public Vector3 muzzleOffset;

        private PlayerController player;
        private bool shootFlag;
        private float lastFireTime;

        private ParticleSystem flash;
        private ParticleSystem smoke;

        public static event System.Action<PlayerGun> ShootEvent;

        public override string AmmoLabel => ammo >= 0 ? $"{ammo}/{maxAmmo}" : "--/--";
        public Vector3 MuzzlePosition => (MainCam ? MainCam.transform : transform).TransformPoint(muzzleOffset);
        public float AimPercent { get; private set; }

        public override float ViewportFieldOfView => Mathf.Lerp(base.ViewportFieldOfView, aimFov, AimPercent);

        protected override void Awake()
        {
            player = GetComponentInParent<PlayerController>();

            base.Awake();

            flash = viewport.Find<ParticleSystem>("Flash");
            smoke = viewport.Find<ParticleSystem>("Smoke");

            ammo = maxAmmo;
        }

        protected override void UpdateEquipped()
        {
            if (singleFire)
            {
                if (Player.ShootAction.WasPressedThisFrame()) shootFlag = true;
            }
            else shootFlag = Player.ShootAction.IsPressed();
        }

        private void FixedUpdate()
        {
            if (Equipped)
            {
                if (shootFlag)
                {
                    Shoot();
                }

                var aiming = Player.AimAction.IsPressed() || forceAim;
                AimPercent += (aiming ? 1 : -1) / aimTime * Time.deltaTime;
                AimPercent = Mathf.Clamp01(AimPercent);
            }
            
            ResetFlags();
        }

        private void Shoot()
        {
            if (!IsOwner) return;
            if (Time.time < lastFireTime + 60.0f / fireRate) return;
            if (ammo == 0) return;

            var direction = MainCam.transform.forward;
            projectile.SpawnFromPrefab(player.gameObject, args, MuzzlePosition, direction);
            ServerRpcShoot(MuzzlePosition, direction);

            if (flash) flash.Play();
            if (smoke && !smoke.isPlaying) smoke.Play();

            lastFireTime = Time.time;
            ammo--;

            ShootEvent?.Invoke(this);
        }

        [ServerRpc]
        private void ServerRpcShoot(Vector3 position, Vector3 direction)
        {
            ClientRpcShoot(position, direction);
            if (IsOwner) return;
            projectile.SpawnFromPrefab(player.gameObject, args, position, direction);
        }

        [ObserversRpc]
        private void ClientRpcShoot(Vector3 position, Vector3 direction)
        {
            if (IsOwner) return;
            projectile.SpawnFromPrefab(player.gameObject, args, position, direction);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(muzzleOffset, 0.04f);

            Gizmos.DrawRay(MuzzlePosition, new Vector3(args.spread, 0.0f, 10.0f).normalized);
            Gizmos.DrawRay(MuzzlePosition, new Vector3(-args.spread, 0.0f, 10.0f).normalized);
            Gizmos.DrawRay(MuzzlePosition, new Vector3(0.0f, args.spread, 10.0f).normalized);
            Gizmos.DrawRay(MuzzlePosition, new Vector3(0.0f, -args.spread, 10.0f).normalized);
        }

        private void ResetFlags() { shootFlag = false; }
    }
}