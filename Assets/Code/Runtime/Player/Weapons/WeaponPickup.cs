using System;
using Framework.Runtime.Interactions;
using Framework.Runtime.Utility;
using UnityEditor.Graphs;
using UnityEngine;

namespace Framework.Runtime.Player.Weapons
{
    [RequireComponent(typeof(Interactable), typeof(Weapon))]
    public class WeaponPickup : MonoBehaviour
    {
        private Rigidbody body;
        private new BoxCollider collider;
        private Bounds bounds;
        private float tHeight;

        private Interactable interactable;
        private Weapon weapon;
        
        private Transform world;

        private void Awake()
        {
            UseViewportModel();

            interactable = GetComponent<Interactable>();
            weapon = GetComponent<Weapon>();
            weapon.StateChangedEvent += OnStateChanged;
            
            if (!TryGetComponent(out body))
            {
                body = gameObject.AddComponent<Rigidbody>();
            }

            body.drag = 3.0f;
            body.freezeRotation = true;

            Show();
        }

        private void OnStateChanged(Weapon.WeaponState oldstate, Weapon.WeaponState newstate)
        {
            switch (newstate)
            {
                case Weapon.WeaponState.Equipped:
                case Weapon.WeaponState.Unequipped:
                {
                    Hide();
                    break;
                }
                case Weapon.WeaponState.OnGround:
                {
                    Show();
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(newstate), newstate, null);
                }
            }
        }

        private void Show()
        {
            world.gameObject.SetActive(true);
            body.isKinematic = false;
            collider.enabled = true;
        }

        private void Hide()
        {
            world.gameObject.SetActive(false);
            body.isKinematic = true;
            collider.enabled = false;
        }

        private void OnEnable()
        {
            interactable.EndInteractEvent += OnEndInteract;
        }

        private void OnDisable()
        {
            interactable.EndInteractEvent -= OnEndInteract;
        }

        private void Update()
        {
            tHeight = 1.4f + Mathf.Sin(Time.time * 2.0f) * 0.15f;
            transform.rotation = Quaternion.Euler(0.0f, Time.time * 60.0f, 0.0f) * Quaternion.Euler(-45.0f, 0.0f, 0.0f);
        }

        private void FixedUpdate()
        {
            var skin = 0.1f;
            var ray = new Ray(body.position + Vector3.up * skin * 0.5f, Vector3.down);
            
            if (Physics.Raycast(ray, out var hit, tHeight + skin))
            {
                body.position = hit.point + Vector3.up * tHeight;
                body.velocity = new Vector3(body.velocity.x, Mathf.Max(0.0f, body.velocity.y), body.velocity.z);
            }
        }

        private void OnEndInteract(bool finished, GameObject interactor)
        {
            if (!finished) return;
            
            var weaponManager = interactor.GetComponent<PlayerWeaponManager>();
            if (!weaponManager) return;
            
            weaponManager.ChangeWeapon(weapon);
        }

        private void UseViewportModel()
        {
            var viewport = transform.Find("Viewport");
            
            world = Instantiate(viewport.gameObject, viewport.parent).transform;
            world.name = "World";
            
            foreach (var t in world.GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = 0;
            }

            var model = world.Find("Model");
            
            bounds = new Bounds();
            foreach (var e in model.GetComponentsInChildren<Renderer>(true))
            {
                bounds.Encapsulate(e.bounds);
            }
            
            model.localPosition = -bounds.center;

            if (bounds.size.magnitude > float.Epsilon)
            {
                collider = gameObject.AddComponent<BoxCollider>();
                collider.size = bounds.size;
                collider.isTrigger = true;
            }

            var animator = model.GetComponent<Animator>();
            if (animator) Destroy(animator);
        }
    }
}