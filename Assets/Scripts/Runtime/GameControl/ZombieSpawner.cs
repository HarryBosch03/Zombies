using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Zombies.Runtime.Enemies;
using Zombies.Runtime.Enemies.Common;
using Zombies.Runtime.Health;
using Zombies.Runtime.Interactive;
using Zombies.Runtime.Player;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

#if UNITY_EDITOR
#endif

namespace Zombies.Runtime.GameControl
{
    public class ZombieSpawner : NetworkBehaviour, IInteractive
    {
        public SpawnDefinition[] spawns = new SpawnDefinition[0];
        public GameObject[] boards;

        public string displayDefaultName = "Repair Barricade";
        public string displayBusyName = "Repairing [{0}s]";
        public string displayFullHealthName = "Barricade at full health";
        public float repairTime = 1.5f;

        public bool hasBarrier = true;
        public int maxBarrierHealth;
        public int currentBarrierHealth;
        public float repairRange = 2f;
        public int pointsForRepair = 50;

        public GameObject prefab;
        public float spawnDelay;
        public Transform repairInvoker;
        public bool repairing;
        public float repairStartTime;

        private NavMeshPath navMeshQuery;
        private SpawnTracker[] trackers;
        private event Action callNextFrame;

        public bool canSpawn { get; private set; }

        public static List<ZombieSpawner> all = new();

        public int GetCost(PlayerController player) => 0;

        public string GetDisplayText(PlayerController player)
        {
            if (!hasBarrier) return null;
            if (currentBarrierHealth >= maxBarrierHealth) return displayFullHealthName;
            if (repairing) return string.Format(displayBusyName, repairStartTime.ToString("0.0"));
            if ((player.transform.position - transform.position).sqrMagnitude > repairRange * repairRange) return "Too Far Away";
            return displayDefaultName;
        }

        public bool Interact(PlayerController player)
        {
            if (!hasBarrier || repairing) return true;
            if ((player.transform.position - transform.position).sqrMagnitude > repairRange * repairRange || currentBarrierHealth >= maxBarrierHealth) return false;

            repairing = true;
            repairStartTime = repairTime;
            repairInvoker = player.transform;
            return true;
        }

        private void Awake()
        {
            trackers = new SpawnTracker[spawns.Length];
            navMeshQuery = new NavMeshPath();
        }

        public override void OnStartNetwork()
        {
            all.Add(this);
            EnemyAttackinator.OnAttackLand += OnAttackLand;

            UpdateBoards();
        }

        public override void OnStopNetwork()
        {
            all.Remove(this);
            EnemyAttackinator.OnAttackLand -= OnAttackLand;
        }


        private void OnAttackLand(EnemyAttackinator invoker, DamageArgs damage)
        {
            if (!Array.Exists(trackers, e => e != null && e.attackinator == invoker)) return;
            DamageBarrier();
        }

