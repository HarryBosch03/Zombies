using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace Zombies.Editor
{
    public static class MenuItems
    {
        [MenuItem("Utility/Run Build")]
        public static void RunBuild()
        {
            Process.Start($"{Application.dataPath}/../Builds/Zombies.exe");
        }
    }
}
