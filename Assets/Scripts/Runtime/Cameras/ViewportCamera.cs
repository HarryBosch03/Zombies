using System.Collections.Generic;
using UnityEngine;

namespace Zombies.Runtime.Cameras
{
    [RequireComponent(typeof(Camera))]
    public class ViewportCamera : MonoBehaviour
    {
        public float spriteOffsetScale;
        public RectTransform sprite;
        
        private new Camera camera;

        public static float viewportFieldOfView { get; set; } = 50f;
        public static float referenceFieldOfView { get; set; }
        public static float centerPlane { get; set; }
        public static Dictionary<int, Vector2> spriteOffset { get; } = new();
        public static bool disableOffset { get; set; }

        public static void SetSpriteOffset(int priority, Vector2 offset)
        {
            spriteOffset[priority] = offset;
        }
        
        public static void ClearSpriteOffset(int priority)
        {
            if (spriteOffset.ContainsKey(priority)) spriteOffset.Remove(priority);
        }
        
        private void Awake()
        {
            camera = GetComponent<Camera>();
        }

        private void Update()
        {
            camera.fieldOfView = viewportFieldOfView;
            var h0 = centerPlane / Mathf.Cos(referenceFieldOfView * Mathf.Deg2Rad * 0.5f);
            var t = Mathf.Tan(referenceFieldOfView * Mathf.Deg2Rad * 0.5f) * h0;

            var h1 = t / Mathf.Tan(viewportFieldOfView * Mathf.Deg2Rad * 0.5f);
            var c = Mathf.Cos(viewportFieldOfView * Mathf.Deg2Rad * 0.5f) * h1;

            transform.localPosition = new Vector3(0f, 0f, centerPlane - c);

            var offset = Vector2.zero;
            var maxPriority = int.MinValue;
            foreach (var pair in spriteOffset)
            {
                if (pair.Key > maxPriority)
                {
                    maxPriority = pair.Key;
                    offset = pair.Value;
                }
            }
            
            sprite.anchoredPosition = disableOffset ? Vector2.zero : offset * spriteOffsetScale;
        }
    }
}