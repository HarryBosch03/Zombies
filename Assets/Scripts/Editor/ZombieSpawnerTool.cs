using System;
using System.Diagnostics;
using System.Timers;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;
using Zombies.Runtime;
using Zombies.Runtime.GameControl;

namespace Zombies.Editor
{
    [EditorTool("Zombie Spawner Configurator", typeof(ZombieSpawner))]
    public class ZombieSpawnerTool : EditorTool
    {
        private Stopwatch stopwatch;

        private void OnEnable()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        private void OnDisable()
        {
            stopwatch.Stop();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (window is not SceneView sceneView) return;
            
            Handles.BeginGUI();
            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label("Hello, World!");
                }

                GUILayout.FlexibleSpace();
            }

            Handles.EndGUI();

            var target = this.target as ZombieSpawner;
            if (target == null) return;

            var handleOrientation = Quaternion.Euler(0f, target.transform.eulerAngles.y, 0f);
            for (var i = 0; i < target.spawns.Length; i++)
            {
                var spawn = target.spawns[i];
                
                var position = target.transform.TransformPoint(spawn.exitPoint);
                EditorGUI.BeginChangeCheck();
                position = Handles.PositionHandle(position, handleOrientation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Modified Zombie Spawner Spawn Definition Path");
                    position = target.transform.InverseTransformPoint(position);
                    position = (Vector3)Vector3Int.RoundToInt(position * 100f) / 100f;
                    spawn.exitPoint = position;
                }
                
                for (var j = 0; j < spawn.pathToBarrier.Length; j++)
                {
                    position = target.transform.TransformPoint(spawn.pathToBarrier[j]);
                    EditorGUI.BeginChangeCheck();
                    position = Handles.PositionHandle(position, handleOrientation);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Modified Zombie Spawner Spawn Definition Path");
                        position = target.transform.InverseTransformPoint(position);
                        position = (Vector3)Vector3Int.RoundToInt(position * 100f) / 100f;
                        spawn.pathToBarrier[j] = position;
                    }
                }
                
                for (var j = 0; j < spawn.pathToBarrier.Length - 1; j++)
                {
                    for (var offset = 0f; offset < 1f; offset += 0.2f)
                    {
                        var a = target.transform.TransformPoint(spawn.pathToBarrier[j]);
                        var b = target.transform.TransformPoint(spawn.pathToBarrier[j + 1]);
                        
                        var t = ((float)stopwatch.Elapsed.TotalSeconds + offset) % 1f;
                        Handles.color = Color.red;
                        Handles.DrawSolidDisc(Vector3.Lerp(a, b, t), Vector3.up, 0.05f);
                    }
                }

                Handles.DrawAAPolyLine(target.transform.TransformPoint(spawn.pathToBarrier[^1]), target.transform.TransformPoint(spawn.exitPoint));
            }
            
            window.Repaint();
        }
    }
}