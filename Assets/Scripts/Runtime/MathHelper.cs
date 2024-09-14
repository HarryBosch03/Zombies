using UnityEngine;

namespace Zombies.Runtime
{
    public static class MathHelper
    {
        public static Vector2 ToScreenRotation(Quaternion worldRotation)
        {
            var result = new Vector2(worldRotation.eulerAngles.y, -worldRotation.eulerAngles.x);
            if (result.x > 180f) result.x -= 360f;
            if (result.x < -180f) result.x += 360f;
            
            if (result.y > 180f) result.y -= 360f;
            if (result.y < -180f) result.y += 360f;
            return result;
        }

        public static Quaternion FromScreenRotation(Vector2 rotation) => Quaternion.Euler(-rotation.y, rotation.x, 0f);

        public static float Remap(float x, float fromMin, float fromMax, float toMin, float toMax)
        {
            var xn = (x - fromMin) / (fromMax - fromMin);
            return xn * (toMax - toMin) + toMin;
        }
        
        public static float RemapClamped(float x, float fromMin, float fromMax, float toMin, float toMax)
        {
            var value = Remap(x, fromMin, fromMax, toMin, toMax);
            if (value > toMax) value = toMax;
            if (value < toMin) value = toMin;
            return value;
        }
    }
}