using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Zombies.Runtime.Entities;

namespace Zombies.Runtime.Player
{
    public class PlayerController : MonoBehaviour
    {
        public float walkSpeed;
        public float runSpeed;
        public float accelerationTime;
        [Range(0f, 1f)]
        public float airAccelerationPenalty = 0.8f;

        [Space]
        public float jumpHeight;
        public float gravityScale = 2f;

        [Space]
        public float mouseSensitivity = 0.3f;
        public float fieldOfView = 90f;
        public float runFovModifier;
        public float fovDamping = 0.1f;

        [Space]
        public PlayerWeapon activeWeapon;
        public LayerMask collisionMask;

        [Space]
        public Transform head;
        public Vector3 headOffset = new Vector3(0f, 1.7f, 0f);
        public bool cameraInterpolation = true;

        [Space]
        public Vector3 velocity;
        public bool onGround;
        public RaycastHit groundHit;

        private InputActionMap input;
        private Vector3 lerpPosition0;
        private Vector3 lerpPosition1;

        private Camera mainCamera;
        private Vector2 rotation;
        private Vector2 recoilPosition;
        private Vector2 recoilVelocity;
        private float recoilSpring;
        private float recoilDamping;
        private bool jump;
        private MoveState moveState;
        
        public static List<PlayerController> all = new();
        public event Action<PlayerController, bool> ActiveViewerChanged;
        
        public bool isActiveViewer { get; private set; }
        public float overrideFieldOfViewValue { get; set; }
        public float overrideFieldOfViewBlending { get; set; }
        public float evaluatedFieldOfView { get; private set; }
        public HealthController health { get; private set; }

        public void SetActiveViewer(bool isActiveViewer)
        {
            this.isActiveViewer = isActiveViewer;
            ActiveViewerChanged?.Invoke(this, isActiveViewer);
        }

        private void Awake()
        {
            input = InputSystem.actions.FindActionMap("Player");
            mainCamera = Camera.main;

            health = GetComponent<HealthController>();

            foreach (var weapon in GetComponentsInChildren<PlayerWeapon>(true))
            {
                weapon.gameObject.SetActive(weapon == activeWeapon);
            }
        }

