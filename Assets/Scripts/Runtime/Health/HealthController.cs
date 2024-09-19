using System;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.Serialization;
using Zombies.Runtime.Entities;
using Zombies.Runtime.GameControl;

namespace Zombies.Runtime.Health
{
    public class HealthController : NetworkBehaviour
    {
        public static float headshotDamageMultiplier = 3f;

        [FormerlySerializedAs("maxHealth")]
        public int baseMaxHealth = 50;
        public int maxHealthPerRound = 100;
        
        public readonly SyncVar<int> currentHealth = new SyncVar<int>();
        public readonly SyncVar<int> maxHealth = new SyncVar<int>();

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
            var round = ZombiesGameMode.instance != null ? ZombiesGameMode.instance.currentRound.Value : 1;
            maxHealth.Value = baseMaxHealth + round * maxHealthPerRound;
            currentHealth.Value = maxHealth.Value;
        }

        private void Update()
        {
            if (IsServerStarted && canPassiveRegen && currentHealth.Value < maxHealth.Value)
            {
                passiveRegenTimer += Time.deltaTime;
                while (passiveRegenTimer >= 1f / passiveRegenRate)
                {
                    currentHealth.Value += passiveRegenAmount;
                    passiveRegenTimer -= 1f / passiveRegenRate;
                    if (currentHealth.Value > maxHealth.Value) currentHealth.Value = maxHealth.Value;
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
            if (ragdoll != null)
            {
                var instance = Instantiate(ragdoll, transform.position, transform.rotation);
                instance.Spawn(modelRoot, report);
            }
            if (damageFx != null)
            {
                damageFx.transform.SetParent(null);
                Destroy(damageFx.gameObject, 5f);
            }
            gameObject.SetActive(false);
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