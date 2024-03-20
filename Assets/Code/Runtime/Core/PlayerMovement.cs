using UnityEngine;
using Framework.Runtime.Utility;

namespace Framework.Runtime.Core
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public class PlayerMovement : MonoBehaviour
    {
        #region Properties

        public float moveSpeed = 12.0f;
        public float accelerationTime = 0.1f;
        public float jumpHeight = 2.2f;

        [Range(0.0f, 1.0f)]
        public float airborneAccelerationPenalty = 0.6f;
        public float upGravity = 2.0f;
        public float downGravity = 3.0f;
        public float stepHeight = 0.6f;

        [Range(0.0f, 1.0f)]
        public float heightSmoothing = 0.4f;

        [Space]
        public float viewKinematicsDrag = 100.0f;

        #endregion

        [HideInInspector] public Vector3 moveInput;
        [HideInInspector] public bool jump;

        public Transform view {get; set;}
        public Vector2 viewRotation {get; set;}
        public Vector2 viewFrameOffset {get; set;}
        public Rigidbody body {get; set;}

        private float height;
        private RaycastHit groundHit;
        private Vector3 lastPosition;

        public bool isOnGround { get; private set; }
        public bool running { get; set; }

        public float movement
        {
            get
            {
                var v = body.velocity;
                return Mathf.Sqrt(v.x * v.x + v.z * v.z) / moveSpeed;
            }
        }

        private Vector3 gravity => Physics.gravity * (body.velocity.y > 0.0f ? upGravity : downGravity);
        public Vector3 center => view.position;
        public Vector3 velocity => body.velocity;
        public float groundSpeed => new Vector2(velocity.x, velocity.z).magnitude;
        public float normalizedGroundSpeed => groundSpeed / moveSpeed;

        private void Awake()
        {
            body = gameObject.GetOrAddComponent<Rigidbody>();
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.interpolation = RigidbodyInterpolation.None;
            body.constraints = RigidbodyConstraints.FreezeRotation;

            view = transform.Get("View");

            body.constraints = RigidbodyConstraints.FreezeRotation;
        }

        private void FixedUpdate()
        {
            LookForGround();
            Move();
            Jump();
            ApplyGravity();

            lastPosition = transform.position;
        }

        private void ApplyGravity()
        {
            if (!body.useGravity) return;
            body.AddForce(gravity - Physics.gravity, ForceMode.Acceleration);
        }

        private void Jump()
        {
            if (!isOnGround) return;
            if (!jump) return;

            var force = Mathf.Sqrt(2.0f * -Physics.gravity.y * upGravity * jumpHeight);
            body.AddForce(Vector3.up * force, ForceMode.VelocityChange);
        }

        private void LookForGround()
        {
            var wasOnGround = isOnGround;
            var ray = new Ray(body.position + Vector3.up, Vector3.down);
            var castDistance = wasOnGround ? 1.0f + stepHeight : 1.0f;
            isOnGround = Physics.Raycast(ray, out groundHit, castDistance) && body.velocity.y < float.Epsilon;

            if (!isOnGround) height = body.position.y;
            else height = Mathf.Lerp(height, groundHit.point.y, heightSmoothing);

            if (!isOnGround) return;

            body.position = new Vector3(body.position.x, height, body.position.z);
            body.velocity = new Vector3(body.velocity.x, Mathf.Max(body.velocity.y, 0.0f), body.velocity.z);

            if (groundHit.rigidbody) body.position += groundHit.rigidbody.velocity * Time.deltaTime;
        }

        private void Move()
        {
            var target = Vector3.ClampMagnitude(moveInput, 1.0f) * moveSpeed;

            var acceleration = 2.0f / accelerationTime;
            if (!isOnGround) acceleration *= 1.0f - airborneAccelerationPenalty;
            var force = (target - body.velocity) * acceleration;
            force.y = 0.0f;

            body.AddForce(force, ForceMode.Acceleration);
        }

        private void Update()
        {
            viewRotation = new Vector2
            {
                x = viewRotation.x % 360.0f,
                y = Mathf.Clamp(viewRotation.y, -90f, 90f)
            };
            
            var final = viewRotation + viewFrameOffset;
            viewFrameOffset = Vector2.zero;

            body.rotation = Quaternion.Euler(0.0f, final.x, 0.0f);

            view.position = Vector3.Lerp(lastPosition, transform.position, (Time.time - Time.fixedTime) / Time.fixedDeltaTime) + Vector3.up * 1.6f;
            view.rotation = Quaternion.Euler(-final.y, final.x, 0.0f);
        }
    }
}