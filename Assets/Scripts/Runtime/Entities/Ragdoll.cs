using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Zombies.Runtime.Entities
{
    public class Ragdoll : MonoBehaviour
    {
        public float knockbackForce = 1f;
        public float forceVariance = 1.2f;
        public Transform root;

        public static List<Ragdoll> all = new();
        
        private void Start()
        {
            all.Add(this);
            while (all.Count > 50)
            {
                var oldest = all[0];
                all.RemoveAt(0);
                Destroy(oldest.gameObject);
            }
        }

        public void Spawn(Transform modelRoot, HealthController.DamageArgs args)
        {
            var bodies = GetComponentsInChildren<Rigidbody>();
            var bestBody = (Rigidbody)null;
            var bestScore = float.MinValue;

            foreach (var body in bodies)
            {
                var score = 1f / (body.transform.position - args.point).magnitude;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestBody = body;
                }
            }

            if (bestBody != null)
            {
                bestBody.AddForceAtPosition(-args.normal * args.damage * Mathf.Pow(forceVariance, Random.Range(-1f, 1f)) * knockbackForce, args.point, ForceMode.Impulse);
            }

            var copyFrom = modelRoot.GetComponentsInChildren<Transform>();
            var copyTo = root.GetComponentsInChildren<Transform>();
            for (var i = 0; i < copyFrom.Length; i++)
            {
                copyTo[i].position = copyFrom[i].position;
                copyTo[i].rotation = copyFrom[i].rotation;
            }
        }
    }
}