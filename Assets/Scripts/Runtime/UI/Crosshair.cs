using System;
using UnityEngine;
using UnityEngine.UI;
using Zombies.Runtime.Health;
using Zombies.Runtime.Player;
using CharacterController = Zombies.Runtime.Player.CharacterController;

namespace Zombies.Runtime.UI
{
    public class Crosshair : MonoBehaviour
    {
        public Image[] components;

        [Space]
        public Image hitmarker;
        public Color hitmarkerBaseColor = Color.white;
        public float hitmarkerBaseDuration = 0.3f;
        public Color hitmarkerHeadshotColor = Color.yellow;
        public float hitmarkerHeadshotDuration = 0.4f;
        public Color hitmarkerKillColor = Color.red;
        public float hitmarkerKillDuration = 0.6f;
        public AnimationCurve hitmarkerAlphaCurve;
        public AnimationCurve hitmarkerScaleCurve;

        [Space]
        public float expansionDistance;
        public float expansionDuration;
        public AnimationCurve expansionCurve;

        private CharacterController character;
        private float hitmarkerTimer;
        private float hitmarkerDuration;
        private float expansionPercent;
        private float reloadPercent;

        private void Awake() { character = GetComponentInParent<CharacterController>(); }

        private void OnEnable()
        {
            HealthController.OnTakeDamage += TakeDamageEvent;
            PlayerWeapon.ShootEvent += OnShoot;
        }

        private void OnDisable()
        {
            HealthController.OnTakeDamage -= TakeDamageEvent;
            PlayerWeapon.ShootEvent -= OnShoot;
        }

        private void OnShoot(PlayerWeapon weapon)
        {
            if (weapon.character != character) return;

            expansionPercent = 1f;
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
            for (var i = 0; i < components.Length; i++)
            {
                var component = components[i];
                if (character.activeWeapon != null)
                {
                    component.color = Color.white.SetAlpha(1f - character.activeWeapon.aimPercent);
                    component.rectTransform.localPosition = component.rectTransform.right * (expansionCurve.Evaluate(expansionPercent) + 2f * expansionCurve.Evaluate(character.activeWeapon.aimPercent)) * expansionDistance;

                    var percent = smootherstep(character.activeWeapon.reloadPercent);
                    var spacing = Mathf.Lerp(-90f, -120f, curve(percent));
                    var offset = -720f * percent * Mathf.Ceil(character.activeWeapon.reloadTime);
                    component.rectTransform.localRotation = Quaternion.Euler(0f, 0f, i * spacing + offset);
                }
                else
                {
                    component.rectTransform.localPosition = Vector3.zero;
                    component.rectTransform.localRotation = Quaternion.Euler(0f, 0f, i * -90f);
                }
            }

            expansionPercent -= Time.deltaTime / expansionDuration;

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

            float curve(float x) => x switch
            {
                < 0f => 0f,
                < 0.25f => sqr(4f * x) / 2f,
                < 0.75f => 1f - 2f * sqr(1f - 2f * x),
                < 1f => 2f * sqr(2f - 2f * x),
                _ => 0f
            };

            float smootherstep(float x) => x * x * x * (x * (6.0f * x - 15.0f) + 10.0f);
            
            float sqr(float x) => x * x;
        }

        public void SetHitmarker(Color color, float duration)
        {
            hitmarker.color = color;
            hitmarkerTimer = duration;
            hitmarkerDuration = duration;
        }
    }
}