        private void OnEnable()
        {
            all.Add(this);
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDisable()
        {
            all.Remove(this);
            Cursor.lockState = CursorLockMode.None;
        }

        private void Update()
        {
            if (input.FindAction("Jump").WasPressedThisFrame()) jump = true;

            ComputeFieldOfView();

            var tanLength = Mathf.Tan(evaluatedFieldOfView * 0.5f * Mathf.Deg2Rad);
            var mouse = Mouse.current;
            var lookDelta = Vector2.zero;
            if (Cursor.lockState == CursorLockMode.Locked) lookDelta += mouse.delta.ReadValue() * mouseSensitivity * tanLength;
            rotation += lookDelta;
            rotation.x %= 360f;
            rotation.y = Mathf.Clamp(rotation.y, -90f, 90f);
            transform.rotation = Quaternion.Euler(0f, rotation.x, 0f);

            head.transform.position = cameraInterpolation ? Vector3.Lerp(lerpPosition1, lerpPosition0, (Time.time - Time.fixedTime) / Time.fixedDeltaTime) + headOffset : head.transform.position;
            
            var headRotation = rotation;
            headRotation += recoilPosition + recoilVelocity * (Time.time - Time.fixedTime);
            head.rotation = Quaternion.Euler(-headRotation.y, headRotation.x, 0f);

            if (activeWeapon != null)
            {
                if (InputDown("Shoot")) activeWeapon.SetShoot(true);
                else if (InputUp("Shoot")) activeWeapon.SetShoot(false);
                if (InputDown("Reload")) activeWeapon.Reload();
                activeWeapon.isAiming = InputPressed("Aim");
            }

            var kb = Keyboard.current;
            if (kb.leftShiftKey.isPressed && kb.rKey.wasPressedThisFrame)
            {
                SceneManager.LoadScene(SceneManager.GetSceneAt(0).name);
            }
        }

        private void ComputeFieldOfView()
        {
            var targetFieldOfView = Mathf.Lerp(fieldOfView, overrideFieldOfViewValue, overrideFieldOfViewBlending);
            if (moveState == MoveState.Run) targetFieldOfView += targetFieldOfView * runFovModifier; 
            
            evaluatedFieldOfView = Mathf.Lerp(evaluatedFieldOfView, targetFieldOfView, Time.deltaTime / fovDamping);
        }

        private void LateUpdate()
        {
            mainCamera.transform.position = head.position;
            mainCamera.transform.rotation = head.rotation;
            mainCamera.fieldOfView = evaluatedFieldOfView;
        }

        private void FixedUpdate()
        {
            UpdateMoveState();
            ApplyGravity();
            CheckForGround();
            Move();
            Jump();
            ApplyRecoil();

            Iterate();
            Collide();
            
            lerpPosition1 = lerpPosition0;
            lerpPosition0 = transform.position;
        }

        private void UpdateMoveState()
        {
            if (input.FindAction("Move").ReadValue<Vector2>().y > 0.5f && input.FindAction("Run").IsPressed() && !activeWeapon.isAiming) moveState = MoveState.Run;
            else moveState = MoveState.Walk;
        }

        private void ApplyRecoil()
        {
            var force = -recoilPosition * recoilSpring - recoilVelocity * recoilDamping;
            
            recoilPosition += recoilVelocity * Time.deltaTime;
            recoilVelocity += force * Time.deltaTime;
        }

        public void AddRecoil(Vector2 dv, float spring, float damping)
        {
            recoilSpring = spring;
            recoilDamping = damping;
            recoilVelocity += dv;
        }

        private void Iterate()
        {
            transform.position += velocity * Time.deltaTime;
        }

        private void Collide()
        {
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var collider in colliders)
            {
                var broadPhase = Physics.OverlapBox(collider.bounds.center, collider.bounds.extents, Quaternion.identity, collisionMask);
                foreach (var other in broadPhase)
                {
                    if (other.transform.IsChildOf(transform)) continue;
                    
                    if (Physics.ComputePenetration(collider, collider.transform.position, collider.transform.rotation, other, other.transform.position, other.transform.rotation, out var normal, out var depth))
                    {
                        transform.position += normal * depth;
                        velocity -= normal * Mathf.Min(Vector3.Dot(normal, velocity), 0f);
                    }
                }
            }
        }

        private void ApplyGravity()
        {
            velocity += Physics.gravity * gravityScale * Time.deltaTime;
        }

        private void CheckForGround()
        {
            var castDistance = 1f;
            var skinWidth = 0.1f;
            var ray = new Ray(transform.position + Vector3.up * castDistance, Vector3.down);
            onGround = Physics.Raycast(ray, out groundHit, castDistance + skinWidth, collisionMask);
            if (onGround)
            {
                transform.position = new Vector3(transform.position.x, groundHit.point.y, transform.position.z);
                velocity -= Vector3.up * Mathf.Min(0f, Vector3.Dot(Vector3.up, velocity));
            }
        }

        private void Move()
        {
            var moveSpeed = moveState switch
            {
                MoveState.Walk => walkSpeed,
                MoveState.Run => runSpeed,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            var input = this.input.FindAction("Move").ReadValue<Vector2>();
            var target = transform.TransformVector(input.x, 0f, input.y) * moveSpeed;
            var dv = (target - velocity) * 2f * Time.deltaTime / Mathf.Max(Time.deltaTime, accelerationTime);
            dv.y = 0f;
            if (!onGround) dv *= 1f - airAccelerationPenalty;

            velocity += dv;
        }

        private void Jump()
        {
            if (onGround && jump)
            {
                velocity = new Vector3(velocity.x, Mathf.Sqrt(2f * Physics.gravity.magnitude * gravityScale * jumpHeight), velocity.z);
            }
            
            jump = false;
        }

        private float InputAxis(string name) => input.FindAction(name).ReadValue<float>();
        private bool InputPressed(string name) => input.FindAction(name).IsPressed();
        private bool InputDown(string name) => input.FindAction(name).WasPressedThisFrame();
        private bool InputUp(string name) => input.FindAction(name).WasReleasedThisFrame();

        public enum MoveState
        {
            Walk,
            Run,
            Crouch
        }
    }
}