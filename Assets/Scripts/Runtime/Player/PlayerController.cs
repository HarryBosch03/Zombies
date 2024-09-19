using System;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Zombies.Runtime.Health;
using Zombies.Runtime.Interactive;
using Zombies.Runtime.Utility;
using Random = UnityEngine.Random;

namespace Zombies.Runtime.Player
{
    [RequireComponent(typeof(CharacterController))]
    public partial class PlayerController : NetworkBehaviour
    {
        public float mouseSensitivity = 0.3f;
        public float controllerSensitivity = 0.3f;
        public float interactDistance = 2f;
        public TMP_Text lookingAtUI;
        public Vector2 lookingAtUIOffset;
        public Shaker lookingAtUIShaker;
        public GameObject[] viewerVisible;
        public GameObject[] ovserverVisible;

        [Space]
        public Animator knifeAnimator;
        public float knifeTotalTime;
        public float knifeRepeatTime;
        public DamageArgs knifeDamage;
        public float knifeRange;
        public float knifeDamageTime;
        public Transform knifeOffsets;

        private Camera mainCamera;
        private InputActionMap input;
        private bool interact;
        private int knifeCooldown;
        private bool knifeInputBuffered;
        private InputData pending;

        public static event Action<PlayerController> FailedInteractionEvent;
        
        public CharacterController character { get; private set; }
        public PlayerPoints points { get; private set; }
        public bool isActiveViewer => activeViewer == this;
        public bool isControlling { get; set; }

        public static readonly List<PlayerController> All = new();
        private static PlayerController activeViewerInternal;
        public static PlayerController activeViewer
        {
            get => activeViewerInternal;
            set
            {
                if (activeViewerInternal != null) activeViewerInternal.UpdateViewerSpecificVisuals(false);
                if (value != null) value.UpdateViewerSpecificVisuals(true);
                activeViewerInternal = value;
            }
        }

        private void UpdateViewerSpecificVisuals(bool isViewer)
        {
            foreach (var e in viewerVisible) e.SetActive(isViewer);
            foreach (var e in ovserverVisible) e.SetActive(!isViewer);

            character.SetIsActiveViewer(isViewer);
            
            Debug.Log($"{name}: {isViewer}");
        }

        public static PlayerController localPlayer { get; set; }

        private void Awake()
        {
            mainCamera = Camera.main;

            input = InputSystem.actions.FindActionMap("Player");
            character = GetComponent<CharacterController>();
            points = GetComponent<PlayerPoints>();
            
            UpdateViewerSpecificVisuals(false);
        }

        private void OnEnable()
        {
            if (knifeOffsets != null) character.cameraModifiers.Add(knifeOffsets);
        }

        private void OnDisable()
        {
            if (knifeOffsets != null) character.cameraModifiers.Remove(knifeOffsets);
        }

        public override void OnStartNetwork()
        {
            if (Owner.IsLocalClient)
            {
                localPlayer = this;
                isControlling = true;
                activeViewer = this;
            }
            
            TimeManager.OnTick += OnTick;
            FailedInteractionEvent += OnFailedInteraction;

            All.Add(this);
        }

        public override void OnStopNetwork()
        {
            TimeManager.OnTick -= OnTick;
            FailedInteractionEvent -= OnFailedInteraction;

            All.Remove(this);
        }

        private void OnFailedInteraction(PlayerController player)
        {
            if (player != this) return;
            lookingAtUIShaker.Shake();
        }

        private void Update()
        {
            var inputData = pending;
            
            if (IsOwner && isControlling)
            {
                var moveInput = input.FindAction("Move").ReadValue<Vector2>();
                inputData.moveDirection = transform.TransformDirection(moveInput.x, 0f, moveInput.y);

                if (InputDown("Jump")) inputData.jump = true;
                if (InputDown("Hold Run")) inputData.run = true;
                if (InputUp("Hold Run")) inputData.run = false;
                if (InputUp("Toggle Run")) inputData.run = !character.run;

                if (InputDown("Primary Weapon")) inputData.switchWeapon = 1;
                if (InputDown("Secondary Weapon")) inputData.switchWeapon = 2;
                inputData.shoot = InputPressed("Shoot");
                inputData.aiming = InputPressed("Aim");
                if (InputDown("Reload")) inputData.reload = true;

                if (InputDown("Interact")) inputData.interact = true;
                if (InputDown("Knife")) inputData.useKnife = true;

                var m = Mouse.current;
                var gp = Gamepad.current;
                Cursor.lockState = CursorLockMode.Locked;

                var lookDelta = Vector2.zero;
                if (m != null) lookDelta += m.delta.ReadValue() * mouseSensitivity;
                if (gp != null) lookDelta += gp.rightStick.ReadValue() * controllerSensitivity * 500f * Time.deltaTime;

                var tanLength = Mathf.Tan(character.currentFieldOfView * 0.5f * Mathf.Deg2Rad);
                inputData.lookDelta += lookDelta * tanLength;
            }
            else
            {
                inputData = default;
                if (IsOwner) Cursor.lockState = CursorLockMode.None;
            }

            character.deltaRotation = inputData.lookDelta;
            pending = inputData;

            var kb = Keyboard.current;
            if (kb.escapeKey.wasPressedThisFrame)
            {
                isControlling = !isControlling;
            }

            lookingAtUIShaker.Update();
            lookingAtUI.rectTransform.anchoredPosition = lookingAtUIOffset + lookingAtUIShaker.offset;
        }

        private void OnTick()
        {
            Replicate(CreateInputData());
            CreateReconcile();
        }

        private InputData CreateInputData()
        {
            var data = pending;
            pending.Consume();
            return data;
        }

