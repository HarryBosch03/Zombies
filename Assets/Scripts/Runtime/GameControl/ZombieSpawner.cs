using System;
using System.Collections.Generic;
using FishNet;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Zombies.Runtime.GameControl
{
    public class ZombieSpawner : MonoBehaviour
    {
        public GameObject prefab;
        public float spawnDelay;

        private float spawnTimer;

        public bool isSpawning => spawnTimer > 0f;

        public static List<ZombieSpawner> all = new();

        private void OnEnable() { all.Add(this); }

        private void OnDisable() { all.Remove(this); }

        private void Update() { spawnTimer -= Time.deltaTime; }

        public GameObject Spawn()
        {
            var server = InstanceFinder.ServerManager;
            if (!server.Started) return null;

            spawnTimer = spawnDelay;
            var instance = Instantiate(prefab, transform.position, transform.rotation);
            server.Spawn(instance);

            return instance;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.color = Color.yellow;
            Handles.DrawWireDisc(transform.position, Vector3.up, 0.5f);

            var a = new Vector3(0.3f, 0.0f, 0.6f);
            var b = new Vector3(-0.3f, 0.0f, 0.6f);
            var c = new Vector3(0.0f, 0.0f, 0.9f);
            
            Handles.DrawLine(transform.TransformPoint(a), transform.TransformPoint(b));
            Handles.DrawLine(transform.TransformPoint(b), transform.TransformPoint(c));
            Handles.DrawLine(transform.TransformPoint(c), transform.TransformPoint(a));
        }
#endif
    }
}