using UnityEngine;

namespace Gnome
{
    public static class RandomUtility
    {
        public static T RandomElement<T>(this T[] variants) => variants[Random.Range(0, variants.Length)];
    }
}