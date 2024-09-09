using System;
using UnityEngine;
using UnityEngine.AI;

namespace Zombies.Runtime.GameMeta
{
    [RequireComponent(typeof(Purchasable))]
    public class Door : MonoBehaviour
    {
        public Animator animator;
        public Collider doorCollider;
        public NavMeshObstacle navMeshDoorBlocker;
        
        private Purchasable purchasable;

        private void Awake()
        {
            purchasable = GetComponent<Purchasable>();
        }

        private void OnEnable()
        {
            purchasable.PurchaseEvent += OnPurchase;
        }

        private void OnDisable()
        {
            purchasable.PurchaseEvent -= OnPurchase;
        }

        private void Start()
        {
            SetOpen(false);
        }

        private void OnPurchase()
        {
            SetOpen(true);
        }

        public void SetOpen(bool isOpen)
        {
            if (animator != null) animator.SetBool("open", isOpen);
            if (doorCollider != null) doorCollider.enabled = !isOpen;
            if (navMeshDoorBlocker != null) navMeshDoorBlocker.enabled = !isOpen;
        }
    }
}