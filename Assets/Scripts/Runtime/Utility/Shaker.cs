using System;
using UnityEngine;

namespace Zombies.Runtime.Utility
{
    [Serializable]
    public class Shaker
    {
        public float frequency;
        public float amplitude;
        public float decay;
        public float shakeStrength;

        private float strength;
        private float timer;
        
        public Vector2 offset { get; private set; }
        
        public void Shake(float scale = 1f)
        {
            strength += shakeStrength * scale;
        }

        public void Update() => Update(Time.deltaTime);
        public void Update(float dt)
        {
            timer += dt;
            
            var a = Mathf.PerlinNoise1D(timer * frequency) * Mathf.PI * 2f;
            var l = Mathf.PerlinNoise1D(timer * frequency + 6029f);
            offset = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * l * amplitude * strength;

            strength -= strength * decay * dt;
        }
    }
}