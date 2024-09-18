using UnityEditor;
using UnityEngine;
using Zombies.Runtime.GameControl;

namespace Zombies.Editor
{
    [CustomEditor(typeof(ZombieSpawner))]
    public class ZombieSpawnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Application.isPlaying)
            {
                var target = this.target as ZombieSpawner;
                EditorGUILayout.Space();
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.Toggle("Can Spawn", target.canSpawn);
                }
            }
        }
    }
}