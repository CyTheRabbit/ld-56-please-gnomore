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

            void OnPerish() { }
        }

        public string Desire;

        private GnomeMovement movement;
        private GnomeAnimator animator;
        private IBehaviour behaviour;
        private new CircleCollider2D collider;
        private GnomeVoice voice;

        public Crowd Crowd;

        public Vector2 Position => transform.position;

        public float Radius => collider.radius;

        public IBehaviour Behaviour => behaviour;

        public GnomeMovement.Target? Destination
        {
            get => movement.Destination;
            set
            {
                movement.Destination = value;
                voice.IsWalking = value is { } destination
                                  && Vector2.Distance(Position, destination.Position) >= destination.Radius;
            }
        }

        public bool WalksSilently
        {
            get => voice.WalksSilently;
            set => voice.WalksSilently = value;
        }

        public void Awake()
        {
            movement = GetComponent<GnomeMovement>();
            animator = GetComponent<GnomeAnimator>();
            voice = GetComponent<GnomeVoice>();
        }

        public void OnEnable()
        {
            behaviour?.Start();
        }

        public void OnDisable()
        {
            behaviour?.End();
        }

        public void SetBehaviour(IBehaviour newBehaviour)
        {
            behaviour?.End();
            behaviour = newBehaviour;
            behaviour?.Start();
        }

        public void Punch(IPunchable victim)
        {
            var hitDirection = (victim.Position - Position).normalized;
            victim.TakeHit(hitDirection);
            voice.Punch();
        }

        public void Bark()
        {
            voice.Bark();
            animator.Jump(0.66f);
        }

        public void Perish()
        {
            behaviour?.OnPerish();
            voice.Perish();
            Destroy(gameObject);
        }

        public void Jump()
        {
            animator.Jump(0.66f);
        }
    }
}