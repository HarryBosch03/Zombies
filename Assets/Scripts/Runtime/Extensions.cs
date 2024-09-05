using UnityEngine;

namespace Zombies.Runtime
{
    public static class Extensions
    {
        public static Pose Pose(this Transform transform)
        {
            return new Pose(transform.position, transform.rotation);
        }
    }
}