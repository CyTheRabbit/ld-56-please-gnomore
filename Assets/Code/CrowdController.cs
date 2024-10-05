using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gnome
{
    public class CrowdController
    {
        public GnomeMovement Leader;
        public List<GnomeMovement> Followers;

        public Vector2 GatheringPoint;
        public float GatheringCircleRadius;
        public float FollowTriggerDistance;
        public float StopDistance;

        public void FixedUpdate()
        {
            if (Vector2.Distance(Leader.Position, GatheringPoint) > FollowTriggerDistance)
            {
                var newPositions = Enumerable.Range(0, Followers.Count)
                    .Select(x => GatheringPoint + Random.insideUnitCircle * GatheringCircleRadius)
                    .OrderBy(newPosition => Vector2.Distance(Leader.Position, newPosition));
                var followers = Followers
                    .OrderBy(follower => Vector2.Distance(follower.Position, Leader.Position));

                foreach (var (follower, newPosition) in followers.Zip(newPositions, (follower, newPosition) => (follower, newPosition)))
                {
                    follower.Destination = new GnomeMovement.Target
                    {
                        Position = newPosition,
                        Radius = StopDistance,
                    };
                }
            }
        }
    }
}