        private void FixedUpdate()
        {
            callNextFrame?.Invoke();
            callNextFrame = null;

            if (repairing)
            {
                if ((transform.position - repairInvoker.position).sqrMagnitude > repairRange * repairRange)
                {
                    repairing = false;
                }
                else
                {
                    repairStartTime -= Time.deltaTime;
                    if (repairStartTime <= 0)
                    {
                        currentBarrierHealth++;
                        if (IsServerStarted)
                        {
                            UpdateHealthRpc(currentBarrierHealth, maxBarrierHealth);
                            if (repairInvoker.TryGetComponent(out PlayerPoints points))
                            {
                                points.AwardPoints("Repair Barricade", pointsForRepair);
                            }
                        }
                        UpdateBoards();
                        if (currentBarrierHealth >= maxBarrierHealth)
                            repairing = false;
                        else
                            repairStartTime = repairTime;
                    }
                }
            }

            canSpawn = false;
            for (var i = 0; i < trackers.Length; i++)
            {
                var tracker = trackers[i];

                if (tracker != null)
                {
                    if (tracker.gameObject == null)
                    {
                        trackers[i] = null;
                        canSpawn = true;
                        continue;
                    }

                    var spawnDef = spawns[i];
                    if (tracker.pathIndex < spawnDef.pathToBarrier.Length)
                    {
                        var point = transform.TransformPoint(spawnDef.pathToBarrier[tracker.pathIndex]);
                        tracker.movement.ForceMoveTowards(point);

                        var normal = (tracker.pathIndex == 0 ?
                            transform.TransformPoint(spawnDef.pathToBarrier[tracker.pathIndex + 1]) - point :
                            point - transform.TransformPoint(spawnDef.pathToBarrier[tracker.pathIndex - 1])).normalized;

                        if (Vector3.Dot(tracker.gameObject.transform.position - point, normal) > -0.1f)
                        {
                            tracker.pathIndex++;
                        }
                    }
                    else
                    {
                        tracker.movement.ClearMovement();
                        if (hasBarrier && currentBarrierHealth > 0)
                        {
                            tracker.attackinator.Attack();
                            tracker.movement.ForceLookAt(tracker.movement.transform.position + transform.forward);
                        }
                        else if (!tracker.attackinator.isAttacking)
                        {
                            tracker.animator.SetTrigger(spawnDef.exitAnimationTriggerName);
                            callNextFrame += () => tracker.gameObject.transform.position = transform.TransformPoint(spawnDef.pathToBarrier[^1]);
                            tracker.gameObject.transform.rotation = transform.rotation;
                            tracker.control.enabled = true;
                            trackers[i] = null;
                        }
                    }
                }
                else canSpawn = true;
            }

            if (canSpawn)
            {
                var hasPath = false;
                foreach (var player in PlayerController.All)
                {
                    if (NavMesh.SamplePosition(player.transform.position, out var naxMeshHit, 1.5f, ~0) && NavMesh.CalculatePath(transform.position + transform.forward, naxMeshHit.position, ~0, navMeshQuery))
                    {
                        if (navMeshQuery.status == NavMeshPathStatus.PathComplete) hasPath = true;
                    }
                }

                if (!hasPath) canSpawn = false;
            }
        }

        private void DamageBarrier()
        {
            if (!IsServerStarted) return;

            if (currentBarrierHealth > 0)
            {
                currentBarrierHealth--;
            }

            UpdateHealthRpc(currentBarrierHealth, maxBarrierHealth);
            UpdateBoards();
        }

        [ObserversRpc(ExcludeServer = true)]
        private void UpdateHealthRpc(int currentBarrierHealth, int maxBarrierHealth)
        {
            this.currentBarrierHealth = currentBarrierHealth;
            this.maxBarrierHealth = maxBarrierHealth;
            UpdateBoards();
        }

        private void UpdateBoards()
        {
            for (var i = 0; i < boards.Length; i++)
            {
                var board = boards[i];
                if (board != null) board.SetActive(i < currentBarrierHealth);
            }
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
        protected override void OnValidate()
        {
            if (hasBarrier)
            {
                currentBarrierHealth = Mathf.Clamp(currentBarrierHealth, 0, maxBarrierHealth);
                maxBarrierHealth = Mathf.Max(0, maxBarrierHealth);
            }
            else
            {
                currentBarrierHealth = 0;
                maxBarrierHealth = 0;
            }

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
                    if (tracker == null || tracker.gameObject == null) str += "empty";
                    else str += $"{tracker.gameObject.name}";
                }
            }

            Handles.Label(transform.position + Vector3.up, str);

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
            public string exitAnimationTriggerName;
        }

        public class SpawnTracker
        {
            public GameObject gameObject;
            public IEnemyControl control;
            public EnemyMovement movement;
            public EnemyAttackinator attackinator;
            public Animator animator;
            public int pathIndex;
            public string animationPath;

            public SpawnTracker(GameObject gameObject)
            {
                this.gameObject = gameObject;

                control = gameObject.GetComponent<IEnemyControl>();
                movement = gameObject.GetComponent<EnemyMovement>();
                attackinator = gameObject.GetComponent<EnemyAttackinator>();
                animator = gameObject.GetComponent<Animator>();

                control.enabled = false;
            }
        }
    }
}