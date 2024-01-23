using System;
using System.Collections.Generic;
using FishNet.Object;
using Framework.Runtime.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Framework.Runtime.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerWeaponManager : NetworkBehaviour
    {
        public int[] equippedWeapons;
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
            if (IsOwner)
            {
                var index = equippedWeapons[i];
                if (index != currentWeaponIndex) ServerRpcEquipWeapons(index);
            }
        };

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
            if (CurrentWeapon) CurrentWeapon.Unequip();
            currentWeaponIndex = i;
            if (CurrentWeapon) CurrentWeapon.Equip();
        }
    }
}