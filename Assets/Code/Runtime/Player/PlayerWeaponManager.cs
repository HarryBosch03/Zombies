using System;
using System.Linq;
using System.Collections.Generic;
using Framework.Runtime.Player.Weapons;
using Framework.Runtime.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Framework.Runtime.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerWeaponManager : MonoBehaviour
    {
        public const int MaxEquippedWeapons = 2;

        public int[] equippedWeapons;
        public int equippedWeaponIndex;

        private PlayerController player;
        public List<PlayerWeapon> weaponRegister { get; set; } = new();

        public PlayerWeapon currentWeapon => weaponRegister.SafeIndex(equippedWeapons.SafeIndex(equippedWeaponIndex, -1));

        private void Awake()
        {
            player = GetComponent<PlayerController>();

            var parent = transform.Find("View/Weapons");
            foreach (Transform e in parent)
            {
                var weapon = e.GetComponent<PlayerWeapon>();
                if (!weapon) continue;
                weaponRegister.Add(weapon);
            }

            var inputAsset = player.inputAsset;
            for (var i = 0; i < MaxEquippedWeapons; i++)
            {
                var action = inputAsset.FindAction($"Weapon.{i + 1}");
                if (action == null) continue;
                action.performed += SwitchWeaponInputCallback(i);
            }

            if (equippedWeapons.Length != MaxEquippedWeapons)
            {
                var newList = new int[MaxEquippedWeapons];
                for (var i = 0; i < equippedWeapons.Length && i < MaxEquippedWeapons; i++)
                {
                    newList[i] = equippedWeapons[i];
                }
                for (var i = equippedWeapons.Length; i < MaxEquippedWeapons; i++)
                {
                    newList[i] = -1;
                }
                equippedWeapons = newList;
            }
        }

        private void Start()
        {
            SwitchWeaponSlot(0);
        }

        private Action<InputAction.CallbackContext> SwitchWeaponInputCallback(int slot) => _ =>
        {
            SwitchWeaponSlot(slot);
        };

        public void PickupWeapon(string weaponIdentifier)
        {
            for (var i = 0; i < equippedWeapons.Length; i++)
            {
                var weapon = weaponRegister.ElementAtOrDefault(equippedWeapons[i]);
                if (!weapon || weapon.identifier != weaponIdentifier) continue;
                
                SwitchWeaponSlot(i);
                return;
            }
            
            var registryIndex = GetRegistryIndexFromIdentifier(weaponIdentifier);
            if (registryIndex == -1) throw new Exception($"Could not find weapon \"{weaponIdentifier}\" in registry");
            
            var index = equippedWeaponIndex;
            for (var i = 0; i < equippedWeapons.Length; i++)
            {
                if (equippedWeapons[i] != -1) continue;
                index = i;
                break;
            }
            
            if (currentWeapon) currentWeapon.Unequip();
            equippedWeapons[index] = registryIndex;
            if (currentWeapon) currentWeapon.Equip();
        }

        private int GetRegistryIndexFromIdentifier(string weaponIdentifier)
        {
            for (var i = 0; i < weaponRegister.Count; i++)
            {
                var weapon = weaponRegister[i];
                if (weapon.identifier == weaponIdentifier) return i;
            }

            return -1;
        }
        
        public void SwitchWeaponSlot(int index)
        {
            if (currentWeapon) currentWeapon.Unequip();
            equippedWeaponIndex = index;
            if (currentWeapon) currentWeapon.Equip();
        }
    }
}