using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace Zombies.Editor
{
    [InitializeOnLoad]
    public static class HideUIButton
    {
        static HideUIButton()
        {
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
            if (hideUI) UpdateUIVisibility();
        }

        public static bool hideUI { get => EditorPrefs.GetBool($"{typeof(HideUIButton).FullName}.hideUI", false); set => EditorPrefs.SetBool($"{typeof(HideUIButton).FullName}.hideUI", value); }

        private static void OnToolbarGUI()
        {
            if (GUILayout.Button($"{(hideUI ? "Show" : "Hide")} UI", GUILayout.ExpandWidth(false)))
            {
                hideUI = !hideUI;
                UpdateUIVisibility();
            }
        }

        private static void UpdateUIVisibility()
        {
            var visible = !hideUI;
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in canvases)
            {
                if (visible) SceneVisibilityManager.instance.Show(canvas.gameObject, true);
                else SceneVisibilityManager.instance.Hide(canvas.gameObject, true);
            }
        }
    }
}