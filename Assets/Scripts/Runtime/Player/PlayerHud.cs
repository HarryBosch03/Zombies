using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Zombies.Runtime.Player
{
    public class PlayerHud : MonoBehaviour
    {
        public Canvas canvas;
        public TMP_Text ammoValue;
        public TMP_Text ammoLabel;
        public Image reloadProgress;
        public TMP_Text healthValue;

        [Space]
        public RectTransform face;

        private float faceClock;
        private PlayerController player;

        private void Awake() { player = GetComponentInParent<PlayerController>(); }

        private void OnEnable() { player.ActiveViewerChanged += OnActiveViewerChanged; }

        private void OnDisable() { player.ActiveViewerChanged -= OnActiveViewerChanged; }

        private void OnActiveViewerChanged(PlayerController player, bool isActiveViewer) { canvas.gameObject.SetActive(isActiveViewer); }

        private void Update()
        {
            var weapon = player.activeWeapon;

            var isReloading = false;
            var currentAmmo = 0;
            var maxAmmo = 0;
            var reloadPercent = 0f;

            if (weapon != null)
            {
                isReloading = weapon.isReloading;
                currentAmmo = weapon.currentMagazine;
                maxAmmo = weapon.magazineSize;
                reloadPercent = weapon.reloadPercent;
            }

            if (ammoLabel) ammoLabel.text = isReloading ? "Reloading" : "Ammo";
            if (ammoValue)
            {
                ammoValue.gameObject.SetActive(!isReloading);
                ammoValue.text = $"{currentAmmo} / {maxAmmo}";
            }

            if (reloadProgress)
            {
                reloadProgress.gameObject.SetActive(isReloading);
                reloadProgress.fillAmount = reloadPercent;
            }

            if (healthValue) healthValue.text = $"{player.health.currentHealth}/{player.health.maxHealth}";

            if (face)
            {
                face.anchoredPosition = new Vector2(-Mathf.FloorToInt(faceClock), 0f) * 125f;
            }

            faceClock += Time.deltaTime;
            faceClock %= 3f;
        }
    }
}