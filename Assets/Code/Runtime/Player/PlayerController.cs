using System.Collections.Generic;
using Framework.Runtime.Core;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.InputSystem;

namespace Framework.Runtime.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerController : MonoBehaviour, IPersonality
    {
        public InputActionAsset inputAsset;
        public float mouseSensitivity = 0.3f;

        public RenderObjects[] viewportRenderObjects;

        private bool jumpFlag;
        private string username;

        public InputAction moveAction { get; private set; }
        public InputAction jumpAction { get; private set; }
        public InputAction shootAction { get; private set; }
        public InputAction aimAction { get; private set; }
        public InputAction reloadAction { get; private set; }
        public InputAction interactAction { get; set; }

        public PlayerMovement biped { get; private set; }
        public new PlayerCameraAnimator camera { get; private set; }

        public Vector2 viewInput { get; private set; }
        public Vector3 LookTarget => biped.center;

        public float viewportFieldOfView
        {
            get => viewportRenderObjects[0].settings.cameraSettings.cameraFieldOfView;
            set
            {
                foreach (var e in viewportRenderObjects) e.settings.cameraSettings.cameraFieldOfView = value;
            }
        }

        public static readonly List<PlayerController> All = new();

        private void Awake()
        {
            biped = GetComponent<PlayerMovement>();
            camera = GetComponent<PlayerCameraAnimator>();

            moveAction = inputAsset.FindAction("Move");
            jumpAction = inputAsset.FindAction("Jump");
            shootAction = inputAsset.FindAction("Shoot");
            aimAction = inputAsset.FindAction("Aim");
            reloadAction = inputAsset.FindAction("Reload");
            interactAction = inputAsset.FindAction("Interact");
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

        private void RpcSetUsername(string username)
        {
            this.username = username;
            name = username;
        }

        private void FixedUpdate()
        {
            var moveInput = moveAction.ReadValue<Vector2>();
            biped.moveInput = transform.TransformDirection(moveInput.x, 0.0f, moveInput.y);

            biped.jump = jumpFlag;
            jumpFlag = false;
        }

        private void Update()
        {
            var delta = Vector2.zero;
            delta += Mouse.current.delta.ReadValue() * mouseSensitivity * Mathf.Min(1.0f, Time.timeScale);
            biped.viewRotation += delta;
            viewInput = delta;

            if (jumpAction.WasPressedThisFrame()) jumpFlag = true;
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