using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using UnityEngine;
using Zombies.Runtime.Enemies.Common;
using Zombies.Runtime.Health;
using Zombies.Runtime.Player;

namespace Zombies.Runtime.GameControl
{
    public class ZombiesGameMode : NetworkBehaviour
    {
        public bool verboseLogging;

        [Space]
        public int currentRound;
        public int zombiesLeft;
        public float speedModifier;

        [Space]
        public float enemiesPerRoundConstant;
        public float enemiesPerRoundLinear;
        public float enemiesPerRoundQuadratic;

        [Space]
        public float enemySpeedLinear;
        public float enemySpeedInvQuadratic;

        [Space]
        public int maxActiveSpawners = 4;
        public float tickRate = 1f;

        private float clock;
        private List<GameObject> trackedZombies = new();
        private ZombieSpawner[] activeSpawners = new ZombieSpawner[0];

        public override void OnStartNetwork() => enabled = IsServerInitialized;

        private void OnEnable() { HealthController.OnDie += OnDie; }

        private void OnDisable() { HealthController.OnDie -= OnDie; }

        private void OnDie(HealthController controller, HealthController.DamageReport report)
        {
            if (trackedZombies.Remove(controller.gameObject))
            {
                VerboseLog($"Successfully removed {gameObject.name} from tracked pool");
                Despawn(controller.gameObject);
            }
        }

        private void Update()
        {
            if (clock > 1f / tickRate)
            {
                if (activeSpawners.Length != maxActiveSpawners) Array.Resize(ref activeSpawners, maxActiveSpawners);

                if (zombiesLeft > 0)
                {
                    for (var i = 0; i < activeSpawners.Length; i++)
                    {
                        var spawner = activeSpawners[i];
                        if (spawner == null || spawner.canSpawn)
                        {
                            activeSpawners[i] = ProcNewSpawner();
                            break;
                        }
                    }
                }
                else if (trackedZombies.Count == 0)
                {
                    currentRound++;
                    zombiesLeft = Mathf.RoundToInt(enemiesPerRoundConstant + enemiesPerRoundLinear * currentRound + enemiesPerRoundQuadratic * currentRound * currentRound);
                    EnemyMovement.globalSpeedModifier = enemySpeedLinear * currentRound + enemySpeedInvQuadratic * Mathf.Sqrt(currentRound);
                    speedModifier = Mathf.RoundToInt(EnemyMovement.globalSpeedModifier * 100f);
                }

                clock -= 1f / tickRate;
            }

            clock += Time.deltaTime;
        }

        private ZombieSpawner ProcNewSpawner()
        {
            var player = PlayerController.GetRandomPlayer();
            if (player == null) return null;

            var bestSpawner = (ZombieSpawner)null;
            var bestSpawnerScore = float.MinValue;

            for (var i = 0; i < ZombieSpawner.all.Count; i++)
            {
                var spawner = ZombieSpawner.all[i];
                var score = 1f / (spawner.transform.position - player.transform.position).sqrMagnitude;
                if (spawner.canSpawn && score > bestSpawnerScore)
                {
                    bestSpawner = spawner;
                    bestSpawnerScore = score;
                }
            }

            if (bestSpawner != null)
            {
                var zombie = bestSpawner.Spawn();
                trackedZombies.Add(zombie);
                if (zombie != null) VerboseLog($"Spawner {bestSpawner.name} Successfully Triggered");
                else VerboseLog($"Spawner {bestSpawner.name} Failed to Trigger");
                zombiesLeft--;
            }
            else
            {
                VerboseLog("Tried to Trigger spawner but none were found");
            }

            return bestSpawner;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            maxActiveSpawners = Mathf.Max(1, maxActiveSpawners);
        }

        public void VerboseLog(object message)
        {
            if (verboseLogging) Debug.Log(message);
        }
    }
}