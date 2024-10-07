using System;
using UnityEngine;

namespace Gnome
{
    public class DesirableItem : MonoBehaviour
    {
        public string Kind;
        public int JoyPriority;
        public float HappyCircle;

        public Vector2 Position => transform.position;

        public void CharmGnome(GnomeAgent gnome)
        {
            if ((gnome.Behaviour?.Priority ?? -100) >= JoyPriority) return;
            if (!gnome.Desire.Equals(Kind, StringComparison.InvariantCultureIgnoreCase)) return;
            gnome.SetBehaviour(new GnomeJoyBehaviour(gnome, JoyPriority, this));
        }
    }
}