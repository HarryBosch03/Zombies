using System;
using System.Collections.Generic;
using FishNet;
using UnityEditor;
using UnityEngine;
using Zombies.Runtime.Enemies;
using Zombies.Runtime.Enemies.Common;
using Zombies.Runtime.Health;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

#if UNITY_EDITOR
#endif

namespace Zombies.Runtime.GameControl
{
    public class ZombieSpawner : MonoBehaviour
    {
        public SpawnDefinition[] spawns = new SpawnDefinition[0];
        public bool hasBarrier = true;
        public int maxBarrierHealth;
        public int currentBarrierHealth;

        public GameObject prefab;
        public float spawnDelay;

        private SpawnTracker[] trackers;

        public bool canSpawn { get; private set; }

        public static List<ZombieSpawner> all = new();

        private void Awake() { trackers = new SpawnTracker[spawns.Length]; }

        private void OnEnable() { all.Add(this); }

        private void OnDisable() { all.Remove(this); }

        private void Update()
        {
            canSpawn = false;
            for (var i = 0; i < trackers.Length; i++)
            {
                var tracker = trackers[i];
                
                if (tracker != null)
                {
                    if (tra)
                    var spawnDef = spawns[i];
                    if (tracker.pathIndex < spawnDef.pathToBarrier.Length)
                    {
                        var point = transform.TransformPoint(spawnDef.pathToBarrier[tracker.pathIndex]);
                        tracker.movement.ForceMoveTowards(point);

                        var normal = (tracker.pathIndex != spawnDef.pathToBarrier.Length - 1 ?
                            transform.TransformPoint(spawnDef.pathToBarrier[tracker.pathIndex + 1]) - point :
                            point - transform.TransformPoint(spawnDef.pathToBarrier[tracker.pathIndex - 1])).normalized;

                        if (Vector3.Dot(tracker.gameObject.transform.position - point, normal) > -0.1f)
                        {
                            tracker.pathIndex++;
                        }
                    }
                    else if (hasBarrier && currentBarrierHealth > 0)
                    {
                        tracker.attackinator.ForceAttack(DamageBarrier, null);
                    }
                    else
                    {
                        tracker.gameObject.transform.position = transform.TransformPoint(spawnDef.exitPoint);
                        tracker.control.enabled = true;
                        trackers[i] = null;
                    }
                }
                else canSpawn = true;
            }
        }

        private void DamageBarrier(DamageArgs damage)
        {
            if (currentBarrierHealth == 0) return;
            currentBarrierHealth--;
        }

        public GameObject Spawn()
        {
            var server = InstanceFinder.ServerManager;
            if (!server.Started) return null;

            int i;
            for (i = 0; i < trackers.Length; i++)
            {
                if (trackers[i] == null) break;
            }

            if (i == trackers.Length) return null;

            var spawnDef = spawns[i];
            var instance = Instantiate(prefab, transform.TransformPoint(spawnDef.pathToBarrier[0]), Quaternion.identity);
            server.Spawn(instance);

            trackers[i] = new SpawnTracker(instance);

            return instance;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            currentBarrierHealth = Mathf.Clamp(currentBarrierHealth, 0, maxBarrierHealth);
            maxBarrierHealth = Mathf.Max(0, maxBarrierHealth);
            hasBarrier = maxBarrierHealth > 0;

            if (spawns.Length == 0) spawns = new SpawnDefinition[1];

            for (var i = 0; i < spawns.Length; i++)
            {
                if (spawns[i] == null) spawns[i] = new SpawnDefinition();
                var spawn = spawns[i];
                if (spawn.pathToBarrier.Length < 2) Array.Resize(ref spawn.pathToBarrier, 2);
            }
        }

        private void OnDrawGizmos()
        {
            var str = $"Zombie Spawner [{spawns.Length}]";
            if (Application.isPlaying)
            {
                str += $"\nCanSpawn: {canSpawn}";
                for (var i = 0; i < trackers.Length; i++)
                {
                    str += $"\nTracker {i}: ";

                    var tracker = trackers[i];
                    if (tracker == null) str += "empty";
                    else str += $"{tracker.gameObject.name}";
                }
            }

            Handles.Label(transform.position + Vector3.up, str);

            Gizmos.color = Color.red;
            Gizmos.matrix = transform.localToWorldMatrix;

            var center = Vector3.zero;
            var size = Vector3.one;
            if (spawns.Length > 0)
            {
                var min = new Vector3(float.MaxValue, 0f, float.MaxValue);
                var max = new Vector3(float.MinValue, 0f, float.MinValue);
                for (var i = 0; i < spawns.Length; i++)
                {
                    var spawn = spawns[i];
                    min.x = Mathf.Min(min.x, spawn.exitPoint.x);
                    max.x = Mathf.Max(max.x, spawn.exitPoint.x);

                    min.z = Mathf.Min(min.z, spawn.exitPoint.z);
                    max.z = Mathf.Max(max.z, spawn.exitPoint.z);

                    Gizmos.DrawSphere(spawn.exitPoint, 0.1f);
                }

                min -= new Vector3(1f, 0f, 1f) * 0.5f;
                max += new Vector3(1f, 0f, 1f) * 0.5f;

                center = (max + min) * 0.5f;
                size = max - min;
            }

            Gizmos.DrawWireCube(center, new Vector3(size.x, 0.01f, size.z));
            Gizmos.color = Gizmos.color.SetAlpha(0.2f);
            Gizmos.DrawCube(center, new Vector3(size.x, 0.01f, size.z));
            Gizmos.color = Gizmos.color.SetAlpha(1f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.identity;
            for (var i = 0; i < spawns.Length; i++)
            {
                var spawn = spawns[i];

                for (var j = 0; j < spawn.pathToBarrier.Length - 1; j++)
                {
                    var a = transform.TransformPoint(spawn.pathToBarrier[j]);
                    var b = transform.TransformPoint(spawn.pathToBarrier[j + 1]);
                    Gizmos.DrawLine(a, b);
                    Gizmos.DrawSphere(a, 0.1f);
                }

                Gizmos.DrawSphere(transform.TransformPoint(spawn.pathToBarrier[^1]), 0.1f);
            }
        }
#endif

        [Serializable]
        public class SpawnDefinition
        {
            public Vector3[] pathToBarrier = new Vector3[0];
            public Vector3 exitPoint;
        }

        public class SpawnTracker
        {
            public GameObject gameObject;
            public IEnemyControl control;
            public EnemyMovement movement;
            public EnemyAttackinator attackinator;
            public int pathIndex;

            public SpawnTracker(GameObject gameObject)
            {
                this.gameObject = gameObject;

                control = gameObject.GetComponent<IEnemyControl>();
                movement = gameObject.GetComponent<EnemyMovement>();
                attackinator = gameObject.GetComponent<EnemyAttackinator>();

                control.enabled = false;
            }
        }
    }
}