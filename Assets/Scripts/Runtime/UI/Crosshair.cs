using System;
using UnityEngine;
using UnityEngine.UI;
using Zombies.Runtime.Health;
using CharacterController = Zombies.Runtime.Player.CharacterController;

namespace Zombies.Runtime.UI
{
    [RequireComponent(typeof(Image))]
    public class Crosshair : MonoBehaviour
    {
        public Image hitmarker;
        public Color hitmarkerBaseColor = Color.white;
        public float hitmarkerBaseDuration = 0.3f;
        public Color hitmarkerHeadshotColor = Color.yellow;
        public float hitmarkerHeadshotDuration = 0.4f;
        public Color hitmarkerKillColor = Color.red;
        public float hitmarkerKillDuration = 0.6f;
        public AnimationCurve hitmarkerAlphaCurve;
        public AnimationCurve hitmarkerScaleCurve;
        
        private CharacterController character;
        private Image sprite;
        private float hitmarkerTimer;
        private float hitmarkerDuration;
        
        private void Awake()
        {
            character = GetComponentInParent<CharacterController>();
            sprite = GetComponent<Image>();
        }

        private void OnEnable()
        {
            HealthController.OnTakeDamage += TakeDamageEvent;
        }

        private void OnDisable()
        {
            HealthController.OnTakeDamage -= TakeDamageEvent;
        }

        private void TakeDamageEvent(HealthController victim, HealthController.DamageReport report)
        {
            if (transform.IsChildOf(report.damage.invoker.transform))
            {
                if (report.wasLethal) SetHitmarker(hitmarkerKillColor, hitmarkerKillDuration);
                else if (report.wasHeadshot) SetHitmarker(hitmarkerHeadshotColor, hitmarkerHeadshotDuration);
                else SetHitmarker(hitmarkerBaseColor, hitmarkerBaseDuration);
            }
        }

        private void Update()
        {
            sprite.color = Color.white.SetAlpha(character.activeWeapon != null ? 1f - character.activeWeapon.aimPercent : 0f);

            if (hitmarkerTimer > 0f)
            {
                hitmarker.transform.localScale = Vector3.one * hitmarkerScaleCurve.Evaluate(hitmarkerDuration - hitmarkerTimer);
                hitmarker.color = hitmarker.color.SetAlpha(hitmarkerAlphaCurve.Evaluate(hitmarkerTimer));
                hitmarkerTimer -= Time.deltaTime;
            }
            else
            {
                hitmarker.color = Color.clear;
            }
        }

        public void SetHitmarker(Color color, float duration)
        {
            hitmarker.color = color;
            hitmarkerTimer = duration;
            hitmarkerDuration = duration;
        }
    }
}