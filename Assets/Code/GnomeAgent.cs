using UnityEngine;

namespace Gnome
{
    public class GnomeAgent : MonoBehaviour
    {
        public interface IBehaviour
        {
            int Priority { get; }
            void Start();
            void End();
        }

        private GnomeMovement movement;
        private GnomeAnimator animator;
        private IBehaviour behaviour;

        public Crowd Crowd;

        public Vector2 Position => transform.position;

        public IBehaviour Behaviour => behaviour;

        public GnomeMovement.Target? Destination
        {
            get => movement.Destination;
            set => movement.Destination = value;
        }

        public void Awake()
        {
            movement = GetComponent<GnomeMovement>();
            animator = GetComponent<GnomeAnimator>();
        }

        public void SetBehaviour(IBehaviour newBehaviour)
        {
            behaviour?.End();
            behaviour = newBehaviour;
            behaviour?.Start();
        }
    }
}