using UnityEngine;

namespace Framework.Runtime.Utility
{
    public static class RandomUtils
    {
        public static float MedianVariance(float median, float variance)
        {
            return Random.Range(median - variance * 0.5f, median + variance * 0.5f);
        }
    }
}