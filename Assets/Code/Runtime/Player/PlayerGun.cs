using System;
using System.Collections;
using FishNet.Object;
using UnityEngine;
using Framework.Runtime.Utility;
using Random = UnityEngine.Random;

namespace Framework.Runtime.Player
{
    public class PlayerGun : PlayerWeapon
    {
        public int ammo;

        [HideInInspector]
        public float fieldOfView = 50.0f;
        public Transform modelRoot;
        public bool doForceAim;

        public GunStatSheet stats;

        private PlayerController player;
        private float lastFireTime;

        private ParticleSystem flash;
        private ParticleSystem smoke;
        private float equipTime;

        private Vector2 recoilPosition;
        private Vector2 recoilVelocity;
        
        public static event System.Action<PlayerGun> ShootEvent;

        public override string AmmoLabel => ammo >= 0 ? $"{ammo}/{stats.maxAmmo}" : "--/--";
        public Vector3? MuzzlePosition => modelRoot && stats ? modelRoot.TransformPoint(stats.muzzleOffset * 0.01f) : null;
        public Quaternion? MuzzleOrientation => modelRoot && stats ? modelRoot.rotation * Quaternion.LookRotation(stats.muzzleForwardDirection).normalized : null;
        public Vector3? MuzzleDirection => MuzzleOrientation * Vector3.forward;
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

        protected override void Update()
        {
            base.Update();

            player.Biped.viewFrameOffset += recoilPosition;
        }

        protected override void UpdateEquipped()
        {
            if (Time.time - equipTime > stats.equipTime)
            {
                var aiming = Player.AimAction.IsPressed();
                if (doForceAim) aiming = true;
                
                if (Player.ReloadAction.WasPressedThisFrame())
                {
                    StartReload();
                }
                else if (IsReloading)
                {
                    aiming = false;
                }
                else
                {
                    if (stats.singleFire ? Player.ShootAction.WasPressedThisFrame() : Player.ShootAction.IsPressed())
                    {
                        Shoot();
                    }
                }

                AimPercent += (aiming ? 1 : -1) / stats.aimTime * Time.deltaTime;
                AimPercent = Mathf.Clamp01(AimPercent);
            }
        }

        private void FixedUpdate()
        {
            UpdateRecoilKinematics();
        }

        private void UpdateRecoilKinematics()
        {
            var force = -recoilPosition * stats.recoilSpring - recoilVelocity * stats.recoilDamping;
            
            recoilPosition += recoilVelocity * Time.deltaTime;
            recoilVelocity += force * Time.deltaTime;
        }

        private void Shoot()
        {
            if (!IsOwner) return;
            if (Time.time < lastFireTime + 60.0f / stats.fireRate) return;
            if (ammo == 0) return;

            stats.projectile.SpawnFromPrefab(player.gameObject, stats.args, MuzzlePosition.Value, player.Biped.body.velocity, MuzzleDirection.Value);
            ServerRpcShoot(MuzzlePosition.Value, player.Biped.body.velocity, MuzzleDirection.Value);

            if (flash) flash.Play();
            if (smoke && !smoke.isPlaying) smoke.Play();

            recoilVelocity += new Vector2
            {
                x = RandomUtils.MedianVariance(stats.viewRecoilMedian.x, stats.viewRecoilVariance.x),
                y = RandomUtils.MedianVariance(stats.viewRecoilMedian.y, stats.viewRecoilVariance.y),
            };
            
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
        private void ServerRpcShoot(Vector3 position, Vector3 velocity, Vector3 direction)
        {
            ClientRpcShoot(position, velocity, direction);
            if (IsOwner) return;
            stats.projectile.SpawnFromPrefab(player.gameObject, stats.args, position, velocity, direction);
        }

        [ObserversRpc]
        private void ClientRpcShoot(Vector3 position, Vector3 velocity, Vector3 direction)
        {
            if (IsOwner) return;
            stats.projectile.SpawnFromPrefab(player.gameObject, stats.args, position, velocity, direction);
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

            if (MuzzlePosition != null && MuzzleOrientation != null && stats)
            {
                Gizmos.DrawRay(MuzzlePosition.Value, MuzzleOrientation.Value * new Vector3(stats.args.spread, 0.0f, 10.0f).normalized);
                Gizmos.DrawRay(MuzzlePosition.Value, MuzzleOrientation.Value * new Vector3(-stats.args.spread, 0.0f, 10.0f).normalized);
                Gizmos.DrawRay(MuzzlePosition.Value, MuzzleOrientation.Value * new Vector3(0.0f, stats.args.spread, 10.0f).normalized);
                Gizmos.DrawRay(MuzzlePosition.Value, MuzzleOrientation.Value * new Vector3(0.0f, -stats.args.spread, 10.0f).normalized);
            }
        }
        
        
    }
}