using UnityEngine;

namespace Zombies.Runtime.Cameras
{
    [RequireComponent(typeof(Camera))]
    public class ViewportCamera : MonoBehaviour
    {
        private new Camera camera;

        public static float viewportFieldOfView { get; set; } = 50f;
        public static float referenceFieldOfView { get; set; }
        public static float centerPlane { get; set; }
        
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
        }
    }
}