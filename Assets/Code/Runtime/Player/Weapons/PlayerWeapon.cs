using FishNet.Object;
using Framework.Runtime.Utility;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace Framework.Runtime.Player.Weapons
{
    public abstract class PlayerWeapon : NetworkBehaviour
    {
        public WeaponType identifier;
        public WeaponStatSheet statSheet;
        
        [Range(10.0f, 120.0f)]
        public float viewportFov = 50.0f;

        public const int ViewportLayer = 3;

        public bool isOwner;

        [HideInInspector] public Transform leftHandHold;
        [HideInInspector] public Transform rightHandHold;

        protected GameObject model;
        protected Transform viewport;

        private static RenderObjects[] viewportRenderObjects;

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

            if (!Player)
            {
                SpawnPickup();
                
                Destroy(gameObject);
                return;
            }
            
            leftHandHold = transform.DeepFind("Hand.L");
            rightHandHold = transform.DeepFind("Hand.R");

            foreach (var t in viewport.GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = ViewportLayer;
            }
            
            Unequip();
        }

        private WeaponPickup SpawnPickup()
        {
            var instance = new GameObject($"Weapon Pickup [{name}]").AddComponent<WeaponPickup>();

            instance.transform.position = transform.position;
            instance.identifier = identifier;
            instance.UseModel(model);
            
            return instance;
        }

        private void OnEnable()
        {
            SetEquipState(false);
        }

        protected virtual void Update()
        {
            isOwner = IsOwner;

            if (Equipped)
            {
                UpdateEquipped();
                Player.ViewportFieldOfView = ViewportFieldOfView;
            }
        }

        protected virtual void UpdateEquipped() { }

        public void Equip()
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