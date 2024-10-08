using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using UnityEngine;
using Zombies.Runtime.Health;

namespace Zombies.Runtime.Player
{
    public class CharacterController : MonoBehaviour
    {
        public const int CollisionIterations = 4;

        [Header("MOVEMENT")]
        public float walkSpeed;
        public float runSpeed;
        public float accelerationTime;
        [Range(0f, 1f)]
        public float airAccelerationPenalty = 0.8f;

        [Space]
        [Header("JUMP")]
        public float jumpHeight;
        public float gravityScale = 2f;

        [Space]
        [Header("CAMERA")]
        public float baseFieldOfView = 90f;
        public float runFovModifier;
        public float fovDamping = 0.1f;
        public Transform head;
        public Vector3 headOffset = new Vector3(0f, 1.7f, 0f);
        public Vector2 movementDutch;
        public float movementDutchSmoothing;

        [Space]
        [Header("PHYSICS")]
        public LayerMask collisionMask;

        [Space]
        [Header("WEAPONS")]
        public PlayerWeapon activeWeapon;
        public PlayerWeapon[] equippedWeapons;
        public float recoilDecay;

        [Space]
        [Header("DAMAGE FX")]
        public float damageDutchAngle;
        public float damageDutchDuration;
        public AnimationCurve dutchCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        private RaycastHit groundHit;
        private PlayerWeapon[] allWeapons;

        private Camera mainCamera;
        private Vector2 recoilVelocity;
        private Vector2 currentMovementDutch;
        private bool shootLastFrame;
        private float dutchTimer;
        private bool forceHolstered;

        public static List<CharacterController> all = new();
        public static event Action<CharacterController, bool> ActiveViewerChanged;

        public Vector3 moveDirection { get; set; }
        public bool run { get; set; }
        public bool jump { get; set; }
        public bool shoot { get; set; }
        public bool reload { get; set; }
        public bool aiming { get; set; }
        public Vector2 rotation { get; set; }
        public Vector2 deltaRotation { get; set; }
        public MoveState moveState { get; set; }
        public List<Transform> cameraModifiers { get; } = new();

        public Vector3 velocity { get; set; }
        public bool onGround { get; private set; }

        public bool isActiveViewer { get; private set; }
        public float overrideFieldOfViewValue { get; set; }
        public float overrideFieldOfViewBlending { get; set; }
        public float currentFieldOfView { get; private set; }
        public HealthController health { get; private set; }

        public void SetIsActiveViewer(bool isActiveViewer)
        {
            this.isActiveViewer = isActiveViewer;

            if (isActiveViewer)
            {
                mainCamera.transform.SetParent(head);
                mainCamera.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }

            ActiveViewerChanged?.Invoke(this, isActiveViewer);
        }

        private void Awake()
        {
            mainCamera = Camera.main;

            health = GetComponent<HealthController>();

            allWeapons = GetComponentsInChildren<PlayerWeapon>(true);
            foreach (var weapon in allWeapons)
            {
                weapon.gameObject.SetActive(weapon == activeWeapon);
            }

            for (var i = 0; i < equippedWeapons.Length; i++)
            {
                if (equippedWeapons[i] != null)
                {
                    SwitchToWeapon(i);
                    break;
                }
            }
        }

        private void OnEnable()
        {
            all.Add(this);
            Cursor.lockState = CursorLockMode.Locked;

            HealthController.OnTakeDamage += TakeDamageEvent;
        }

        private void OnDisable()
        {
            all.Remove(this);
            Cursor.lockState = CursorLockMode.None;

            HealthController.OnTakeDamage -= TakeDamageEvent;
        }

        private void TakeDamageEvent(HealthController victim, HealthController.DamageReport report)
        {
            if (!transform.IsChildOf(victim.transform)) return;

            dutchTimer = damageDutchDuration;
        }

        private void Update()
        {
            ComputeFieldOfView();

            rotation += recoilVelocity * Time.fixedDeltaTime;
            rotation = new Vector2
            {
                x = rotation.x % 360f,
                y = Mathf.Clamp(rotation.y, -90f, 90f),
            };

            var finalRotation = rotation + deltaRotation;
            finalRotation = new Vector2
            {
                x = finalRotation.x % 360f,
                y = Mathf.Clamp(finalRotation.y, -90f, 90f),
            };

            transform.rotation = Quaternion.Euler(0f, finalRotation.x, 0f);
            head.localPosition = headOffset;

            var dutch = 0f;
            if (dutchTimer > 0f)
            {
                dutchTimer -= Time.fixedDeltaTime;
                dutch = dutchCurve.Evaluate(1f - dutchTimer / damageDutchDuration) * damageDutchAngle;
            }

            var targetMovementDutch = new Vector2
            {
                x = Vector3.Dot(transform.right, velocity),
                y = Vector3.Dot(transform.forward, velocity),
            } * movementDutch;
            currentMovementDutch = Vector2.Lerp(currentMovementDutch, targetMovementDutch, Time.deltaTime / Mathf.Max(Time.deltaTime, movementDutchSmoothing));

            dutch += currentMovementDutch.x;
            finalRotation.y += currentMovementDutch.y;

            foreach (var camMod in cameraModifiers)
            {
                finalRotation += new Vector2(camMod.localEulerAngles.y, -camMod.localEulerAngles.x);
            }

            head.rotation = Quaternion.Euler(-finalRotation.y, finalRotation.x, 0f) * Quaternion.Euler(0f, 0f, dutch);

            if (activeWeapon != null)
            {
                if (shoot && !shootLastFrame) activeWeapon.SetShoot(true);
                else if (!shoot && shootLastFrame) activeWeapon.SetShoot(false);
                if (reload) activeWeapon.Reload();
                activeWeapon.isAiming = aiming;
            }

            shootLastFrame = shoot;
            reload = false;
        }

