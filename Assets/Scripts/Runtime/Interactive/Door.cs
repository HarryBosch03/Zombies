using System;
using UnityEngine;
using UnityEngine.AI;
using Zombies.Runtime.Player;

namespace Zombies.Runtime.Interactive
{
    public class Door : MonoBehaviour, IInteractive
    {
        public int cost = 1000;
        public string displayText = "Unlock Door";
        
        [Space]
        public Animator animator;
        public Collider doorCollider;
        public NavMeshObstacle navMeshDoorBlocker;

        private void OnEnable()
        {
            SetOpen(false);
        }

        public void SetOpen(bool isOpen)
        {
            enabled = !isOpen;
            
            if (animator != null) animator.SetBool("open", isOpen);
            if (doorCollider != null) doorCollider.enabled = !isOpen;
            if (navMeshDoorBlocker != null) navMeshDoorBlocker.enabled = !isOpen;
        }

        public int GetCost(PlayerController player) => cost;
        public string GetDisplayText(PlayerController player) => displayText;

        public bool Interact(PlayerController player)
        {
            SetOpen(true);
            return true;
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                if (doorCollider == null) doorCollider = GetComponentInChildren<Collider>();
                if (animator == null) animator = GetComponentInChildren<Animator>();
                if (navMeshDoorBlocker == null) navMeshDoorBlocker = GetComponentInChildren<NavMeshObstacle>();
            }
        }
    }
}