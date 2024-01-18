using System;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Zombies.Runtime.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerWeaponManager : NetworkBehaviour
    {
        public string[] equippedWeapons;
        public int maxEquippedWeapons = 10;

        private PlayerController player;
        private int currentWeaponIndex;
        private List<PlayerWeapon> registeredWeapons = new();

        public PlayerWeapon CurrentWeapon => currentWeaponIndex >= 0 && currentWeaponIndex < registeredWeapons.Count ? registeredWeapons[currentWeaponIndex] : null;

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
            for (var i = 0; i < maxEquippedWeapons; i++)
            {
                var action = inputAsset.FindAction($"Weapon.{i + 1}");
                if (action == null) continue;
                action.performed += SwitchWeaponInputCallback(i);
            }
        }

        private void Start()
        {
            if (registeredWeapons.Count > 0)
            {
                registeredWeapons[0].Equip();
                for (var i = 1; i < registeredWeapons.Count; i++)
                {
                    registeredWeapons[i].Unequip();
                }
            }
        }

        private Action<InputAction.CallbackContext> SwitchWeaponInputCallback(int i) => _ =>
        {
            if (IsOwner) ServerRpcEquipWeapons(i);
        };

        private int NameToIndex(string name)
        {
            for (var i = 0; i < registeredWeapons.Count; i++)
            {
                var other = registeredWeapons[i].name;
                if (other != name) continue;
                return i;
            }

            return -1;
        }

        [ServerRpc]
        private void ServerRpcEquipWeapons(int i)
        {
            EquipWeapons(i);
            ObserverRpcEquipWeapons(i);
        }
        
        [ObserversRpc]
        private void ObserverRpcEquipWeapons(int i) => EquipWeapons(i);

        private void EquipWeapons(int i)
        {
            if (i == currentWeaponIndex) return;

            if (CurrentWeapon) CurrentWeapon.Unequip();
            currentWeaponIndex = i >= 0 && i < equippedWeapons.Length ? NameToIndex(equippedWeapons[i]) : -1;
            if (CurrentWeapon) CurrentWeapon.Equip();
        }
    }
}