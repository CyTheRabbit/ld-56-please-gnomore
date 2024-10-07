using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gnome
{
    public class EmotionPerception : MonoBehaviour
    {
        public float EmotionFrequency = 12f;
        public float LonelinessFrequency = 6f;

        private readonly List<GnomeAgent> gnomes = new();

        public void Start()
        {
            StartCoroutine(EmotionsActivationRoutine());
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<GnomeAgent>() is { } gnome)
            {
                gnomes.Add(gnome);
            }
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<GnomeAgent>() is { } gnome)
            {
                gnomes.Remove(gnome);
            }
        }

        private IEnumerator EmotionsActivationRoutine()
        {
            while (this != null)
            {
                yield return new WaitForSeconds(EmotionFrequency * Random.value + LonelinessFrequency / Mathf.Max(gnomes.Count, 1));

                var randomGnome = gnomes[Random.Range(0, gnomes.Count)];
                if (randomGnome.Behaviour is GnomeFollowBehaviour)
                {
                    randomGnome.Wish();
                }
            }
        }
    }
}