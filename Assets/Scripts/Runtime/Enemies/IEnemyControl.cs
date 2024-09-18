using System;
using UnityEngine;
using Zombies.Runtime.Health;

namespace Zombies.Runtime.Enemies
{
    public interface IEnemyControl
    {
        public bool enabled { get; set; }
    }
}