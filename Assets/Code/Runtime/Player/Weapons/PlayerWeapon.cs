using Framework.Runtime.Utility;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace Framework.Runtime.Player.Weapons
{
    public abstract class PlayerWeapon : MonoBehaviour
    {
        public string identifier;
        public WeaponStatSheet statSheet;
        
        [Range(10.0f, 120.0f)]
        public float viewportFov = 50.0f;

        public const int ViewportLayer = 3;

        [HideInInspector] public Transform leftHandHold;
        [HideInInspector] public Transform rightHandHold;

        protected Transform viewport;
        protected GameObject viewportModel;
        
        private static RenderObjects[] viewportRenderObjects;

        public Camera mainCam { get; private set; }
        public PlayerController player { get; private set; }

        public virtual string displayName => name;
        public abstract string ammoLabel { get; }
        public bool equipped { get; private set; }

        public virtual float viewportFieldOfView => viewportFov;

        protected virtual void Awake()
        {
            mainCam = Camera.main;
            player = GetComponentInParent<PlayerController>();
            
            viewport = transform.Find("Viewport");
            viewportModel = viewport.FindGameObject("Model");

            if (!player)
            {
                SpawnPickup();
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
            instance.UseModel(viewportModel);
            
            return instance;
        }

        private void OnEnable()
        {
            SetEquipState(false);
        }

        protected virtual void Update()
        {
            if (equipped)
            {
                UpdateEquipped();
                player.viewportFieldOfView = viewportFieldOfView;
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
            if (viewportModel)
            {
                viewportModel.gameObject.SetActive(state);
            }

            equipped = state;
        }

        protected virtual void OnValidate()
        {
            #if UNITY_EDITOR
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this))
            {
                identifier = name;
            }
            #endif
        }
    }
}