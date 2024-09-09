using System;
using System.Collections.Generic;
using UnityEngine;
using Zombies.Runtime.Entities;

namespace Zombies.Runtime.Health
{
    public class HealthController : MonoBehaviour
    {
        public static float headshotDamageMultiplier = 3f;

        public int currentHealth = 100;
        public int maxHealth = 100;

        [Space]
        public bool canPassiveRegen;
        public float passiveRegenDelay;
        public float passiveRegenRate;
        public int passiveRegenAmount;

        [Space]
        public bool godMode;
        public bool buddhaMode;

        [Space]
        public ParticleSystem damageFx;
        public Ragdoll ragdoll;
        public Transform modelRoot;
        public List<Collider> headColliders = new();

        public static event Action<HealthController, DamageReport> OnTakeDamage;
        public static event Action<HealthController, DamageReport> OnDie;

        private float passiveRegenTimer;

        private void OnEnable() { currentHealth = maxHealth; }

        private void Update()
        {
            if (canPassiveRegen && currentHealth < maxHealth)
            {
                passiveRegenTimer += Time.deltaTime;
                while (passiveRegenTimer >= 1f / passiveRegenRate)
                {
                    currentHealth += passiveRegenAmount;
                    passiveRegenTimer -= 1f / passiveRegenRate;
                    if (currentHealth > maxHealth) currentHealth = maxHealth;
                }
            }
        }

        public DamageReport TakeDamage(DamageArgs args)
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
            if (!godMode) currentHealth -= report.finalDamage;

            if (buddhaMode && currentHealth < 1) currentHealth = 1;

            passiveRegenTimer = -passiveRegenDelay;

            report.wasLethal = currentHealth <= 0;

            OnTakeDamage?.Invoke(this, report);

            if (report.wasLethal) return Kill(report);
            else return report;
        }

        private DamageReport Kill(DamageReport report)
        {
            if (!godMode)
            {
                gameObject.SetActive(false);
                if (ragdoll != null)
                {
                    var instance = Instantiate(ragdoll, transform.position, transform.rotation);
                    instance.Spawn(modelRoot, report);
                }
            }

            OnDie?.Invoke(this, report);

            return report;
        }

        public struct DamageReport
        {
            public DamageArgs damage;
            public bool wasHeadshot;
            public bool wasLethal;
            public int finalDamage;
        }
    }
}