        private void ComputeFieldOfView()
        {
            var targetFieldOfView = Mathf.Lerp(baseFieldOfView, overrideFieldOfViewValue, overrideFieldOfViewBlending);
            if (moveState == MoveState.Run) targetFieldOfView += targetFieldOfView * runFovModifier;

            currentFieldOfView = Mathf.Lerp(currentFieldOfView, targetFieldOfView, Time.fixedDeltaTime / fovDamping);
        }

        private void LateUpdate()
        {
            if (isActiveViewer)
            {
                mainCamera.fieldOfView = currentFieldOfView;
            }
        }

        public void Simulate()
        {
            if (isActiveAndEnabled)
            {
                moveDirection = Vector3.ClampMagnitude(new Vector3(moveDirection.x, 0f, moveDirection.z), 1f);

                UpdateMoveState();
                ApplyGravity();
                CheckForGround();
                Move();
                Jump();
                ApplyRecoil();

                for (var i = 0; i < CollisionIterations; i++)
                {
                    Iterate(CollisionIterations);
                    Collide();
                }
            }

            rotation += deltaRotation;
            deltaRotation = default;
        }

        private void UpdateMoveState()
        {
            if (Vector3.Dot(moveDirection, transform.forward) > 0.5f && run && (activeWeapon == null || !activeWeapon.isAiming)) moveState = MoveState.Run;
            else moveState = MoveState.Walk;
        }

        private void ApplyRecoil()
        {
            recoilVelocity -= recoilVelocity * recoilDecay * Time.fixedDeltaTime;
            rotation += recoilVelocity * Time.fixedDeltaTime;
        }

        public void AddRecoil(Vector2 dv) { recoilVelocity += dv * recoilDecay / 20f; }

        private void Iterate(int subFrameCount) { transform.position += velocity * Time.fixedDeltaTime / subFrameCount; }

        private void Collide()
        {
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var collider in colliders)
            {
                if (collider.isTrigger) continue;

                var broadPhase = Physics.OverlapBox(collider.bounds.center, collider.bounds.extents * 2f, Quaternion.identity, collisionMask);
                foreach (var other in broadPhase)
                {
                    if (other.isTrigger || other.transform.IsChildOf(transform)) continue;

                    if (Physics.ComputePenetration(collider, collider.transform.position, collider.transform.rotation, other, other.transform.position, other.transform.rotation, out var normal, out var depth))
                    {
                        transform.position += normal * depth;
                        velocity -= normal * Mathf.Min(Vector3.Dot(normal, velocity), 0f);
                    }
                }
            }
        }

        private void ApplyGravity() { velocity += Physics.gravity * gravityScale * Time.fixedDeltaTime; }

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

            var target = moveDirection * moveSpeed;
            var dv = (target - velocity) * 2f * Time.fixedDeltaTime / Mathf.Max(Time.fixedDeltaTime, accelerationTime);
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

        public void SwitchToWeapon(int index)
        {
            if (equippedWeapons[index] == null) return;
            if (activeWeapon == equippedWeapons[index]) return;

            if (forceHolstered)
            {
                activeWeapon = equippedWeapons[index];
            }
            else
            {
                if (activeWeapon) activeWeapon.gameObject.SetActive(false);
                activeWeapon = equippedWeapons[index];
                if (activeWeapon) activeWeapon.gameObject.SetActive(true);
            }
        }

        public void ForceWeaponHolstered(bool forceHolstered)
        {
            this.forceHolstered = forceHolstered;
            if (activeWeapon != null) activeWeapon.gameObject.SetActive(!forceHolstered);
        }

        public PlayerWeapon GetWeaponFromInventory(string name)
        {
            foreach (var weapon in equippedWeapons)
            {
                if (weapon != null && SimplifyWeaponName(weapon.name) == SimplifyWeaponName(name)) return weapon;
            }

            return null;
        }

        public void PickupWeapon(string name)
        {
            var index = getIndex();

            foreach (var weapon in allWeapons)
            {
                if (weapon != null && SimplifyWeaponName(weapon.name) == SimplifyWeaponName(name))
                {
                    equippedWeapons[index] = weapon;
                    SwitchToWeapon(index);
                }
            }

            int getIndex()
            {
                for (var i = 0; i < equippedWeapons.Length; i++)
                {
                    if (equippedWeapons[i] == null) return i;
                }

                for (var i = 0; i < equippedWeapons.Length; i++)
                {
                    if (equippedWeapons[i] == activeWeapon) return i;
                }

                return 0;
            }
        }

        private static string SimplifyWeaponName(string name) => name?.Trim().ToLower() ?? string.Empty;

        private void OnGUI()
        {
            if (isActiveViewer && Cursor.lockState != CursorLockMode.Locked)
            {
                GUI.Label(new Rect(0, Screen.height / 2f + 50, Screen.width, 18f), "CURSOR UNLOCKED");
            }
        }

        public enum MoveState
        {
            Walk,
            Run,
            Crouch
        }
    }
}