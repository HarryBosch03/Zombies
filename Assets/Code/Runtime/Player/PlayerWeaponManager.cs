using System;
using Framework.Runtime.Player.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Framework.Runtime.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerWeaponManager : MonoBehaviour
    {
        public const int MaxEquippedWeapons = 2;

        private Transform weaponParent;
        private PlayerController player;
        private int weaponIndex;

        public Weapon CurrentWeapon { get; private set; }

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            weaponParent = transform.Find("View/Weapons");

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
            for (var i = 0; i < weaponParent.childCount; i++)
            {
                var weapon = weaponParent.GetChild(i).GetComponent<Weapon>();
                if (!weapon) continue;
                weapon.Pickup(player, weaponParent, i, false);
            }
            EquipWeapon(0);
        }

        private Action<InputAction.CallbackContext> SwitchWeaponInputCallback(int slot) => _ =>
        {
            EquipWeapon(slot);
        };

        public void EquipWeapon(int index)
        {
            if (CurrentWeapon) CurrentWeapon.Unequip();
            weaponIndex = index;
            CurrentWeapon = index >= 0 && index < weaponParent.childCount ? weaponParent.GetChild(index).GetComponent<Weapon>() : null;
            if (CurrentWeapon) CurrentWeapon.Equip();
        }

        public void ChangeWeapon(Weapon weapon)
        {
            if (!CanChangeWeapon(weapon)) return;
            
            if (CurrentWeapon)
            {
                CurrentWeapon.Drop();
            }
            CurrentWeapon = weapon;
            if (CurrentWeapon)
            {
                CurrentWeapon.Pickup(player, weaponParent, weaponIndex, true);
            }
        }

        private bool CanChangeWeapon(Weapon weapon)
        {
            foreach (Transform other in weaponParent)
            {
                if (!other.TryGetComponent(out Weapon otherWeapon)) continue;
                if (weapon.statSheet == otherWeapon.statSheet) return false;
            }
            return true;
        }
    }
}