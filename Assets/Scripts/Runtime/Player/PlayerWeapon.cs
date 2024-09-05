using System;
using System.Collections;
using UnityEngine;
using Zombies.Runtime.Cameras;
using Zombies.Runtime.Entities;
using Random = UnityEngine.Random;

namespace Zombies.Runtime.Player
{
    [DefaultExecutionOrder(50)]
    public class PlayerWeapon : MonoBehaviour
    {
        public Projectile projectile;
        public HealthController.DamageArgs damage;
        public Transform physicalSpawnpoint;
        public Transform visualSpawnpoint;
        public float fireRate = 600f;
        public bool automatic;

        [Space]
        public int currentMagazine;
        public int magazineSize;
        public float reloadTime;
        public float aimFieldOfView = 70f;
        public float defaultViewportFieldOfView = 50f;
        public float aimViewportFieldOfView = 10f;
        public float aimViewportCenterPlane;

        [Space]
        public Vector2 recoilBase;
        public Vector2 recoilVariance;
        public float recoilSpring;
        public float recoilDamping;

        [Space]
        public float aimDuration;
        public bool forceAim;

        private float shootTimer;
        private bool shoot;

        public event Action ShootEvent;
        public event Action ReloadStartEvent;
        public event Action ReloadEndEvent;

        public PlayerController player { get; private set; }
        public float lastShootTime { get; private set; }
        public bool isReloading { get; private set; }
        public float reloadPercent { get; private set; }
        public float aimPercent { get; private set; }
        public bool isAiming { get; set; }
        
        private void Awake() { player = GetComponentInParent<PlayerController>(); }

        private void OnEnable()
        {
            if (currentMagazine == 0)
            {
                Reload();
            }
        }

        private void FixedUpdate()
        {
            if (shoot && !isReloading)
            {
                if (currentMagazine > 0)
                {
                    if (shootTimer <= 0f)
                    {
                        ShootEvent?.Invoke();
                        var instance = Projectile.Spawn(projectile, player.gameObject, player.velocity, physicalSpawnpoint, visualSpawnpoint ? visualSpawnpoint : physicalSpawnpoint);
                        instance.damage = damage;
                        shootTimer += 60f / fireRate;
                        lastShootTime = Time.time;
                        currentMagazine--;

                        var recoilForce = new Vector2
                        {
                            x = recoilBase.x + Random.Range(-recoilVariance.x, recoilVariance.x),
                            y = recoilBase.y + Random.Range(-recoilVariance.y, recoilVariance.y),
                        };
                        player.AddRecoil(recoilForce, recoilSpring, recoilDamping);
                    }
                }
                else
                {
                    Reload();
                }
            }

            aimPercent = Mathf.MoveTowards(aimPercent, (isAiming || forceAim) && !isReloading ? 1f : 0f, Time.deltaTime / Mathf.Max(Time.deltaTime, aimDuration));
            SetFieldOfView();

            if (shootTimer > 0f) shootTimer -= Time.deltaTime;
            else shootTimer = 0f;

            if (!automatic) shoot = false;
        }

        private void SetFieldOfView()
        {
            player.overrideFieldOfViewValue = aimFieldOfView;
            player.overrideFieldOfViewBlending = aimPercent;
            ViewportCamera.viewportFieldOfView =  Mathf.Lerp(defaultViewportFieldOfView, aimViewportFieldOfView, aimPercent);
            ViewportCamera.referenceFieldOfView = defaultViewportFieldOfView;
            ViewportCamera.centerPlane = aimViewportCenterPlane;
        }

        public void SetShoot(bool input)
        {
            if (automatic) shoot = input;
            else if (input) shoot = true;
        }

        public void Reload() => StartCoroutine(ReloadRoutine());

        private IEnumerator ReloadRoutine()
        {
            if (isReloading || currentMagazine >= magazineSize) yield break;

            isReloading = true;
            currentMagazine = 0;
            
            reloadPercent = 0f;
            ReloadStartEvent?.Invoke();
            
            while (reloadPercent < 1f)
            {
                reloadPercent += Time.deltaTime / reloadTime;
                yield return null;
            }

            reloadPercent = 0f;
            currentMagazine = magazineSize;
            isReloading = false;
            
            ReloadEndEvent?.Invoke();
        }

        private void OnDrawGizmos()
        {
            if (visualSpawnpoint != null)
            {
                var gizmoSize = 0.1f;
                Gizmos.matrix = visualSpawnpoint.localToWorldMatrix;
                Gizmos.color = Color.red;
                Gizmos.DrawLine(-Vector3.right * gizmoSize, Vector3.right * gizmoSize);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(-Vector3.up * gizmoSize, Vector3.up * gizmoSize);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(-Vector3.forward * gizmoSize, Vector3.forward * gizmoSize);
            }
            
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawCube(Vector3.forward * aimViewportCenterPlane, new Vector3(1f, 1f, 0f));
        }
    }
}