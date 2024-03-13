using System;
using Framework.Runtime.Utility;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.Runtime.Player.Weapons
{
    public abstract class Weapon : MonoBehaviour
    {
        public WeaponStatSheet statSheet;
        
        [Range(10.0f, 120.0f)]
        public float viewportFov = 50.0f;
        
        [HideInInspector] public Transform viewport;
        [HideInInspector] public Transform world;
        
        [HideInInspector] public Transform leftHandHold;
        [HideInInspector] public Transform rightHandHold;

        public event StateChangedEventDelegate StateChangedEvent;

        private static RenderObjects[] viewportRenderObjects;

        public Camera MainCam { get; private set; }
        public PlayerController Player { get; private set; }

        public virtual string DisplayName => name;
        public abstract string AmmoLabel { get; }
        public WeaponState State { get; private set; }

        public virtual float ViewportFieldOfView => viewportFov;

        protected virtual void Awake()
        {
            MainCam = Camera.main;

            leftHandHold = transform.DeepFind("Hand.L");
            rightHandHold = transform.DeepFind("Hand.R");

            State = WeaponState.OnGround;
        }

        protected virtual void Update()
        {
            if (State == WeaponState.Equipped)
            {
                UpdateEquipped();
                Player.ViewportFieldOfView = ViewportFieldOfView;
            }
        }

        protected virtual void UpdateEquipped() { }

        public void Equip()
        {
            SetState(WeaponState.Equipped);
        }

        public void Unequip()
        {
            SetState(WeaponState.Unequipped);
        }
        
        public void Drop()
        {
            SetState(WeaponState.OnGround);
            transform.SetParent(null);
        }

        public void Pickup(PlayerController player, Transform weaponParent, int slot, bool equipped)
        {
            Player = player;
            transform.SetParent(weaponParent);
            transform.SetSiblingIndex(slot);
            SetState(equipped ? WeaponState.Equipped : WeaponState.Unequipped);
        }

        private void SetState(WeaponState newState)
        {
            StateChangedEvent?.Invoke(State, newState);
            State = newState;
        }

        public enum WeaponState
        {
            Equipped,
            Unequipped,
            OnGround,
        }
        
        public delegate void StateChangedEventDelegate(WeaponState oldState, WeaponState newState);
    }
}