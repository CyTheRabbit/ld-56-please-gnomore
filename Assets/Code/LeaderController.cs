using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gnome
{
    public class LeaderController : MonoBehaviour
    {
        public GnomeMovement Leader;
        public List<GnomeMovement> Followers;
        [Range(0, 1)]
        public float Overtaking = 0.25f;
        public float AvoidanceDistance = 3;

        private IEnumerable<GnomeMovement> followersByDistance;

        private void Awake()
        {
            followersByDistance =
                Followers.OrderBy(follower => Vector2.Distance(Leader.Position, follower.Position));
        }

        public void FixedUpdate()
        {
            if (Leader.Destination is not { } destination)
            {
                return;
            }

            var toLeader = Vector2.Lerp(Leader.Position, destination.Position, Overtaking);
            var leaderRay = new Ray2D(Leader.Position, destination.Position - Leader.Position);
            var i = 1;
            foreach (var follower in followersByDistance)
            {
                i++;
                var radius = i * 0.15f;

                var toFollower = follower.Position - Leader.Position;
                var avoidanceFactor = Mathf.Clamp01(Vector2.Dot(toFollower.normalized, leaderRay.direction) - 0.3f);
                var avoidSide = Cross(leaderRay.direction, toFollower) > 0 ? 1 : -1;
                var avoid = Leader.Position + Vector2.Perpendicular(leaderRay.direction) * avoidSide * AvoidanceDistance;
                var target = Vector2.Lerp(toLeader, avoid, avoidanceFactor);

                Debug.DrawLine(follower.Position, toLeader, Color.blue);
                Debug.DrawLine(follower.Position, avoid, Color.green);
                Debug.DrawLine(follower.Position, target, Color.yellow);

                follower.Destination = new GnomeMovement.Target
                    {
                        Position = target,
                        Radius = radius,
                    };

            }
        }

        private float Cross(Vector3 a, Vector3 b)
        {
            return a.x * b.y - a.y * b.x;
        }
    }
}