using Framework.Runtime.Projectiles;
using UnityEngine;

namespace Framework.Runtime.Player
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Player/GunStatSheet")]
    public class GunStatSheet : ScriptableObject
    {
        public Projectile projectile;
        public ProjectileSpawnArgs args;
        public bool singleFire = false;
        public float fireRate = 180.0f;
        public float aimTime = 0.15f;
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
    }
}