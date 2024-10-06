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
        public float Acceleration = 2;
        public float WalkDrag = 30;
        public float StopDrag = 100;
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
                var toDestination = destination.Position - Position;
                if (toDestination.magnitude < destination.Radius)
                {
                    Body.drag = StopDrag;
                    Destination = null;
                }
                else
                {
                    var target = toDestination.normalized * SpeedLimit;
                    Body.velocity = Vector2.MoveTowards(Body.velocity, target, Time.deltaTime * Acceleration);
                    Body.drag = WalkDrag;
                }
            }

            Animator.IsMoving = Destination != null;
        }
    }
}