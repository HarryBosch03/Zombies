using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.Navigation
{
    [System.Serializable]
    public class NavigationIsland
    {
        public List<Vector2Int> openCells = new();
    }
}