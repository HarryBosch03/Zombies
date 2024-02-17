using System.Collections.Generic;
using FishNet.Object;
using Framework.Runtime.Core;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.InputSystem;

namespace Framework.Runtime.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerController : NetworkBehaviour, IPersonality
    {
        public InputActionAsset inputAsset;
        public float mouseSensitivity = 0.3f;

        public RenderObjects[] viewportRenderObjects;

        private bool jumpFlag;
        private string username;

        public InputAction MoveAction { get; private set; }
        public InputAction JumpAction { get; private set; }
        public InputAction ShootAction { get; private set; }
        public InputAction AimAction { get; private set; }
        public InputAction ReloadAction { get; private set; }
        public InputAction InteractAction { get; set; }
        public PlayerMovement Biped { get; private set; }
        public Vector2 ViewInput { get; private set; }
        public Vector3 LookTarget => Biped.Center;

        public float ViewportFieldOfView
        {
            get => viewportRenderObjects[0].settings.cameraSettings.cameraFieldOfView;
            set
            {
                if (!IsOwner) return;
                foreach (var e in viewportRenderObjects) e.settings.cameraSettings.cameraFieldOfView = value;
            }
        }
        
        public static readonly List<PlayerController> All = new();

        private void Awake()
        {
            Biped = GetComponent<PlayerMovement>();

            MoveAction = inputAsset.FindAction("Move");
            JumpAction = inputAsset.FindAction("Jump");
            ShootAction = inputAsset.FindAction("Shoot");
            AimAction = inputAsset.FindAction("Aim");
            ReloadAction = inputAsset.FindAction("Reload");
            InteractAction = inputAsset.FindAction("Interact");
        }

        private void OnEnable()
        {
            inputAsset.Enable();
            
            Cursor.lockState = CursorLockMode.Locked;
            All.Add(this);
        }

        private void OnDisable()
        {
            inputAsset.Disable();
            Cursor.lockState = CursorLockMode.None;

            All.Remove(this);
        }

        public override void OnStartNetwork()
        {
            if (Owner.IsLocalClient)
            {
                username = Reference.FirstNames[Random.Range(0, Reference.FirstNames.Length)];
                RpcSetUsername(username);
            }
        }

        private void RpcSetUsername(string username)
        {
            this.username = username;
            name = username;
        }

        private void FixedUpdate()
        {
            if (IsOwner)
            {
                var moveInput = MoveAction.ReadValue<Vector2>();
                Biped.moveInput = transform.TransformDirection(moveInput.x, 0.0f, moveInput.y);

                Biped.jump = jumpFlag;
                jumpFlag = false;
            }
        }

        private void Update()
        {
            if (!IsOwner) return;

            var delta = Vector2.zero;
            delta += Mouse.current.delta.ReadValue() * mouseSensitivity * Mathf.Min(1.0f, Time.timeScale);
            Biped.viewRotation += delta;
            ViewInput = delta;

            if (JumpAction.WasPressedThisFrame()) jumpFlag = true;
        }

        public static void EnableInput(bool state)
        {
            foreach (var p in All)
            {
                if (p.inputAsset.enabled == state) continue;

                if (state) p.inputAsset.Enable();
                else p.inputAsset.Disable();
            }
        }
    }
}