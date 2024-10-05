using System.Collections.Generic;
using System.Linq;
using TMPro.EditorUtilities;
using UnityEngine;

namespace Gnome
{
    public class LeaderController : MonoBehaviour
    {
        public List<Vector2> FollowerPlace;
        public List<GnomeMovement> Followers;
        public float TriggerDistance;
        public float StopDistance;
        public float GatheringCircleRadius;

        private Vector2 gatheringPoint;

        public void FixedUpdate()
        {
            var leaderPosition = transform.position;
            var shouldFollow = Vector2.Distance(gatheringPoint, leaderPosition) > TriggerDistance;

            if (shouldFollow)
            {
                // foreach (var follower in Followers)
                // {
                //     follower.Destination = new GnomeMovement.Target
                //     {
                //         Position = leaderPosition,
                //         Radius = StopDistance,
                //     };
                // }

                gatheringPoint = Vector2.MoveTowards(leaderPosition, gatheringPoint, StopDistance);

                {
                    var newPositions = Enumerable.Range(0, Followers.Count)
                        .Select(x => gatheringPoint + Random.insideUnitCircle * GatheringCircleRadius)
                        .OrderBy(newPosition => Vector2.Distance(leaderPosition, newPosition));
                    var followers = Followers
                        .OrderBy(follower => Vector2.Distance(follower.Position, leaderPosition));

                    foreach (var (follower, newPosition) in followers.Zip(newPositions,
                                 (follower, newPosition) => (follower, newPosition)))
                    {
                        follower.Destination = new GnomeMovement.Target
                        {
                            Position = newPosition,
                            Radius = 0.1f,
                        };
                    }
                }
            }
        }
    }
}