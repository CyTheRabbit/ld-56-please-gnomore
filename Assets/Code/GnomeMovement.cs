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

        public Target? Destination;
        private Vector2 velocity;

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

                Vector2.SmoothDamp(Position, destination.Position, ref velocity, MovementSmoothTime, SpeedLimit);
                Body.velocity = velocity;
            }

            // var startPosition = (Vector2) transform.position;
            // var currentPosition = startPosition;
            // var desiredPosition = Vector2.SmoothDamp(currentPosition, Destination, ref velocity, MovementSmoothTime, SpeedLimit);
            // var totalTravel = Vector2.Distance(desiredPosition, currentPosition);
            //
            // if (totalTravel < 0.01f) return;
            //
            // var originalDirection = (desiredPosition - currentPosition).normalized;
            // var direction = originalDirection;
            // var travel = totalTravel;
            // const int stepLimit = 4;
            // var stepsMade = 0;
            // while (travel > 0.01f && stepsMade < stepLimit)
            // {
            //     
            //     var contact = Physics2D.CircleCast(currentPosition, Shape.radius, direction, travel);
            //     if (contact.collider == null)
            //     {
            //         currentPosition += direction * travel;
            //         travel = 0;
            //     }
            //     else
            //     {
            //         var firstContact = raycastResults[0];
            //         var stepDistance = Mathf.Max(firstContact.distance - 0.01f, 0);
            //         currentPosition += direction * stepDistance;
            //         travel -= stepDistance;
            //
            //         var newDirection = Vector2.Perpendicular(firstContact.normal);
            //         if (Vector2.Dot(newDirection, direction) < 0)
            //         {
            //             newDirection *= -1;
            //         }
            //         travel *= Vector2.Dot(direction, newDirection);
            //         direction = newDirection;
            //     }
            //
            //     stepsMade++;
            //
            //     if (Vector2.Dot(direction, originalDirection) < 0)
            //     {
            //         break;
            //     }
            // }
            //
            // transform.position = currentPosition;
            //
            // if (Vector2.Distance(startPosition, currentPosition) < FrictionToStop)
            // {
            //     Destination = currentPosition;
            // }
        }
    }
}