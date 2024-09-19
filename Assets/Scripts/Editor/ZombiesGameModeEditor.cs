using UnityEditor;
using UnityEngine;
using Zombies.Runtime.GameControl;

namespace Zombies.Editor
{
    [CustomEditor(typeof(ZombiesGameMode))]
    public class ZombiesGameModeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var target = this.target as ZombiesGameMode;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                for (var i = 1; i <= 16; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PrefixLabel($"Round {i}");
                        var rect = EditorGUILayout.GetControlRect(false, 18f);
                        rect.width /= 4f;
                        
                        var zombieCount = Mathf.RoundToInt(target.enemiesPerRoundConstant + target.enemiesPerRoundLinear * i + target.enemiesPerRoundQuadratic * i * i);
                        var speedModifier = Mathf.RoundToInt(target.enemySpeedMax * (target.enemySpeedMax / (-target.enemySpeedGradient * i - target.enemySpeedMax) + 1f) * 100f);

                        EditorGUI.LabelField(rect, "Zombies");
                        rect.x += rect.width;
                        EditorGUI.IntField(rect, zombieCount);
                        rect.x += rect.width;
                        EditorGUI.LabelField(rect, "Speed Mod");
                        rect.x += rect.width;
                        EditorGUI.IntField(rect, speedModifier);
                    }
                }
            }
            
        }
    }
}