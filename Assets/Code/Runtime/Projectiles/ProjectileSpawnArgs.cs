﻿using Framework.Runtime.Vitality;
using UnityEngine;

namespace Framework.Runtime.Projectiles
{
    [System.Serializable]
    public class ProjectileSpawnArgs
    {
        public DamageArgs damage;
        public float speed = 100.0f;
        public float lifetime = 2.0f;
        public float gravityScale = 1.0f;
        public int pierce = 0;
        [Range(0.0f, 1.0f)]
        public float spread = 0.0f;
        public int count = 1;
    }
}