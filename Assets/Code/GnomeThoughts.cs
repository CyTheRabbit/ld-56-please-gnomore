using UnityEngine;

namespace Gnome
{
    internal class GnomeThoughts : MonoBehaviour
    {
        public SpriteRenderer ThoughtIcon;
        public Animator Animator;

        private static readonly int WishHash = Animator.StringToHash("Wish");

        public void Wish()
        {
            Animator.SetTrigger(WishHash);
        }
    }
}