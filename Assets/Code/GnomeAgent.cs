using System;
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
        private new CircleCollider2D collider;

        public Crowd Crowd;

        public Vector2 Position => transform.position;

        public float Radius => collider.radius;

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
    }
}