using System;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using Zombies.Runtime.Entities;

namespace Zombies.Runtime.Health
{
    public class HealthController : NetworkBehaviour
    {
        public static float headshotDamageMultiplier = 3f;

        public readonly SyncVar<int> currentHealth = new SyncVar<int>();
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

        public override void OnStartServer()
        {
            currentHealth.Value = maxHealth;
        }

        private void Update()
        {
            if (IsServerStarted && canPassiveRegen && currentHealth.Value < maxHealth)
            {
                passiveRegenTimer += Time.deltaTime;
                while (passiveRegenTimer >= 1f / passiveRegenRate)
                {
                    currentHealth.Value += passiveRegenAmount;
                    passiveRegenTimer -= 1f / passiveRegenRate;
                    if (currentHealth.Value > maxHealth) currentHealth.Value = maxHealth;
                }
            }
        }

        public void TakeDamage(DamageArgs args)
        {
            if (!IsServerStarted) return;

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
            if (!godMode) currentHealth.Value -= report.finalDamage;

            if (buddhaMode && currentHealth.Value < 1) currentHealth.Value = 1;

            passiveRegenTimer = -passiveRegenDelay;

            report.wasLethal = currentHealth.Value <= 0;

            NotifyTakeDamageRpc(report);

            if (report.wasLethal) SetDead(report);
        }

        [ObserversRpc(ExcludeServer = false, RunLocally = true)]
        private void NotifyTakeDamageRpc(DamageReport report) => OnTakeDamage?.Invoke(this, report);

        private void SetDead(DamageReport report)
        {
            if (!IsServerStarted) return;
            
            if (!godMode)
            {
                SetDeadRpc(report);
            }
        }

        [ObserversRpc(RunLocally = true)]
        private void SetDeadRpc(DamageReport report)
        {
            gameObject.SetActive(false);
            if (ragdoll != null)
            {
                var instance = Instantiate(ragdoll, transform.position, transform.rotation);
                instance.Spawn(modelRoot, report);
            }
            OnDie?.Invoke(this, report);
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