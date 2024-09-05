using UnityEngine;
using Zombies.Runtime.Entities;

namespace Zombies.Runtime.Player
{
    public class Projectile : MonoBehaviour
    {
        public HealthController.DamageArgs damage;
        public float initialSpeed = 80f;
        public float maxLifetime = 20f;
        public int spawnpointInterpolationFrames = 2;
        public ParticleSystem impactFx;
        public Transform visuals;

        private GameObject shooter;
        private Vector3 velocity;
        private float age;
        private Vector3 visualOffset;
        private Transform visualParent;
        private Vector3 physicalSpawnpoint;
        private int interpolationFrame;
        
        private Vector3 lastVisualsPosition;

        public static Projectile Spawn(Projectile prefab, GameObject shooter, Vector3 shooterVelocity, Transform physicalSpawnpoint, Transform visualSpawnpoint)
        {
            var offset = shooterVelocity * Time.fixedDeltaTime;
            var instance = Instantiate(prefab, physicalSpawnpoint.position + offset, Quaternion.LookRotation(physicalSpawnpoint.forward));
            instance.physicalSpawnpoint = physicalSpawnpoint.position + offset;
            instance.visualOffset = physicalSpawnpoint.InverseTransformPoint(visualSpawnpoint.position);
            instance.visualParent = physicalSpawnpoint;
            instance.velocity += shooterVelocity;
            instance.shooter = shooter;
            return instance;
        }

        private void Start()
        {
            velocity += transform.forward * initialSpeed;
            impactFx.gameObject.SetActive(false);
            lastVisualsPosition = visuals.position;
        }

        private void Update()
        {
            var position = transform.position;
            var visualPosition = visualParent.TransformPoint(visualOffset);
            position = position - physicalSpawnpoint + Vector3.Lerp(visualPosition, physicalSpawnpoint, (float)interpolationFrame / spawnpointInterpolationFrames);

            var nextPosition = transform.position + velocity * Time.fixedDeltaTime;
            nextPosition = nextPosition - physicalSpawnpoint + Vector3.Lerp(visualPosition, physicalSpawnpoint, (float)interpolationFrame / spawnpointInterpolationFrames);

            visuals.position = Vector3.Lerp(position,  nextPosition, (Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            Debug.DrawLine(lastVisualsPosition, visuals.position, Color.red, 20f);
            lastVisualsPosition = visuals.position;
        }

        private void FixedUpdate()
        {
            var ray = new Ray(transform.position, velocity);
            if (Physics.Raycast(ray, out var hit, velocity.magnitude * Time.deltaTime * 1.02f))
            {
                impactFx.gameObject.SetActive(true);
                impactFx.transform.SetParent(null);
                impactFx.transform.position = hit.point;
                impactFx.transform.rotation = Quaternion.LookRotation(hit.normal);
                impactFx.Play();

                var health = hit.collider.GetComponentInParent<HealthController>();
                if (health != null)
                {
                    health.TakeDamage(damage.UpdateWithContext(shooter, hit.point, hit.normal, hit.collider));
                }
                
                Destroy(gameObject);
            }

            age += Time.deltaTime;
            if (age > maxLifetime)
            {
                Destroy(gameObject);
            }
            
            transform.position += velocity * Time.deltaTime;
            velocity += Physics.gravity * Time.deltaTime;

            interpolationFrame++;
        }
    }
}