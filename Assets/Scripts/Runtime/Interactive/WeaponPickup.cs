using System;
using UnityEngine;
using Zombies.Runtime.Player;

namespace Zombies.Runtime.Interactive
{
    public class WeaponPickup : MonoBehaviour, IInteractive
    {
        public string weaponName;
        public int cost;

        [Space]
        public Transform visuals;
        public Vector3 visualsPosition;
        public Vector3 visualsRotation;
        public float bobFrequency;
        public float bobAmplitude;
        public float spinSpeed;

        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (visuals != null)
            {
                visuals.localPosition = visualsPosition + Vector3.up * Mathf.Sin(Time.time * bobFrequency * Mathf.PI) * bobAmplitude;
                visuals.localEulerAngles = visualsRotation;
            }

            transform.rotation = Quaternion.Euler(Vector3.up * Time.time * spinSpeed);
        }

        public int GetCost(PlayerController player) => player.character.GetWeaponFromInventory(weaponName) == null ? cost : cost / 2;

        public string GetDisplayText(PlayerController player) => player.character.GetWeaponFromInventory(weaponName) == null ? $"Purchase {weaponName}" : $"Purchase {weaponName} Ammo";

        public bool Interact(PlayerController player)
        {
            var weapon = player.character.GetWeaponFromInventory(weaponName);
            if (weapon == null)
            {
                player.character.PickupWeapon(weaponName);
                return true;
            }
            else if (weapon.reserveCurrent < weapon.reserveMax)
            {
                weapon.reserveCurrent = weapon.reserveMax;
                return true;
            }
            else return false;
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                if (visuals != null)
                {
                    visuals.localPosition = visualsPosition;
                    visuals.localEulerAngles = visualsRotation;
                }
            }
        }
    }
}