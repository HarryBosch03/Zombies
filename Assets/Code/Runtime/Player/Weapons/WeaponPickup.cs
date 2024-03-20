using Framework.Runtime.Interactions;
using UnityEngine;

namespace Framework.Runtime.Player.Weapons
{
    public class WeaponPickup : Interactable
    {
        public string identifier;
        
        private GameObject model;
        private Rigidbody body;
        private new BoxCollider collider;
        private Bounds bounds;

        public override float InteractDuration => ShortInteract;

        private void Start()
        {
            if (!TryGetComponent(out body))
            {
                body = gameObject.AddComponent<Rigidbody>();
            } 
        }

        protected override void OnStartInteract()
        {
            
        }

        protected override void OnEndInteract(bool finished)
        {
            if (!finished) return;
            
            var weaponManager = Interactor.GetComponent<PlayerWeaponManager>();
            if (!weaponManager) return;
            
            weaponManager.PickupWeapon(identifier);
        }

        public void UseModel(GameObject model)
        {
            this.model = Instantiate(model);
            this.model.transform.position = Vector3.zero;
            this.model.transform.rotation = Quaternion.identity;

            bounds = new Bounds();
            foreach (var e in this.model.GetComponentsInChildren<Renderer>(true))
            {
                bounds.Encapsulate(e.bounds);
            }
            
            this.model.transform.SetParent(transform);
            this.model.transform.localPosition = -bounds.center;

            if (bounds.size.magnitude > float.Epsilon)
            {
                collider = gameObject.AddComponent<BoxCollider>();
                collider.size = bounds.size;
            }
            
            foreach (var e in this.model.GetComponentsInChildren<Animator>()) e.enabled = false;
        }
    }
}