using UnityEngine;
using UnityEngine.Rendering;
using Zombies.Runtime.Player;

namespace Zombies.Runtime.Health
{
    [RequireComponent(typeof(HealthController))]
    [RequireComponent(typeof(PlayerController))]
    public class PlayerHealthEffects : MonoBehaviour
    {
        public Volume hurtVolume;
        public AnimationCurve weightCurve;
        public float throbFrequency;
        [Range(0f, 1f)]
        public float throbAmplitude;
        public AnimationCurve throbCurve = new AnimationCurve(new[]
        {
            new Keyframe(0f, 1f),
            new Keyframe(0.5f, -1f),
            new Keyframe(1f, 1f),
        })
        {
            postWrapMode = WrapMode.Loop,
            preWrapMode = WrapMode.Loop,
        };

        private PlayerController player;
        private HealthController health;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            health = GetComponent<HealthController>();
        }

        private void Update()
        {
            if (hurtVolume != null)
            {
                hurtVolume.enabled = player.character.isActiveViewer;
                var weight = weightCurve.Evaluate((float)health.currentHealth / health.maxHealth) * (1f - throbAmplitude);
                weight *= 1f + throbCurve.Evaluate(Time.time * throbFrequency % 1f) * throbAmplitude;
                hurtVolume.weight = weight;
            }
        }

        private void OnValidate()
        {
            if (hurtVolume != null) hurtVolume.weight = 0f;
        }
    }
}