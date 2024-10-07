using UnityEngine;

namespace Gnome
{
    public class StaticCrowd : MonoBehaviour
    {
        public int Priority = -5;

        public void Start()
        {
            var crowd = new Crowd(Priority);
            var gnomes = GetComponentsInChildren<GnomeAgent>();
            foreach (var gnome in gnomes)
            {
                if ((gnome.Behaviour?.Priority ?? -100) >= Priority) continue;
                gnome.SetBehaviour(new GnomeFollowBehaviour(gnome, crowd));
            }
        }
    }
}