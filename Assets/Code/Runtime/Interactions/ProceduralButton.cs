using System;
using UnityEngine;

namespace Framework.Runtime.Interactions
{
    [RequireComponent(typeof(Interactable))]
    public class ProceduralButton : MonoBehaviour
    {
        public bool latch;
        public float spring = 600.0f;
        public float damper = 35.0f;

        public bool state;
        
        [Range(-0.5f, 1.5f)]
        public float cPosition;
        
        [Range(-0.5f, 1.5f)] 
        public float tPosition;

        public AnimationDriver[] drivers;
        
        private float cVelocity;
        private Interactable interactable;

        public Action<bool> stateChangedEvent;

        private void Awake()
        {
            interactable = GetComponent<Interactable>();
        }

        private void OnEnable()
        {
            interactable.EndInteractEvent += OnEndInteract;
        }

        private void OnDisable()
        {
            interactable.EndInteractEvent -= OnEndInteract;
        }

        private void OnEndInteract(bool finished, GameObject interactor)
        {
            if (!finished) return;
            ChangeState();
        }

        protected void FixedUpdate()
        {
            var force = (tPosition - cPosition) * spring - cVelocity * damper;
            cPosition += cVelocity * Time.deltaTime;
            cVelocity += force * Time.deltaTime;

            if (!latch)
            {
                if (cPosition > 1.0f || cPosition > 0.5f && cVelocity < 0.0f)
                {
                    tPosition = 0.0f;
                }
            }
            
            foreach (var d in drivers)
            {
                d.Animate(cPosition);
            }
        }

        private void ChangeState()
        {
            if (latch)
            {
                state = !state;
                tPosition = state ? 1.0f : 0.0f;
                stateChangedEvent?.Invoke(state);
            }
            else
            {
                tPosition = 1.0f;
                stateChangedEvent?.Invoke(true);
                stateChangedEvent?.Invoke(false);
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;

            cPosition = state ? 1.0f : 0.0f;

            if (drivers != null)
            {
                foreach (var d in drivers)
                {
                    d.Animate(cPosition);
                }
            }
        }

        [Serializable]
        public class AnimationDriver
        {
            public Transform target;
            
            [Space]
            public bool animatePosition;
            public Vector3 positionMin;
            public Vector3 positionMax;
            
            [Space]
            public bool animateRotation;
            public Vector3 rotationMin;
            public Vector3 rotationMax;

            public void Animate(float position)
            {
                if (!target) return;

                if (animatePosition)
                {
                    target.localPosition = Vector3.LerpUnclamped(positionMin, positionMax, position);
                }

                if (animateRotation)
                {
                    target.localRotation = Quaternion.SlerpUnclamped(Quaternion.Euler(rotationMin), Quaternion.Euler(rotationMax), position);
                }
            }
        }
    }
}