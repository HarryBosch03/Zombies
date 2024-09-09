using System;
using UnityEngine;

namespace Zombies.Runtime.Health
{
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

        public DamageArgs(int damage) : this()
        {
            this.damage = damage;
        }
            
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
}