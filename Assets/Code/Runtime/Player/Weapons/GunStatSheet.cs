using Framework.Runtime.Projectiles;
using UnityEngine;

namespace Framework.Runtime.Player.Weapons
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Player/GunStatSheet")]
    public class GunStatSheet : WeaponStatSheet
    {
        public const float AimFov = 70.0f;
        
        public Projectile projectile;
        public ProjectileSpawnArgs args;
        public bool singleFire = false;
        public float fireRate = 180.0f;
        public float aimTime = 0.15f;
        public float aimZoom = 1.0f;
        public Vector3 muzzleOffset;
        public Vector3 muzzleForwardDirection = Vector3.forward;

        [Space]
        public int maxAmmo = 1;
        public int ammoReloadedPerLoop = 0;
        public float preReloadDuration = 0.0f;
        public float reloadLoopDuration = 1.0f;
        public float postReloadDuration = 0.0f;


        [Space]
        public float equipTime = 0.2f;

        [Space] 
        public Vector2 viewRecoilMedian;
        public Vector2 viewRecoilVariance;
        public float recoilSpring;
        public float recoilDamping;
    }
}