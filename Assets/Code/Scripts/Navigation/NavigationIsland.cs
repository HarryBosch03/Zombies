using System.Collections.Generic;
using UnityEngine;

namespace Zombies.Runtime.Navigation
{
    [System.Serializable]
    public class NavigationIsland
    {
        public List<Vector2Int> openCells = new();
    }
}