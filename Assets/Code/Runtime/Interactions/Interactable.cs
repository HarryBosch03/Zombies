using System;
using UnityEngine;

namespace Framework.Runtime.Interactions
{
    public abstract class Interactable : MonoBehaviour
    {
        public const float ShortInteract = 0.3f;
        
        private GameObject interactor;
        private Action finishInteractionCallback;

        public abstract float InteractDuration { get; }
        public float InteractionTimer { get; private set; }
        public float InteractionPercent => InteractionTimer / InteractDuration;
        
        public GameObject Interactor => interactor;

        protected virtual void FixedUpdate()
        {
            if (interactor)
            {
                InteractionTimer += Time.deltaTime;
                if (InteractionTimer > InteractDuration)
                {
                    EndInteract(true);
                }
            }
            else
            {
                InteractionTimer = Mathf.Max(0, InteractionTimer - Time.deltaTime);
            }
        }

        public bool StartInteract(GameObject interactor, Action finishInteractionCallback)
        {
            if (this.interactor) return false;
            if (!interactor) return false;

            this.interactor = interactor;
            this.finishInteractionCallback = finishInteractionCallback;

            OnStartInteract();
            return true;
        }

        public void EndInteract() => EndInteract(false);

        private void EndInteract(bool finished)
        {
            OnEndInteract(finished);
            
            if (finished) InteractionTimer = 0.0f;
            interactor = null;

            finishInteractionCallback?.Invoke();
        }

        protected abstract void OnStartInteract();
        protected abstract void OnEndInteract(bool finished);
    }
}