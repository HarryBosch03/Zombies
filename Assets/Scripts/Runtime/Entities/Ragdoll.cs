using System.Collections.Generic;
using UnityEngine;
using Zombies.Runtime.Health;

namespace Zombies.Runtime.Entities
{
    public class Ragdoll : MonoBehaviour
    {
        public float knockbackForce = 1f;
        public Transform root;
        public Rigidbody[] bodies;
 
        public static List<Ragdoll> all = new();

        private void Awake()
        {
            foreach (var child in GetComponentsInChildren<Transform>())
            {
                child.gameObject.layer = 7;
            }
            
            bodies = GetComponentsInChildren<Rigidbody>();
        }

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

        private void FixedUpdate()
        {
            var poseList = new Pose[bodies.Length];
            for (var i = 0; i < bodies.Length; i++)
            {
                poseList[i].position = bodies[i].transform.position;
                poseList[i].rotation = bodies[i].transform.rotation;
            }

            if (bodies.Length > 0)
            {
                var center = Vector3.zero;
                foreach (var body in bodies)
                {
                    center += body.transform.position / bodies.Length;
                }
                transform.position = center;
                transform.rotation = Quaternion.identity;
            }

            for (var i = 0; i < bodies.Length; i++)
            {
                bodies[i].transform.position = poseList[i].position;
                bodies[i].transform.rotation = poseList[i].rotation;
            }
        }

        public void Spawn(Transform modelRoot, HealthController.DamageReport report)
        {
            var copyFrom = modelRoot.GetComponentsInChildren<Transform>(true);
            var copyTo = root.GetComponentsInChildren<Transform>(true);
            
            for (var i = 0; i < copyFrom.Length; i++)
            {
                if (report.damage.hitCollider.transform == copyFrom[i].transform)
                {
                    var rb = copyTo[i].GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.AddForceAtPosition(-report.damage.normal * report.finalDamage * knockbackForce, report.damage.point, ForceMode.Impulse);
                    }
                }
                
                copyTo[i].position = copyFrom[i].position;
                copyTo[i].rotation = copyFrom[i].rotation;
            }
        }
    }
}