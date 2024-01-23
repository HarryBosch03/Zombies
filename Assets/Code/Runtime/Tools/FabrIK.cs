using System;
using UnityEngine;

namespace Framework.Runtime.Tools
{
    public static class FabrIK
    {
        public static Vector3[] Solve(Vector3[] points, Vector3[] hints, float[] lengths, int iterations)
        {
            if (points.Length != hints.Length) throw new Exception($"Hints has invalid size [is:{hints.Length} | should be: {points.Length}]");
            if (points.Length != lengths.Length + 1) throw new Exception($"Lengths has invalid size [is: {lengths.Length} | should be: {points.Length - 1}]");

            for (var i = 0; i < points.Length; i++)
            {
                points[i] = hints[i];
            }

            for (var i = 0; i < iterations; i++)
            {
                points[0] = hints[0];
                for (var j = 1; j < points.Length; j++)
                {
                    var a = points[j - 1];
                    var b = points[j];
                    var d = (b - a).normalized;

                    points[j] = a + d * lengths[j - 1];
                }
                
                points[^1] = hints[^1];
                for (var j = points.Length - 2; j > 0; j--)
                {
                    var a = points[j + 1];
                    var b = points[j];
                    var d = (b - a).normalized;

                    points[j] = a + d * lengths[j];
                }
            }

            return points;
        }
    }
}