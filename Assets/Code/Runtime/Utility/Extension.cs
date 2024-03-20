
using System;
using System.Collections.Generic;
using Framework.Runtime.Vitality;
using UnityEngine;

namespace Framework.Runtime.Utility
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

        public static Transform Get(this Transform transform, string path)
        {
            var head = transform;
            foreach (var name in path.Split('/'))
            {
                var child = head.Find(name);
                if (!child)
                {
                    child = new GameObject(name).transform;
                    child.SetParent(head);
                    child.ResetPose();
                }
                head = child;
            }
            return head;
        }
        
        public static GameObject FindGameObject(this Transform transform, string path)
        {
            var find = transform.Find(path);
            return find ? find.gameObject : null;
        }

        public static Transform DeepFind(this Transform transform, string name)
        {
            if (transform.name == name) return transform;
            foreach (Transform child in transform)
            {
                var r = child.DeepFind(name);
                if (r) return r;
            }
            return null;
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

        public static Quaternion Euler(this Vector3 v) => Quaternion.Euler(v);

        public static void ResetPose(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        public static T SafeIndex<T>(this T[] array, int index, T fallback = default)
        {
            return index >= 0 && index < array.Length ? array[index] : fallback;
        }
        
        public static T SafeIndex<T>(this IReadOnlyList<T> list, int index, T fallback = default)
        {
            return index >= 0 && index < list.Count ? list[index] : fallback;
        }
    }
}