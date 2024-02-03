using FishNet.Object;
using UnityEngine;
using Framework.Runtime.Rendering;
using Framework.Runtime.Utility;

namespace Framework.Runtime.Player
{
    public abstract class PlayerWeapon : NetworkBehaviour
    {
        [Range(10.0f, 120.0f)]
        public float viewportFov = 50.0f;

        public const int ViewportLayer = 3;

        public bool isOwner;

        [HideInInspector] public Transform leftHandHold;
        [HideInInspector] public Transform rightHandHold;

        protected GameObject model;
        protected Transform viewport;

        public Camera MainCam { get; private set; }
        public PlayerController Player { get; private set; }

        public virtual string DisplayName => name;
        public abstract string AmmoLabel { get; }
        public bool Equipped { get; private set; }

        public virtual float ViewportFieldOfView => viewportFov;

        protected virtual void Awake()
        {
            MainCam = Camera.main;
            Player = GetComponentInParent<PlayerController>();

            viewport = transform.Find("Viewport");
            model = viewport.FindGameObject("Model");

            leftHandHold = transform.DeepFind("Hand.L");
            rightHandHold = transform.DeepFind("Hand.R");

            foreach (var t in viewport.GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = ViewportLayer;
            }
        }

        private void OnEnable() { SetEquipState(false); }

        protected virtual void Update()
        {
            isOwner = IsOwner;

            if (Equipped)
            {
                UpdateEquipped();
                ViewportOverlayPass.ViewportFieldOfView = ViewportFieldOfView;
            }
        }

        protected virtual void UpdateEquipped() { }

        public void Equip(object args = null)
        {
            SetEquipState(true);
            OnEquip();
        }

        public void Unequip()
        {
            SetEquipState(false);
            OnUnequip();
        }
        
        public virtual void OnEquip() { }
        public virtual void OnUnequip() { }

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