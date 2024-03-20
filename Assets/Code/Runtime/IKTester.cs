using System;
using Framework.Runtime.Animation;
using UnityEngine;

namespace Framework.Runtime
{
    public class IKTester : MonoBehaviour
    {
        public float length = 1.0f;
        public int steps = 50;
        
        private void OnDrawGizmos()
        {
            var root = transform.GetChild(0);
            var chainLength = 0;
            var next = root;
            while (next.childCount > 0)
            {
                next = next.GetChild(0);
                chainLength++;
            }
            
            var target = transform.GetChild(1);

            root.transform.position = transform.position;
            
            var leg = new BipedalAnimator.Leg(root, chainLength);

            for (var i = 0; i < leg.lengths.Length; i++)
            {
                leg.lengths[i] = length;
            }

            leg.Solve(target.position, target.rotation, steps);
            
            for (var i = 0; i < leg.segments.Length; i++)
            {
                var segment = leg.segments[i];
                var color = new Color(0f, 0f, 0f, 1f);
                color[i % 3] = 1f;
                Gizmos.color = color;
                DrawTriangle(segment.position, segment.rotation, 0.25f * length);
            }

            Gizmos.color = Color.yellow;
            for (var i = 0; i < leg.segments.Length - 1; i++)
            {
                Gizmos.DrawLine(leg.segments[i].position, leg.segments[i + 1].position);
            }
        }

        private void DrawTriangle(Vector3 position, Quaternion rotation, float size)
        {
            var a = new Vector3(0f, 0f, 0f);
            var b = new Vector3(0f, 0f, 1f);
            var c = new Vector3(0f, 1f, 0f);

            a = position + rotation * a * size;
            b = position + rotation * b * size;
            c = position + rotation * c * size;
            
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, a);
        }
    }
}