using System;
using UnityEngine;
using UnityEngine.Events;

namespace Gnome
{
    public class GnomeAttractor : MonoBehaviour
    {
        public UnityEvent<GnomeAgent> OnGnomeApproached;
        public UnityEvent<GnomeAgent> OnGnomeLeft;

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<GnomeAgent>() is { } gnome)
            {
                OnGnomeApproached?.Invoke(gnome);
            }
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<GnomeAgent>() is { } gnome)
            {
                OnGnomeLeft?.Invoke(gnome);
            }
        }
    }
}