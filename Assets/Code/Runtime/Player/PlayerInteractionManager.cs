using System;
using FishNet.Object;
using Framework.Runtime.Interactions;
using Framework.Runtime.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.Runtime.Player
{
    public class PlayerInteractionManager : NetworkBehaviour
    {
        public float interactionDistance = 3.0f;

        private TMP_Text hudText;
        private Image hudFill;
        
        private bool input;
        private bool lastInput;
        
        private Camera mainCamera;
        private PlayerController player;

        private Interactable currentInteractable;
        private Vector3 interactionLocalPosition;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            mainCamera = Camera.main;

            hudText = transform.Find<TMP_Text>("Overlay/InteractionText");
            hudFill = hudText.transform.GetChild(0).GetComponent<Image>();
        }

        private void OnEnable()
        {
            UpdateHud(null);
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;

            lastInput = input;
            input = player.InteractAction.IsPressed();

            if (!currentInteractable)
            {
                var lookingAt = GetLookingAt();
                UpdateHud(lookingAt);
                
                if (lookingAt && WantsToInteract())
                {
                    StartInteraction(lookingAt);
                }
            }
            else
            {
                UpdateHud(null);
                
                var point = currentInteractable.transform.TransformPoint(interactionLocalPosition);
                if (!input || (point - mainCamera.transform.position).magnitude > interactionDistance)
                {
                    CancelInteraction();
                }
            }
        }

        private Interactable GetLookingAt()
        {
            var ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            if (!Physics.Raycast(ray, out var hit, interactionDistance)) return null;

            var interactable = hit.collider.GetComponentInParent<Interactable>();
            if (interactable) interactionLocalPosition = interactable.transform.InverseTransformPoint(hit.point);
            
            return interactable;
        }

        private void StartInteraction(Interactable target)
        {
            if (!target.StartInteract(gameObject, FinishInteraction)) return;
            currentInteractable = target;
        }

        private void FinishInteraction() { currentInteractable = null; }

        private void CancelInteraction()
        {
            if (!currentInteractable) return;
            
            currentInteractable.EndInteract();
            currentInteractable = null;
        }

        public void UpdateHud(Interactable lookingAt)
        {
            var percent = 0.0f;
            if (currentInteractable) percent = currentInteractable.InteractionPercent;
            else if (lookingAt) percent = lookingAt.InteractionPercent;
            
            if (hudText)
            {
                if (currentInteractable) hudText.text = currentInteractable.name;
                else if (lookingAt) hudText.text = lookingAt.name;
                else hudText.text = string.Empty;
            }

            if (hudFill)
            {
                hudFill.fillAmount = percent;
            }
        }

        public bool WantsToInteract() => input && !lastInput;
    }
}