using FishNet.Object;
using UnityEngine;
using UnityEngine.Serialization;
using Framework.Runtime.Utility;

namespace Framework.Runtime.Core
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public class PlayerMovement : NetworkBehaviour
    {
        public bool isOwner;

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

        [HideInInspector] public Transform view;
        [HideInInspector] public Vector2 viewRotation;
        [HideInInspector] public Vector2 viewFrameOffset;
        [HideInInspector] public Rigidbody body;

        private float height;
        private RaycastHit groundHit;
        private Vector3 lastPosition;

        public bool IsOnGround { get; private set; }
        public bool Running { get; set; }

        public float Movement
        {
            get
            {
                var v = body.velocity;
                return Mathf.Sqrt(v.x * v.x + v.z * v.z) / moveSpeed;
            }
        }

        private Vector3 Gravity => Physics.gravity * (body.velocity.y > 0.0f ? upGravity : downGravity);
        public Vector3 Center => view.position;
        public Vector3 Velocity => body.velocity;
        public float GroundSpeed => new Vector2(Velocity.x, Velocity.z).magnitude;
        public float NormalizedGroundSpeed => GroundSpeed / moveSpeed;

        private void Awake()
        {
            body = gameObject.GetOrAddComponent<Rigidbody>();
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.interpolation = RigidbodyInterpolation.None;
            body.constraints = RigidbodyConstraints.FreezeRotation;

            view = transform.Find("View");

            body.constraints = RigidbodyConstraints.FreezeRotation;
        }

        private void FixedUpdate()
        {
            isOwner = IsOwner;

            if (IsOwner)
            {
                var networkState = new NetworkState();

                networkState.position = body.position;
                networkState.velocity = body.velocity;
                networkState.rotation = viewRotation;
                
                networkState.moveInput = moveInput;
                networkState.jump = jump;

                RpcSendNetworkStateToServer(networkState);
            }

            LookForGround();
            Move();
            Jump();
            ApplyGravity();

            lastPosition = transform.position;
        }

        [ServerRpc]
        private void RpcSendNetworkStateToServer(NetworkState networkState)
        {
            ApplyNetworkState(networkState);
            RpcSendNetworkStateToObservers(networkState);
        }
        
        [ObserversRpc]
        private void RpcSendNetworkStateToObservers(NetworkState networkState) => ApplyNetworkState(networkState);

        private void ApplyNetworkState(NetworkState networkState)
        {
            if (IsOwner) return;

            body.position = reconcileV3(body.position, networkState.position, 1.0f);
            body.velocity = reconcileV3(body.velocity, networkState.velocity, 1.0f);
            viewRotation = networkState.rotation;
            moveInput = networkState.moveInput;
            jump = networkState.jump;

            Vector3 reconcileV3(Vector3 localValue, Vector3 netValue, float threshold) => (localValue - netValue).magnitude > threshold ? netValue : localValue;
        }

        private void ApplyGravity()
        {
            if (!body.useGravity) return;
            body.AddForce(Gravity - Physics.gravity, ForceMode.Acceleration);
        }

        private void Jump()
        {
            if (!IsOnGround) return;
            if (!jump) return;

            var force = Mathf.Sqrt(2.0f * -Physics.gravity.y * upGravity * jumpHeight);
            body.AddForce(Vector3.up * force, ForceMode.VelocityChange);
        }

        private void LookForGround()
        {
            var wasOnGround = IsOnGround;
            var ray = new Ray(body.position + Vector3.up, Vector3.down);
            var castDistance = wasOnGround ? 1.0f + stepHeight : 1.0f;
            IsOnGround = Physics.Raycast(ray, out groundHit, castDistance) && body.velocity.y < float.Epsilon;

            if (!IsOnGround) height = body.position.y;
            else height = Mathf.Lerp(height, groundHit.point.y, heightSmoothing);

            if (!IsOnGround) return;

            body.position = new Vector3(body.position.x, height, body.position.z);
            body.velocity = new Vector3(body.velocity.x, Mathf.Max(body.velocity.y, 0.0f), body.velocity.z);

            if (groundHit.rigidbody) body.position += groundHit.rigidbody.velocity * Time.deltaTime;
        }

        private void Move()
        {
            var target = Vector3.ClampMagnitude(moveInput, 1.0f) * moveSpeed;

            var acceleration = 2.0f / accelerationTime;
            if (!IsOnGround) acceleration *= 1.0f - airborneAccelerationPenalty;
            var force = (target - body.velocity) * acceleration;
            force.y = 0.0f;

            body.AddForce(force, ForceMode.Acceleration);
        }

        private void Update()
        {
            viewRotation.x %= 360.0f;
            viewRotation.y = Mathf.Clamp(viewRotation.y, -90.0f, 90.0f);

            var final = viewRotation + viewFrameOffset;
            viewFrameOffset = Vector2.zero;
            
            body.rotation = Quaternion.Euler(0.0f, final.x, 0.0f);

            view.position = Vector3.Lerp(lastPosition, transform.position, (Time.time - Time.fixedTime) / Time.fixedDeltaTime) + Vector3.up * 1.8f;
            view.rotation = Quaternion.Euler(-final.y, final.x, 0.0f);
        }
        
        [System.Serializable]
        public struct NetworkState
        {
            public Vector3 position;
            public Vector3 velocity;
            public Vector2 rotation;

            public Vector3 moveInput;
            [FormerlySerializedAs("jumpFlag")] public bool jump;
        }
    }
}