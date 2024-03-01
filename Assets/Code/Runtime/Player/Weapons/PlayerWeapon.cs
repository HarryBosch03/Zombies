using Framework.Runtime.Utility;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace Framework.Runtime.Player.Weapons
{
    public abstract class PlayerWeapon : MonoBehaviour
    {
        public WeaponType identifier;
        public WeaponStatSheet statSheet;
        
        [Range(10.0f, 120.0f)]
        public float viewportFov = 50.0f;

        public const int ViewportLayer = 3;

        [HideInInspector] public Transform leftHandHold;
        [HideInInspector] public Transform rightHandHold;

        protected Transform viewport;
        protected GameObject viewportModel;
        
        protected Transform world;
        protected GameObject worldModel;

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
            viewportModel = viewport.FindGameObject("Model");

            InstanceItemModel();
            
            leftHandHold = transform.DeepFind("Hand.L");
            rightHandHold = transform.DeepFind("Hand.R");

            foreach (var t in viewport.GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = ViewportLayer;
            }
            
            Unequip();
        }

        private void InstanceItemModel()
        {
            world = new GameObject("World").transform;
            world.SetParent(transform);
            world.ResetPose();

            worldModel = Instantiate(viewportModel);
            worldModel.transform.ResetPose();
            
            var bounds = new Bounds();
            foreach (var e in worldModel.GetComponentsInChildren<Renderer>(true))
            {
                bounds.Encapsulate(e.bounds);
            }
            
            worldModel.transform.SetParent(world);
            worldModel.transform.localPosition = -bounds.center;

            if (bounds.size.magnitude > float.Epsilon)
            {
                var collider = gameObject.AddComponent<BoxCollider>();
                collider.size = bounds.size;
                collider.isTrigger = true;
            }
            
            foreach (var e in worldModel.GetComponentsInChildren<Animator>()) Destroy(e);
            
            world.gameObject.SetActive(false);
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
            if (viewportModel)
            {
                viewportModel.gameObject.SetActive(state);
            }

            Equipped = state;
        }
    }
}