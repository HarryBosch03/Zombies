using Framework.Runtime.Interactions;
using UnityEngine;

namespace Framework.Runtime.Player.Weapons
{
    public class WeaponPickup : Interactable
    {
        public WeaponType identifier;
        
        private GameObject model;
        private Rigidbody body;
        private new BoxCollider collider;
        private Bounds bounds;
        private float tHeight;

        public override float InteractDuration => ShortInteract;

        private void Start()
        {
            if (!TryGetComponent(out body))
            {
                body = gameObject.AddComponent<Rigidbody>();
            }

            body.drag = 3.0f;
            body.freezeRotation = true;
        }

        private void Update()
        {
            tHeight = 1.4f + Mathf.Sin(Time.time * 2.0f) * 0.15f;
            transform.rotation = Quaternion.Euler(0.0f, Time.time * 60.0f, 0.0f) * Quaternion.Euler(-45.0f, 0.0f, 0.0f);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            
            var skin = 0.1f;
            var ray = new Ray(body.position + Vector3.up * skin * 0.5f, Vector3.down);
            
            if (Physics.Raycast(ray, out var hit, tHeight + skin))
            {
                body.position = hit.point + Vector3.up * tHeight;
                body.velocity = new Vector3(body.velocity.x, Mathf.Max(0.0f, body.velocity.y), body.velocity.z);
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
            
            weaponManager.EquipWeapon(identifier);
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
                collider.isTrigger = true;
            }
            
            foreach (var e in this.model.GetComponentsInChildren<Animator>()) e.enabled = false;
        }
    }
}