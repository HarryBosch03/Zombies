using System.Linq;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Framework.Runtime.Vitality;
using UnityEngine;
using Framework.Runtime.Utility;

namespace Framework.Runtime.Projectiles
{
    public class Projectile : MonoBehaviour
    {
        private GameObject hitFX;
        private ParticleSystem trail;

        [SyncVar]
        private ProjectileSpawnArgs args;
        
        private Vector3 velocity;
        private Vector3 force;

        private int age;
        private int pierce;

        private GameObject owner;

        public static event System.Action<Projectile, RaycastHit> HitEvent;
        public static event System.Action<Projectile, RaycastHit, IDamageable, DamageArgs> DamageEvent;
        public static event System.Action<Projectile> DespawnEvent;

        private void Awake()
        {
            hitFX = transform.FindGameObject("HitFX");
            if (hitFX) hitFX.SetActive(false);

            trail = transform.Find<ParticleSystem>("Trail");
            trail.gameObject.SetActive(false);
        }

        public void SpawnFromPrefab(GameObject owner, ProjectileSpawnArgs args, Vector3 position, Vector3 direction)
        {
            SpawnFromPrefab(owner, args, position, Quaternion.LookRotation(direction));
        }
        
        public void SpawnFromPrefab(GameObject owner, ProjectileSpawnArgs args, Vector3 position, Quaternion baseOrientation)
        {
            var instances = new Projectile[args.count];
            for (var i = 0; i < args.count; i++)
            {
                var spread = Random.insideUnitCircle * args.spread;
                var instanceOrientation = baseOrientation * Quaternion.Euler(SpreadToDeg(spread.x), SpreadToDeg(spread.y), 0.0f);

                var instance = Instantiate(this, position, instanceOrientation);
                instance.owner = owner;
                instance.SetupWithArgs(args);
                instances[i] = instance;
            }
        }

        private float SpreadToDeg(float spread)
        {
            return Mathf.Atan(spread) * Mathf.Rad2Deg;
        }

        private Vector3 GetTangent(Vector3 normal)
        {
            var a = Vector3.Cross(normal, Vector3.up);
            var b = Vector3.Cross(normal, Vector3.forward);
            return (a.magnitude > b.magnitude ? a : b).normalized;
        }

        private void SetupWithArgs(ProjectileSpawnArgs args)
        {
            if (args == null) Destroy(gameObject);
            this.args = args;

            velocity = transform.forward * args.speed;
        }

        private void FixedUpdate()
        {
            Collide();
            Iterate();

            UpdateAge();
        }

        private void UpdateAge()
        {
            if (age > args.lifetime / Time.fixedDeltaTime) Despawn(null);
            age++;
            
            if (age == 1) trail.gameObject.SetActive(true);
        }

        private void Collide()
        {
            var ray = new Ray(transform.position, velocity);
            var step = velocity.magnitude * Time.deltaTime;
            var hits = Physics.RaycastAll(ray, step * 1.02f);

            foreach (var hit in hits.OrderBy(e => e.distance))
            {
                HitEvent?.Invoke(this, hit);

                var damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    DamageEvent?.Invoke(this, hit, damageable, args.damage);
                    damageable.Damage(new DamageInstance(args.damage, hit.point, -velocity.normalized));
                }

                if (pierce == 0)
                {
                    Despawn(hit);
                    break;
                }

                pierce--;

                SpawnFX(hitFX, hit);
            }
        }

        private void Despawn(RaycastHit? hit)
        {
            if (hit != null) SpawnFX(hitFX, hit.Value);
            DespawnEvent?.Invoke(this);
            Destroy(gameObject);
        }

        private void SpawnFX(GameObject inlinePrefab, RaycastHit hit)
        {
            if (!inlinePrefab) return;

            var direction = hit.normal;
            var instance = Instantiate(inlinePrefab, hit.point, Quaternion.LookRotation(direction));
            instance.SetActive(true);

            if (trail)
            {
                trail.transform.SetParent(null);
                trail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private void Iterate()
        {
            transform.position += velocity * Time.deltaTime;
            velocity += force * Time.deltaTime;
            force = Physics.gravity * args.gravityScale;
        }
    }
}