using UnityEngine;
using UnityEngine.Serialization;

namespace Framework.Runtime.Player
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Config/Player/PlayerCameraAnimatorSettings")]
    public class PlayerCameraAnimatorSettings : ScriptableObject
    {
        public PlayerCameraAnimator.SwayPose idlePose;
        public PlayerCameraAnimator.SwayPose walkPose;
        public PlayerCameraAnimator.SwayPose runPose;
        public float poseSmoothing;
        
        public float fieldOfView = 90.0f;
        [Range(0.0f, 1.0f)]
        public float fovEffects = 1.0f;
        public float runZoom = 0.9f;
    }
}