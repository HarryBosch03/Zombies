using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zombies.Runtime.Entities
{
    public class HealthController : MonoBehaviour
    {
        public static float headshotDamageMultiplier = 3f;
        
        public int currentHealth = 100;
        public int maxHealth = 100;
        public ParticleSystem damageFx;
        public Ragdoll ragdoll;
        public Transform modelRoot;
        public List<Collider> headColliders = new();

        public static event Action<HealthController, DamageReport> OnTakeDamage;
        public static event Action<HealthController, DamageReport> OnDie;
        
        private void OnEnable() { currentHealth = maxHealth; }

        public void TakeDamage(DamageArgs args)
        {
            var report = new DamageReport();
            report.damage = args;
            report.wasHeadshot = !args.ignoreLocationalDamage && args.hitCollider != null && headColliders.Contains(args.hitCollider);

            if (damageFx != null)
            {
                damageFx.transform.position = args.point;
                damageFx.transform.rotation = Quaternion.LookRotation(args.normal);
                damageFx.Play(true);
            }

            var damage = (float)args.damage;
            if (report.wasHeadshot) damage *= headshotDamageMultiplier;
            report.finalDamage = Mathf.FloorToInt(damage);
            currentHealth -= report.finalDamage;

            report.wasLethal = currentHealth <= 0;

            OnTakeDamage?.Invoke(this, report);
            
            if (report.wasLethal)
            {
                Kill(report);
            }
        }

        private void Kill(DamageReport report)
        {
            gameObject.SetActive(false);
            if (ragdoll != null)
            {
                var instance = Instantiate(ragdoll, transform.position, transform.rotation);
                instance.Spawn(modelRoot, report.damage);
            }

            OnDie?.Invoke(this, report);
        }

        [Serializable]
        public struct DamageArgs
        {
            public int damage;
            public bool ignoreLocationalDamage;

            [HideInInspector]
            public GameObject invoker;
            [HideInInspector]
            public Vector3 point;
            [HideInInspector]
            public Vector3 normal;
            [HideInInspector]
            public Collider hitCollider;

            public DamageArgs UpdateWithContext(GameObject invoker, Vector3 point, Vector3 normal, Collider hitCollider)
            {
                var args = this;
                args.invoker = invoker;
                args.point = point;
                args.normal = normal;
                args.hitCollider = hitCollider;
                return args;
            }
        }

        public struct DamageReport
        {
            public DamageArgs damage;
            public bool wasHeadshot;
            public bool wasLethal;
            public int finalDamage;
        }

        [Serializable]
        public struct DamageZone
        {
            public Transform parent;
            public float scaling;
            public Bounds bounds;
            
            public bool Contains(Transform transform, Vector3 point)
            {
                var localPoint = (parent != null ? parent : transform).InverseTransformPoint(point);
                return bounds.Contains(localPoint);
            }
        }
    }
}