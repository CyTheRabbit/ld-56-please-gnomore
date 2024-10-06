using UnityEngine;

namespace Gnome
{
    public class GnomeMovement : MonoBehaviour
    {
        public struct Target
        {
            public Vector2 Position;
            public float Radius;
        }
        
        public float MovementSmoothTime;
        public float SpeedLimit;
        public Rigidbody2D Body;
        public GnomeAnimator Animator;

        public Target? Destination;

        public Vector2 Position => Body.position;

        public void FixedUpdate()
        {
            Move();
        }

        private void Move()
        {
            if (Destination is { } destination)
            {
                if (Vector2.Distance(Position, destination.Position) < destination.Radius)
                {
                    Destination = null;
                    return;
                }

                var velocity = Body.velocity;
                Vector2.SmoothDamp(Position, destination.Position, ref velocity, MovementSmoothTime, SpeedLimit);
                Body.velocity = velocity;
            }

            Animator.IsMoving = Destination != null;
        }
    }
}