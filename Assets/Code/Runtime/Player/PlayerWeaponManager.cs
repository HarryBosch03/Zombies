using System;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Framework.Runtime.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerWeaponManager : NetworkBehaviour
    {
        public const int MaxEquippedWeapons = 2;

        public int[] equippedWeapons;

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
            for (var i = 0; i < MaxEquippedWeapons; i++)
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
                foreach (var e in registeredWeapons) e.Unequip();
                registeredWeapons[equippedWeapons[0]].Equip();
            }
        }

        private Action<InputAction.CallbackContext> SwitchWeaponInputCallback(int i) => _ =>
        {
            if (IsOwner)
            {
                var index = equippedWeapons[i];
                if (index != currentWeaponIndex) ServerRpcSwitchWeapon(index);
            }
        };

        [ServerRpc]
        private void ServerRpcSwitchWeapon(int i)
        {
            SwitchWeapon(i);
            ObserverRpcSwitchWeapon(i);
        }

        [ObserversRpc]
        private void ObserverRpcSwitchWeapon(int i) => SwitchWeapon(i);

        private void SwitchWeapon(int i)
        {
            if (CurrentWeapon) CurrentWeapon.Unequip();
            currentWeaponIndex = i;
            if (CurrentWeapon) CurrentWeapon.Equip();
        }

        public void EquipWeapon(string name, object args)
        {
            var index = -1;

            if (!string.IsNullOrWhiteSpace(name))
            {
                for (var i = 0; i < registeredWeapons.Count; i++)
                {
                    var w = registeredWeapons[i];
                    if (w.name != name) continue;

                    index = i;
                    break;
                }
            }

            if (CurrentWeapon) CurrentWeapon.Unequip();
            equippedWeapons[currentWeaponIndex] = index;
            if (CurrentWeapon) CurrentWeapon.Equip(args);
        }
    }
}