using Framework.Runtime.Utility;
using UnityEngine;

namespace Framework.Runtime.Animation
{
    public class BipedalAnimator : MonoBehaviour
    {
        public float maxDistance = 0.5f;
        
        private Rigidbody body;
        private Foot[] feet = new Foot[2];
        private Leg[] legs = new Leg[2];
        private int footIndex;

        private void Awake()
        {
            body = GetComponentInParent<Rigidbody>();

            legs[0] = new Leg(transform.DeepFind("Leg.1.L"), 2);
            legs[1] = new Leg(transform.DeepFind("Leg.1.R"), 2);
        }

        private void FixedUpdate()
        {
            ref var foot = ref feet[footIndex];
            if ((foot.position - transform.position).magnitude > maxDistance)
            {
                foot.position = transform.position + body.velocity.normalized * maxDistance;
                foot.rotation = Quaternion.LookRotation(body.velocity.normalized, body.transform.up) * Quaternion.Euler(180f, 0f, 0f);
                footIndex = (footIndex + 1) % feet.Length;
            }

            for (var i = 0; i < legs.Length; i++)
            {
                legs[i].positions[1] = legs[i].segments[0].position + legs[i].segments[0].forward;
                legs[i].Solve(feet[i].position, feet[i].rotation);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;
            
            Gizmos.color = Color.magenta;
            for (var i = 0; i < feet.Length; i++)
            {
                var foot = feet[i];
                Gizmos.DrawWireSphere(foot.position, i == footIndex ? 0.1f : 0.05f);
            }
        }

        private struct Foot
        {
            public Vector3 localPosition;
            public Quaternion localRotation;
            public Transform anchor;

            public Vector3 position
            {
                get => anchor ? anchor.TransformPoint(localPosition) : localPosition;
                set => localPosition = anchor ? anchor.InverseTransformPoint(value) : value;
            }
            
            public Quaternion rotation
            {
                get => (anchor ? anchor.rotation * localRotation : localRotation).normalized;
                set => localRotation = (anchor ? value * Quaternion.Inverse(anchor.rotation) : value).normalized;
            }
        }

        public struct Leg
        {
            public Transform[] segments;
            public Vector3[] positions;
            public Quaternion[] rotations;
            public float[] lengths;

            public int count => positions.Length;
            public int chainLength => positions.Length + 1;
            
            public Leg(Transform root, int chainLength)
            {
                segments = new Transform[chainLength + 1];

                segments[0] = root;
                for (var i = 1; i < segments.Length; i++)
                {
                    segments[i] = segments[i - 1].GetChild(0);
                }
                
                positions = new Vector3[segments.Length];
                rotations = new Quaternion[segments.Length];
                for (var i = 0; i < positions.Length; i++)
                {
                    positions[i] = segments[i].position;
                    rotations[i] = segments[i].rotation;
                }

                rotations = new Quaternion[segments.Length];

                lengths = new float[segments.Length - 1];
                for (var i = 0; i < lengths.Length; i++)
                {
                    lengths[i] = (positions[i] - positions[i + 1]).magnitude;
                }
            }

            public void Solve(Vector3 end, Quaternion endRotation, int steps = 50)
            {
                if (chainLength < 1) return;
                
                var totalLength = 0.0f;
                for (var i = 0; i < lengths.Length; i++) totalLength += lengths[i];

                var start = segments[0].position;
                end = start + Vector3.ClampMagnitude(end - start, totalLength);
                
                positions[^1] = end;
                rotations[^1] = endRotation;

                for (var step = 0; step < steps; step++)
                {
                    positions[0] = start;
                    
                    for (var i = 0; i < positions.Length - 1; i++)
                    {
                        var a = positions[i];
                        ref var b = ref positions[i + 1];

                        b = a + (b - a).normalized * lengths[i];
                    }
                    
                    positions[^1] = end;
                    
                    for (var i = positions.Length - 1; i > 1; i--)
                    {
                        var a = positions[i];
                        ref var b = ref positions[i - 1];

                        b = a + (b - a).normalized * lengths[i - 1];
                    }
                }

                positions[0] = start;
                rotations[0] = Quaternion.LookRotation((positions[1] - positions[0]).normalized, rotations[0] * Vector3.up);
                    
                for (var i = 0; i < rotations.Length - 1; i++)
                {
                    var a = positions[i];
                    var b = positions[i + 1];

                    var d0 = (b - a).normalized;

                    rotations[i] = Quaternion.LookRotation(d0, rotations[i] * Vector3.up);
                }

                UpdateSegments();
            }

            private void UpdateSegments()
            {
                for (var i = 0; i < segments.Length; i++)
                {
                    segments[i].position = positions[i];
                    segments[i].rotation = rotations[i] * Quaternion.Euler(180f, 0f, 0f);
                }
            }
        }
    }
}