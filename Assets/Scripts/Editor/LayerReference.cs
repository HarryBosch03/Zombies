using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Zombies.Editor
{
    [InitializeOnLoad]
    public static class LayerReference
    {
        private const string Guid = "899c2594e9e347978a1bdab6dd8d91fc";

        static LayerReference() { EditorSceneManager.sceneSaved += OnSceneSaved; }

        private static void OnSceneSaved(Scene scene)
        {
            var path = AssetDatabase.GUIDToAssetPath(Guid);

            var sb = new StringBuilder();

            sb.Append
            (
                "namespace Zombies.Runtime\n" +
                "{\n" +
                "   public static class LayerReference\n" +
                "   {\n"
            );

            for (var i = 0; i < 32; i++)
            {
                var layerName = LayerMask.LayerToName(i).Replace(" ", "");
                if (layerName.Length == 0)
                {
                    layerName = $"UndefLayer{i}";
                }
                sb.Append($"        public const int {layerName} = {i};\n");
            }

            sb.Append
            (
                "   }\n" +
                "}"
            );

            var existing = File.ReadAllText(path);
            if (existing == sb.ToString()) return;

            Debug.Log($"Updated {Path.GetFileName(path)}");
            File.WriteAllText(path, sb.ToString());
            AssetDatabase.Refresh();
        }
    }
}