        [Replicate]
        private void Replicate(InputData inputData, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
        {
            character.moveDirection = inputData.moveDirection;
            character.jump = inputData.jump;
            character.run = inputData.run;
            if (inputData.switchWeapon > 0) character.SwitchToWeapon(inputData.switchWeapon - 1);
            character.shoot = inputData.shoot;
            character.aiming = inputData.aiming;
            character.reload = inputData.reload;
            interact = inputData.interact;
            character.deltaRotation = inputData.lookDelta;

            AttackWithKnife(inputData);

            character.Simulate();
            CheckForInteractables();
        }

        private void AttackWithKnife(InputData inputData)
        {
            var knifeTotalFrames = Mathf.RoundToInt(knifeTotalTime * TimeManager.TickRate);
            var knifeRepeatFrame = knifeTotalFrames - Mathf.RoundToInt((knifeRepeatTime) * TimeManager.TickRate);
            var knifeDamageFrame = knifeTotalFrames - Mathf.RoundToInt(knifeDamageTime * TimeManager.TickRate);
            
            if (knifeCooldown == knifeDamageFrame)
            {
                DealKnifeDamage();
            }
            
            if (knifeCooldown == 0) character.ForceWeaponHolstered(false);
            if (knifeInputBuffered && knifeCooldown <= knifeRepeatFrame)
            {
                knifeInputBuffered = false;
                UseKnife();
            }
            else if (inputData.useKnife)
            {
                if (knifeCooldown <= knifeRepeatFrame) UseKnife();
                else knifeInputBuffered = true;
            }
            knifeCooldown--;
        }

        private void DealKnifeDamage()
        {
            var ray = new Ray(transform.position + character.headOffset, character.head.forward);
            if (Physics.Raycast(ray, out var hit, knifeRange))
            {
                var health = hit.collider.GetComponentInParent<HealthController>();
                if (health != null)
                {
                    health.TakeDamage(knifeDamage.UpdateWithContext(gameObject, hit.point, transform.right, hit.collider));
                }
            }
        }

        private void UseKnife()
        {
            knifeCooldown = Mathf.RoundToInt(knifeTotalTime * TimeManager.TickRate);
            knifeAnimator.SetInteger("random", Random.value > 0.5f ? 1 : 0);
            knifeAnimator.SetTrigger("swing");
            character.ForceWeaponHolstered(true);
        }

        public override void CreateReconcile()
        {
            var data = new ReconcileData
            {
                position = transform.position,
                velocity = character.velocity,
                rotation = character.rotation,
                moveState = character.moveState,
                knifeCooldown = knifeCooldown,
                knifeInputBuffered = knifeInputBuffered,
            };
            Reconcile(data);
        }

        [Reconcile]
        private void Reconcile(ReconcileData data, Channel channel = Channel.Unreliable)
        {
            transform.position = data.position;
            character.velocity = data.velocity;
            character.rotation = data.rotation;
            character.moveState = data.moveState;
            knifeCooldown = data.knifeCooldown;
            knifeInputBuffered = data.knifeInputBuffered;
        }

        private void CheckForInteractables()
        {
            lookingAtUI.text = "";

            var ray = new Ray(character.head.position, character.head.forward);
            if (Physics.Raycast(ray, out var hit, interactDistance))
            {
                var interactive = hit.collider.GetComponentInParent<IInteractive>();
                if (interactive != null && interactive.isActiveAndEnabled)
                {
                    var cost = interactive.GetCost(this);
                    lookingAtUI.text = cost != 0 ? $"{interactive.GetDisplayText(this)} [{cost}]" : interactive.GetDisplayText(this);
                    if (IsServerStarted && interact)
                    {
                        if (points.currentPoints.Value >= cost && interactive.Interact(this))
                        {
                            points.DeductPoints(cost);
                        }
                        else
                        {
                            NotifyFailedInteraction();
                        }
                    }
                }
            }

            interact = false;
        }

        [ObserversRpc(RunLocally = true)]
        private void NotifyFailedInteraction() => FailedInteractionEvent?.Invoke(this);

        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (!Application.isPlaying)
            {
                if (lookingAtUI != null)
                {
                    lookingAtUIOffset = lookingAtUI.rectTransform.anchoredPosition;
                }
            }
        }

        private float InputAxis(string name) => input.FindAction(name).ReadValue<float>();
        private bool InputPressed(string name) => input.FindAction(name).IsPressed();
        private bool InputDown(string name) => input.FindAction(name).WasPressedThisFrame();
        private bool InputUp(string name) => input.FindAction(name).WasReleasedThisFrame();
        
        public struct InputData : IReplicateData
        {
            public Vector3 moveDirection;
            public bool jump;
            public bool run;
            public int switchWeapon;
            public bool shoot;
            public bool aiming;
            public bool reload;
            public bool interact;
            public bool useKnife;
            public Vector2 lookDelta;
            
            private uint tick;
            public uint GetTick() => tick;
            public void SetTick(uint value) => tick = value;
            public void Dispose() {  }

            public void Consume()
            {
                jump = false;
                switchWeapon = 0;
                reload = false;
                interact = false;
                useKnife = false;
                lookDelta = default;
            }
        }

        public struct ReconcileData : IReconcileData
        {
            public Vector3 position;
            public Vector3 velocity;
            public Vector2 rotation;
            public int knifeCooldown;
            public bool knifeInputBuffered;
            public CharacterController.MoveState moveState;
            
            private uint tick;
            public uint GetTick() => tick;
            public void SetTick(uint value) => tick = value;
            public void Dispose() {  }
        }

        public static PlayerController GetRandomPlayer() => All.Count > 0 ? All[Random.Range(0, All.Count - 1)] : null;
    }
}