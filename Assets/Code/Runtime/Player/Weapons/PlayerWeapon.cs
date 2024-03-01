using Framework.Runtime.Utility;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace Framework.Runtime.Player.Weapons
{
    public abstract class PlayerWeapon : MonoBehaviour
    {
        public WeaponStatSheet statSheet;
        
        [Range(10.0f, 120.0f)]
        public float viewportFov = 50.0f;

        public const int ViewportLayer = 3;

        [HideInInspector] public Transform leftHandHold;
        [HideInInspector] public Transform rightHandHold;

        protected Transform Viewport { get; private set; }
        public GameObject Model { get; private set; }
        
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
            
            Viewport = transform.Find("Viewport");
            Model = Viewport.FindGameObject("Model");
            
            if (!Player)
            {
                WeaponPickup.Of(this);
                Destroy(gameObject);
                return;
            }
   
            leftHandHold = transform.DeepFind("Hand.L");
            rightHandHold = transform.DeepFind("Hand.R");

            foreach (var t in Viewport.GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = ViewportLayer;
            }

            Unequip();
        }

        private void OnEnable()
        {
            SetEquipState(false);
        }

        protected virtual void Update()
        {
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
            if (Viewport)
            {
                Viewport.gameObject.SetActive(state);
            }

            Equipped = state;
        }
    }
}