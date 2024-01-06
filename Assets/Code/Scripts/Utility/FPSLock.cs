using UnityEngine;

namespace Zombies.Runtime.Utility
{
    public class FPSLock : MonoBehaviour
    {
        public int fps = 60;

        private void OnEnable()
        {
            Debug.Log(Application.targetFrameRate);
            Application.targetFrameRate = fps;
        }

        private void OnDisable()
        {
            Application.targetFrameRate = -1;
        }
    }
}