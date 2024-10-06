using UnityEngine;

namespace Gnome
{
    public class AvatarController : MonoBehaviour
    {
        public LeaderController Leader;

        public void Start()
        {
            var gnome = GetComponent<GnomeAgent>();
            gnome.SetBehaviour(new GnomePlayerBehaviour(gnome, Leader, Camera.main));
        }
    }
}

