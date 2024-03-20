using System;
using System.Linq;
using TMPro;
using UnityEngine;
using Framework.Runtime.Utility;

namespace Framework.Runtime.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public class PlayerUI : MonoBehaviour
    {
        private PlayerWeaponManager weaponManager;
        
        private Canvas canvas;
        private WeaponStats[] weaponStats;

        private void Awake()
        {
            weaponManager = GetComponent<PlayerWeaponManager>();
            
            canvas = transform.Find<Canvas>("Overlay");
        }

        private void Start()
        {
            weaponStats = new WeaponStats[weaponManager.equippedWeapons.Length];
            weaponStats[0] = new WeaponStats(canvas.transform.Find("WeaponStatGroup/WeaponStats").gameObject);
            for (var i = 1; i < weaponStats.Length; i++)
            {
                var instance = Instantiate(weaponStats[0].root, weaponStats[0].root.transform.parent);
                weaponStats[i] = new WeaponStats(instance);
                instance.transform.localScale = Vector3.one * 0.5f;
            }

            for (var i = 0; i < weaponStats.Length; i++)
            {
                weaponStats[i].root.name = $"[{i}] WeaponStats";
            }
        }

        private void Update()
        {
            for (var i0 = 0; i0 < weaponManager.equippedWeapons.Length; i0++)
            {
                var i1 = (i0 + weaponManager.equippedWeaponIndex) % weaponManager.equippedWeapons.Length;
                var weapon = weaponManager.weaponRegister.ElementAtOrDefault(weaponManager.equippedWeapons[i1]);
                var stats = weaponStats[i0];
                if (weapon)
                {
                    stats.text.text = $"<size=30%>[{i1 + 1}] {weapon.name}</size>\n{weapon.ammoLabel}".ToUpper();
                }
                else
                {
                    stats.text.text = $"[{i1 + 1}] Empty".ToUpper();
                }
            }
            
        }

        private struct WeaponStats
        {
            public GameObject root;
            public TMP_Text text;

            public WeaponStats(GameObject root)
            {
                this.root = root;

                text = root.transform.Find("Text").GetComponent<TMP_Text>();
            }
        }
    }
}