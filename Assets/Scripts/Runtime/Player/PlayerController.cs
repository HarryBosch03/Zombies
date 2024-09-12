using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.SceneManagement;
using Zombies.Runtime.GameMeta;
using Zombies.Runtime.Interactive;
using Zombies.Runtime.Utility;

namespace Zombies.Runtime.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public float mouseSensitivity = 0.3f;
        public float controllerSensitivity = 0.3f;
        public float interactDistance = 2f;
        public TMP_Text lookingAtUI;
        public Vector2 lookingAtUIOffset;
        public Shaker lookingAtUIShaker;

        private Camera mainCamera;
        private InputActionMap input;
        private bool interact;

        public CharacterController character { get; private set; }
        public PlayerPoints points { get; private set; }
        public bool isActiveViewer => activeViewer == this;
        public bool isControlling { get; set; } = true;

        public static PlayerController activeViewer { get; set; }
        public static PlayerController localPlayer { get; set; }

        private void Awake()
        {
            mainCamera = Camera.main;

            input = InputSystem.actions.FindActionMap("Player");
            character = GetComponent<CharacterController>();
            points = GetComponent<PlayerPoints>();
        }

        private void OnEnable() { localPlayer = this; }

        private void Update()
        {
            var psHapticFeedback = DualShockGamepad.current as DualSenseGamepadHID;
            
            if (isControlling)
            {
                var moveInput = input.FindAction("Move").ReadValue<Vector2>();
                character.moveDirection = transform.TransformDirection(moveInput.x, 0f, moveInput.y);

                if (InputDown("Jump")) character.jump = true;
                if (InputDown("Hold Run")) character.run = true;
                if (InputUp("Hold Run")) character.run = false;
                if (InputUp("Toggle Run")) character.run = !character.run;

                if (InputDown("Primary Weapon")) character.SwitchWeapon(0);
                if (InputDown("Secondary Weapon")) character.SwitchWeapon(1);
                character.shoot = InputPressed("Shoot");
                character.aiming = InputPressed("Aim");
                if (InputDown("Reload")) character.reload = true;

                if (InputDown("Interact")) interact = true;
                
                var m = Mouse.current;
                var gp = Gamepad.current;
                Cursor.lockState = CursorLockMode.Locked;

                
                
                var lookDelta = Vector2.zero;
                if (m != null) lookDelta += m.delta.ReadValue() * mouseSensitivity;
                if (gp != null) lookDelta += gp.rightStick.ReadValue() * controllerSensitivity * 500f * Time.deltaTime;
                
                var tanLength = Mathf.Tan(character.currentFieldOfView * 0.5f * Mathf.Deg2Rad);
                character.rotation += lookDelta * tanLength;
            }
            else
            {
                character.moveDirection = Vector3.zero;
                character.jump = false;
                character.run = false;
                character.shoot = false;
                character.aiming = false;
                character.reload = false;

                Cursor.lockState = CursorLockMode.None;
            }

            var kb = Keyboard.current;
            if (kb.escapeKey.wasPressedThisFrame)
            {
                isControlling = !isControlling;
            }

            if (kb.leftShiftKey.isPressed && kb.leftCtrlKey.isPressed && kb.rKey.wasPressedThisFrame)
            {
                SceneManager.LoadScene(SceneManager.GetSceneAt(0).name);
            }

            lookingAtUIShaker.Update();
            lookingAtUI.rectTransform.anchoredPosition = lookingAtUIOffset + lookingAtUIShaker.offset;
        }

        private void FixedUpdate()
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
                    if (interact)
                    {
                        if (points.currentPoints >= cost && interactive.Interact(this))
                        {
                            points.Deduct(cost);
                        }
                        else
                        {
                            lookingAtUIShaker.Shake();
                        }
                    }
                }
            }

            interact = false;
        }

        private void OnValidate()
        {
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
    }
}