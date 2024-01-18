using System;
using FishNet.Object;
using UnityEngine;
using Zombies.Runtime.Utility;

namespace Zombies.Runtime.Player
{
    public abstract class PlayerWeapon : NetworkBehaviour
    {
        public const int ViewportLayer = 3;

        public bool isOwner;
        
        protected GameObject model;
        protected Transform viewport;
        
        public Camera MainCam { get; private set; }
        public PlayerController Player { get; private set; }
        
        public virtual string DisplayName => name;
        public abstract string AmmoLabel { get; }
        public bool Equipped { get; private set; }
        
        protected virtual void Awake()
        {
            MainCam = Camera.main;
            Player = GetComponentInParent<PlayerController>();

            viewport = transform.Find("Viewport");
            model = viewport.FindGameObject("Model");
            
            foreach (var t in viewport.GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = ViewportLayer;
            }
        }

        private void OnEnable()
        {
            SetEquipState(false);
        }

        protected virtual void Update()
        {
            isOwner = IsOwner;
            
            if (Equipped) UpdateEquipped();
        }

        protected virtual void UpdateEquipped() { }

        public void Equip()
        {
            SetEquipState(true);
        }

        public void Unequip()
        {
            SetEquipState(false);
        }

        private void SetEquipState(bool state)
        {
            if (model)
            {
                model.gameObject.SetActive(state);
            }

            Equipped = state;
        }
    }
}
