using UnityEngine;
using UnityEngine.Serialization;

namespace Framework.Runtime.Player
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Player/PlayerGunPose")]
    public class PlayerGunPose : ScriptableObject
    {
        public Vector3 position;
        public Vector3 eulerAngles;
        
        [Range(10.0f, 120.0f)]
        public float fieldOfView = 50.0f;

        [Tooltip("Idle Default: -0.1\nAim Default: -0.05")]
        public float translationSway = -0.1f;
        [Tooltip("Idle Default: -15.0\nAim Default: 0.0")]
        public float rotationSway = -15.0f;
        
        public float translationLag = -5.0f;
    }
}