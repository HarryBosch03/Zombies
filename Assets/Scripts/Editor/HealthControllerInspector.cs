using UnityEditor;
using Zombies.Runtime.Health;

namespace Zombies.Editor
{
    [CustomEditor(typeof(HealthController))]
    public class HealthControllerInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var target = this.target as HealthController;
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.IntField("Current Health", target.currentHealth.Value);
                EditorGUILayout.IntField("Max Health", target.maxHealth.Value);
            }
            EditorGUILayout.Space();
            
            base.OnInspectorGUI();
        }
    }
}