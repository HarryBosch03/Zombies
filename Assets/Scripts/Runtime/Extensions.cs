using UnityEngine;

namespace Zombies.Runtime
{
    public static class Extensions
    {
        public static Pose Pose(this Transform transform)
        {
            return new Pose(transform.position, transform.rotation);
        }

        public static Color SetAlpha(this Color c, float a) => new Color(c.r, c.g, c.b, a);
        public static Color MulAlpha(this Color c, float a) => c.SetAlpha(c.a * a);
    }
}