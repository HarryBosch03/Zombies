using TMPro;
using UnityEngine;
using Zombies.Runtime.Utility;

namespace Zombies.Runtime.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public class PlayerUI : MonoBehaviour
    {
        private PlayerWeaponManager weaponManager;
        
        private Canvas canvas;
        private TMP_Text ammo;

        private void Awake()
        {
            weaponManager = GetComponent<PlayerWeaponManager>();
            
            canvas = transform.Find<Canvas>("Overlay");
            ammo = canvas.transform.Find<TMP_Text>("Ammo");
        }

        private void Update()
        {
            var weapon = weaponManager.CurrentWeapon;
            if (weapon)
            {
                ammo.enabled = true;
                ammo.text = $"<size=30%>{weapon.name}</size>\n{weapon.AmmoLabel}".ToUpper();
            }
            else
            {
                ammo.enabled = false;
            }
        }
    }
}