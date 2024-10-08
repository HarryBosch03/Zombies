using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zombies.Runtime.GameControl;
using Zombies.Runtime.Health;
using Zombies.Runtime.Utility;

namespace Zombies.Runtime.Player
{
    public class PlayerHud : MonoBehaviour
    {
        public CanvasGroup dot;
        public TMP_Text ammoValue;
        public TMP_Text ammoLabel;
        public Image reloadProgress;
        public TMP_Text healthValue;
        public TMP_Text roundCounter;
        public Image colorOverlay;
        public float damageFxDuration = 0.2f;
        public Color damageOverlayColor = new Color(1f, 0f, 0f, 0.3f);
        public Shaker damageShaker = new();

        [Space]
        public RectTransform face;

        [Space]
        public RectTransform shakeParent;

        private float faceClock;
        private CharacterController character;
        
        private Color colorOverlayColor;
        private float colorOverlayDuration;
        private float colorOverlayTimer;

        private void Awake()
        {
            character = GetComponentInParent<CharacterController>();
            colorOverlayColor = Color.clear;
            colorOverlayDuration = 1f;
            colorOverlayTimer = 0f;
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
            if (!transform.IsChildOf(victim.transform)) return;
            
            SetColorOverlay(damageOverlayColor, damageFxDuration);
            damageShaker.Shake();
        }

        private void Update()
        {
            UpdateWeapon();
            UpdateHealth();
            UpdateFace();
            UpdateOverlay();
            UpdateShake();
            UpdateCrosshair();
            UpdateRoundCounter();
        }

        private void UpdateRoundCounter()
        {
            if (roundCounter != null)
            {
                roundCounter.text = (ZombiesGameMode.instance != null ? ZombiesGameMode.instance.currentRound.Value : 1).ToString();
            }
        }

        private void UpdateCrosshair()
        {
            var weapon = character.activeWeapon;
            if (dot != null) dot.alpha = weapon != null ? 1f - weapon.aimPercent : 1f;
        }

        private void UpdateShake()
        {
            damageShaker.Update();
            shakeParent.anchoredPosition = damageShaker.offset;
        }

        private void UpdateOverlay()
        {
            colorOverlay.color = colorOverlayColor.MulAlpha(sqr(Mathf.Clamp01(colorOverlayTimer / colorOverlayDuration)));
            colorOverlayTimer -= Time.deltaTime;
            
            float sqr(float x) => x * x;
        }

        private void UpdateWeapon()
        {
            var weapon = character.activeWeapon;

            var isReloading = false;
            var currentAmmo = 0;
            var currentReserve = 0;
            var reloadPercent = 0f;

            if (weapon != null)
            {
                isReloading = weapon.isReloading;
                currentAmmo = weapon.currentMagazine;
                currentReserve = weapon.reserveCurrent;
                reloadPercent = weapon.reloadPercent;
            }

            if (ammoLabel) ammoLabel.text = isReloading ? "Reloading" : "Ammo";
            if (ammoValue)
            {
                ammoValue.gameObject.SetActive(!isReloading);
                ammoValue.text = $"{currentAmmo} / {currentReserve}";
            }

            if (reloadProgress)
            {
                reloadProgress.gameObject.SetActive(isReloading);
                reloadProgress.fillAmount = reloadPercent;
            }
        }

        private void UpdateHealth()
        {
            if (healthValue) healthValue.text = $"{character.health.currentHealth.Value}/{character.health.maxHealth.Value}";
        }

        private void UpdateFace()
        {
            if (face)
            {
                var row = Mathf.FloorToInt(3f * (1f - (float)character.health.currentHealth.Value / character.health.maxHealth.Value));
                face.anchoredPosition = new Vector2(-Mathf.FloorToInt(faceClock), row) * 125f;
            }

            faceClock += Time.deltaTime;
            faceClock %= 3f;
        }

        public void SetColorOverlay(Color color, float duration)
        {
            colorOverlayColor = color;
            colorOverlayDuration = duration;
            colorOverlayTimer = duration;
        }

        private void OnValidate()
        {
            if (colorOverlay != null)
            {
                colorOverlay.color = Color.clear;
            }
        }
    }
}