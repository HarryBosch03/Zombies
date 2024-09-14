using System;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Zombies.Runtime.Entities
{
    public class BodyRotator : MonoBehaviour
    {
        public Transform actualBody;
        public Transform actualHead;
        public Transform visualBody;
        public Transform visualHead;
        public float bodyDeadzone = 20f;
        public float yawRange = 110f;
        public float pitchRange = 60f;

        private float visualBodyRotation;

        private void Update()
        {
            var actualBodyRotation = new Vector2(actualBody.eulerAngles.y, 0f);
            var actualHeadRotation = MathHelper.ToScreenRotation(actualHead.rotation);
            var visualHeadRotation = actualHeadRotation;

            visualHeadRotation.y = MathHelper.Remap(visualHeadRotation.y, -90f, 90f, -pitchRange, pitchRange);
            visualHeadRotation.x = MathHelper.Remap(Mathf.DeltaAngle(visualHeadRotation.x, actualBodyRotation.x), -180f, 180f, -yawRange, yawRange) + actualBodyRotation.x;

            var bodyRotationDelta = Mathf.DeltaAngle(visualBodyRotation, actualBodyRotation.x);
            if (Mathf.Abs(bodyRotationDelta) > bodyDeadzone) visualBodyRotation = actualBodyRotation.x - bodyDeadzone * Mathf.Sign(bodyRotationDelta);
            
            visualBody.rotation = Quaternion.Euler(0f, this.visualBodyRotation, 0f);
            visualHead.rotation = MathHelper.FromScreenRotation(visualHeadRotation);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (actualBody != null && visualHead != null)
            {
                drawArc(actualBody.up, yawRange, Color.red, 0.5f);
                drawArc(actualBody.right, pitchRange, Color.green, 0.6f);

                void drawArc(Vector3 normal, float range, Color color, float radius)
                {
                    var from = Quaternion.Euler(normal * -range) * actualBody.forward;

                    Handles.color = color;
                    Handles.DrawWireArc(visualHead.position, normal, from, range * 2f, radius);
                    Handles.color = color.MulAlpha(0.1f);
                    Handles.DrawSolidArc(visualHead.position, normal, from, range * 2f, radius);
                }
            }
        }
#endif
    }
}