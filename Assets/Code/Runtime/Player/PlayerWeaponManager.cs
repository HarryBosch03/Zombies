using System;
using System.Collections.Generic;
using Framework.Runtime.Player.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Framework.Runtime.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerWeaponManager : MonoBehaviour
    {
        public const int MaxEquippedWeapons = 2;

        public int[] weaponSlots;

        private PlayerController player;
        private int slotIndex;
        private List<PlayerWeapon> registeredWeapons = new();

        public PlayerWeapon CurrentWeapon => slotIndex >= 0 && slotIndex < weaponSlots.Length ? registeredWeapons[slotIndex] : null;

        private void Awake()
        {
            player = GetComponent<PlayerController>();

            var parent = transform.Find("View/Weapons");
            foreach (Transform e in parent)
            {
                var weapon = e.GetComponent<PlayerWeapon>();
                if (!weapon) continue;
                registeredWeapons.Add(weapon);
            }

            var inputAsset = player.inputAsset;
            for (var i = 0; i < MaxEquippedWeapons; i++)
            {
                var action = inputAsset.FindAction($"Weapon.{i + 1}");
                if (action == null) continue;
                action.performed += SwitchWeaponInputCallback(i);
            }
        }

        private void Start()
        {
            EquipWeapon(0);
        }

        private Action<InputAction.CallbackContext> SwitchWeaponInputCallback(int slot) => _ =>
        {
            EquipWeapon(slot);
        };

        public void EquipWeapon(int slot)
        {
            if (CurrentWeapon) CurrentWeapon.Unequip();
            slotIndex = slot;
            if (CurrentWeapon) CurrentWeapon.Equip();
        }

        public void EquipWeapon(WeaponStatSheet identifier)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                for (var i = 0; i < registeredWeapons.Count; i++)
                {
                    var w = registeredWeapons[i];
                    if (w.statSheet != identifier) continue;
                    EquipWeapon(i);
                    return;
                }
            }

            EquipWeapon(-1);
        }
    }
}