using System;
using System.Collections.Generic;
using FishNet.Object;
using Framework.Runtime.Player.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Framework.Runtime.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerWeaponManager : NetworkBehaviour
    {
        public const int MaxEquippedWeapons = 2;

        public WeaponType[] weaponSlots;

        private PlayerController player;
        private int currentWeaponRegistryIndex;
        private List<PlayerWeapon> registeredWeapons = new();

        public PlayerWeapon CurrentWeapon => currentWeaponRegistryIndex >= 0 && currentWeaponRegistryIndex < registeredWeapons.Count ? registeredWeapons[currentWeaponRegistryIndex] : null;

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

        public override void OnStartClient()
        {
            EquipWeapon(weaponSlots[0]);
        }

        private Action<InputAction.CallbackContext> SwitchWeaponInputCallback(int slot) => _ =>
        {
            EquipWeapon(weaponSlots[slot]);
        };

        public void EquipWeaponFromRegistry(int index)
        {
            if (!IsOwner) return;
            RpcSwitchWeapon(index);
        }

        [ObserversRpc(ExcludeOwner = false, ExcludeServer = false)]
        private void RpcSwitchWeapon(int index)
        {
            if (CurrentWeapon) CurrentWeapon.Unequip();
            currentWeaponRegistryIndex = index;
            if (CurrentWeapon) CurrentWeapon.Equip();
        }

        public void EquipWeapon(WeaponType type)
        {
            if (!IsOwner) return;
            if (!string.IsNullOrWhiteSpace(name))
            {
                for (var i = 0; i < registeredWeapons.Count; i++)
                {
                    var w = registeredWeapons[i];
                    if (w.identifier != type) continue;
                    EquipWeaponFromRegistry(i);
                    return;
                }
            }

            EquipWeaponFromRegistry(-1);
        }
    }
}