using System.Collections.Generic;
using UnityEngine;

namespace Gnome
{
    public static class RandomUtility
    {
        public static T RandomElement<T>(this T[] variants) => variants[Random.Range(0, variants.Length)];

        public static void Shuffle<T>(this IList<T> list)
        {
            for (var i = 0; i < list.Count - 1; i++)
            {
                var j = Random.Range(i + 1, list.Count);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}