﻿using UnityEngine;

namespace Framework.Runtime.Vitality
{
    [SelectionBase, DisallowMultipleComponent]
    public class HealthController : MonoBehaviour, IDamageable
    {
        public int currentHealth = 100;
        public int maxHealth = 100;

        public float LastDamageTime { get; private set; }

        protected virtual void OnEnable()
        {
            currentHealth = maxHealth;
        }

        public virtual void Damage(DamageInstance instance)
        {
            Validate();
            
            var damage = Mathf.Max(1, Mathf.FloorToInt(instance.Calculate()));
            currentHealth -= damage;
            
            LastDamageTime = Time.time;
            
            if (currentHealth <= 0)
            {
                Die(instance);
            }
        }

        private void Validate() { currentHealth = Mathf.Min(currentHealth, maxHealth); }

        public virtual void Die(DamageInstance instance)
        {
            gameObject.SetActive(false);
        }
    }
}