
using System;
using System.Collections.Generic;
using UnityEngine;
using Zombies.Runtime.Vitality;

namespace Zombies.Runtime.Utility
{
    public static class Extension
    {
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            var c = gameObject.GetComponent<T>();
            return c ? c : gameObject.AddComponent<T>();
        }

        public static T Find<T>(this Transform transform, string path)
        {
            var find = transform.Find(path);
            return find ? find.GetComponent<T>() : default;
        }

        public static GameObject FindGameObject(this Transform transform, string path)
        {
            var find = transform.Find(path);
            return find ? find.gameObject : null;
        }

        public static T Best<T>(this IEnumerable<T> list, Func<T, float> scoreCallback, T fallback = default)
        {
            var best = fallback;
            var bestScore = float.MinValue;
            foreach (var other in list)
            {
                var otherScore = scoreCallback(other);
                if (otherScore > bestScore)
                {
                    bestScore = otherScore;
                    best = other;
                }
            }
            return best;
        }

        public static void Damage(this Collider collider, DamageInstance damage)
        {
            var damageable = collider.GetComponentInParent<IDamageable>();
            if (damageable == null) return;
            
            damageable.Damage(damage);
        }
    }
}