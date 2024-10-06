using System;
using System.Linq;
using UnityEngine;

namespace Gnome
{
    public class LeaderController : MonoBehaviour
    {
        private const float DefaultDestinationRadius = 0.1f;

        public GnomeAgent Leader;
        public float InitialCrowdRadius = 5;
        public float JoinRadius = 3;
        public int FollowPriority = 0;
        [Range(0, 1)]
        public float Overtaking = 0.25f;
        public float AvoidanceDistance = 3;
        public float SocialDistance = 2;
        public float GrumpyDistance = 3;
        public float GrumpyHeatUp = 3;
        public float GrumpyCoolDown = 3;
        [Space]
        public float FollowDistanceFromLeader = 1f;
        public float CrowdSpread = 0.15f;

        public bool IsBarking;

        private float grumpiness;
        private float grumpinessVelocity;

        private Collider2D[] neighbours = new Collider2D[128];

        public void Start()
        {
            var crowd = Leader.Crowd = new Crowd();

            InviteNeighbours(InitialCrowdRadius);
        }

        public void FixedUpdate()
        {
            var leaderPosition = Leader.Position;
            var destination = Leader.Destination ?? new GnomeMovement.Target
            {
                Position = leaderPosition,
                Radius = DefaultDestinationRadius,
            };
            var leaderDirection = (destination.Position - leaderPosition).normalized;

            InviteNeighbours(JoinRadius);

            Leader.Crowd.Center = leaderPosition;
            using (var nativeOrders = Leader.Crowd.BeginOrders())
            {
                var orders = nativeOrders.AsSpan();

                ApplyLeaderFollowing(destination, leaderPosition, orders);
                ApplyLeaderAvoidance(orders, leaderPosition, leaderDirection);
                ApplySocialDistance(orders, leaderPosition);
                ApplyBarking(orders, leaderPosition);

                Leader.Crowd.RunOrders(orders);
            }
        }

        private void InviteNeighbours(float radius)
        {
            var neighboursCount = Physics2D.OverlapCircleNonAlloc(Leader.Position, radius, neighbours);
            for (var i = 0; i < neighboursCount; i++)
            {
                var neighbour = neighbours[i];
                if (neighbour.GetComponent<GnomeAgent>() is { } gnome)
                {
                    TryInvite(gnome);
                }
            }
        }

        private void TryInvite(GnomeAgent gnome)
        {
            if (gnome == Leader) return;
            if (gnome.Crowd == Leader.Crowd) return;
            if ((gnome.Behaviour?.Priority ?? -100) >= FollowPriority) return;
            gnome.SetBehaviour(new GnomeFollowBehaviour(gnome, Leader.Crowd));
        }

        private void ApplySocialDistance(Span<Crowd.Order> orders, Vector2 leaderPosition)
        {
            for (var i = 0; i < orders.Length; i++)
            {
                ref var order = ref orders[i];
                var fromLeader = order.CurrentPosition - leaderPosition;
                var target = order.CurrentPosition + fromLeader.normalized * SocialDistance;
                var factor = Mathf.Clamp01(1 - fromLeader.magnitude);
                
                order.Target = new GnomeMovement.Target
                {
                    Position = Vector2.Lerp(order.Target.Position, target, factor),
                    Radius = Mathf.Lerp(order.Target.Radius, DefaultDestinationRadius, factor),
                };
            }
        }

        private void ApplyLeaderAvoidance(Span<Crowd.Order> orders, Vector2 leaderPosition, Vector2 leaderDirection)
        {
            for (var i = 0; i < orders.Length; i++)
            {
                ref var order = ref orders[i];
                var toFollower = order.CurrentPosition - leaderPosition;
                var distanceInverse = Mathf.Clamp01(0.3f / toFollower.magnitude);
                var avoidanceFactor = Mathf.Clamp01(Vector2.Dot(toFollower.normalized, leaderDirection) - distanceInverse);
                var avoidSide = Cross(leaderDirection, toFollower) > 0 ? 1 : -1;
                var avoid = leaderPosition + avoidSide * AvoidanceDistance * Vector2.Perpendicular(leaderDirection);

                Debug.DrawLine(order.CurrentPosition, avoid, Color.green);

                order.Target = new GnomeMovement.Target
                {
                    Position = Vector2.Lerp(order.Target.Position, avoid, avoidanceFactor),
                    Radius = Mathf.Lerp(order.Target.Radius, DefaultDestinationRadius, avoidanceFactor),
                };
            }
        }

        private void ApplyLeaderFollowing(
            GnomeMovement.Target leaderDestination,
            Vector2 leaderPosition,
            Span<Crowd.Order> orders)
        {
            var leaderTarget = Vector2.Lerp(leaderPosition, leaderDestination.Position, Overtaking);
            for (var i = 0; i < orders.Length; i++)
            {
                ref var order = ref orders[i];

                Debug.DrawLine(order.CurrentPosition, leaderTarget, Color.blue);

                order.Target = new GnomeMovement.Target
                {
                    Position = leaderTarget,
                    Radius = FollowDistanceFromLeader + order.Rank * CrowdSpread,
                };
            }
        }

        private void ApplyBarking(Span<Crowd.Order> orders, Vector2 leaderPosition)
        {
            var smoothTime = IsBarking ? GrumpyHeatUp : GrumpyCoolDown;
            grumpiness = Mathf.SmoothDamp(grumpiness, IsBarking ? 1 : 0, ref grumpinessVelocity, smoothTime);

            for (var i = 0; i < orders.Length; i++)
            {
                ref var order = ref orders[i];
                var fromLeader = order.CurrentPosition - leaderPosition;
                var target = order.CurrentPosition + fromLeader.normalized * (grumpiness * GrumpyDistance);
                var factor = 1 / fromLeader.magnitude;

                Debug.DrawLine(order.CurrentPosition, target, Color.red);

                order.Target = new GnomeMovement.Target
                {
                    Position = Vector2.Lerp(order.Target.Position, target, factor),
                    Radius = Mathf.Lerp(order.Target.Radius, DefaultDestinationRadius, factor),
                };
            }
        }

        private static float Cross(Vector3 a, Vector3 b) => a.x * b.y - a.y * b.x;
    }
}