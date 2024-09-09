using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Zombies.Runtime.GameMeta;
using Zombies.Runtime.Utility;

namespace Zombies.Runtime.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public float mouseSensitivity = 0.3f;
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
            if (isControlling)
            {
                var moveInput = input.FindAction("Move").ReadValue<Vector2>();
                character.moveDirection = transform.TransformDirection(moveInput.x, 0f, moveInput.y);

                if (InputDown("Jump")) character.jump = true;
                character.run = InputPressed("Run");

                if (InputDown("Primary Weapon")) character.SwitchWeapon(0);
                if (InputDown("Secondary Weapon")) character.SwitchWeapon(1);
                character.shoot = InputPressed("Shoot");
                character.aiming = InputPressed("Aim");
                if (InputDown("Reload")) character.reload = true;

                if (InputDown("Interact")) interact = true;
                
                var m = Mouse.current;
                Cursor.lockState = CursorLockMode.Locked;

                var tanLength = Mathf.Tan(character.currentFieldOfView * 0.5f * Mathf.Deg2Rad);
                var lookDelta = Vector2.zero;
                lookDelta += m.delta.ReadValue() * mouseSensitivity * tanLength;
                character.rotation += lookDelta;
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
                var purchasable = hit.collider.GetComponentInParent<Purchasable>();
                if (purchasable != null && purchasable.isActiveAndEnabled)
                {
                    lookingAtUI.text = purchasable.display;
                    if (interact)
                    {
                        if (points.currentPoints >= purchasable.cost)
                        {
                            purchasable.Purchase();
                            points.Deduct(purchasable.cost);
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