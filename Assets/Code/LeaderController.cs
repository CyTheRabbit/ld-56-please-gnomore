using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Gnome
{
    public class LeaderController : MonoBehaviour
    {
        public struct NavigationData
        {
            public int Index;
            public int Rank;
            public float RankDistance;
            public Vector2 CurrentPosition;
            public GnomeMovement.Target Target;
        }

        private struct NavigationRankComparer : IComparer<NavigationData>
        {
            public int Compare(NavigationData x, NavigationData y) =>
                x.RankDistance.CompareTo(y.RankDistance);
        }

        private const float DefaultDestinationRadius = 0.1f;

        public GnomeMovement Leader;
        public List<GnomeMovement> Followers;
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

        public void FixedUpdate()
        {
            var leaderPosition = Leader.Position;
            var destination = Leader.Destination ?? new GnomeMovement.Target
            {
                Position = leaderPosition,
                Radius = DefaultDestinationRadius,
            };

            using var nativeNavigations = new NativeArray<NavigationData>(Followers.Count, Allocator.Temp);
            var navigations = nativeNavigations.AsSpan();

            InitNavigationData(navigations, leaderPosition);
            AssignRanks(nativeNavigations, navigations);

            var leaderDirection = (destination.Position - leaderPosition).normalized;
            var leaderTarget = Vector2.Lerp(leaderPosition, destination.Position, Overtaking);
            for (var i = 0; i < navigations.Length; i++)
            {
                ref var follower = ref navigations[i];

                Debug.DrawLine(follower.CurrentPosition, leaderTarget, Color.blue);

                follower.Target = new GnomeMovement.Target
                {
                    Position = leaderTarget,
                    Radius = FollowDistanceFromLeader + follower.Rank * CrowdSpread,
                };
            }

            for (var i = 0; i < navigations.Length; i++)
            {
                ref var follower = ref navigations[i];
                var toFollower = follower.CurrentPosition - leaderPosition;
                var distanceInverse = Mathf.Clamp01(0.3f / toFollower.magnitude);
                var avoidanceFactor = Mathf.Clamp01(Vector2.Dot(toFollower.normalized, leaderDirection) - distanceInverse);
                var avoidSide = Cross(leaderDirection, toFollower) > 0 ? 1 : -1;
                var avoid = leaderPosition + avoidSide * AvoidanceDistance * Vector2.Perpendicular(leaderDirection);

                Debug.DrawLine(follower.CurrentPosition, avoid, Color.green);

                follower.Target = new GnomeMovement.Target
                {
                    Position = Vector2.Lerp(follower.Target.Position, avoid, avoidanceFactor),
                    Radius = Mathf.Lerp(follower.Target.Radius, DefaultDestinationRadius, avoidanceFactor),
                };
            }

            for (var i = 0; i < navigations.Length; i++)
            {
                ref var navigation = ref navigations[i];
                var fromLeader = navigation.CurrentPosition - leaderPosition;
                var target = navigation.CurrentPosition + fromLeader.normalized * SocialDistance;
                var factor = Mathf.Clamp01(1 - fromLeader.magnitude);
                
                navigation.Target = new GnomeMovement.Target
                {
                    Position = Vector2.Lerp(navigation.Target.Position, target, factor),
                    Radius = Mathf.Lerp(navigation.Target.Radius, DefaultDestinationRadius, factor),
                };
            }

            ApplyBarking(navigations, leaderPosition);

            foreach (var navigation in navigations)
            {
                Debug.DrawLine(navigation.CurrentPosition, navigation.Target.Position, Color.yellow);

                var follower = Followers[navigation.Index];
                var isAlreadyThere = Vector2.Distance(navigation.CurrentPosition, navigation.Target.Position) <= navigation.Target.Radius;
                follower.Destination = isAlreadyThere ? null : navigation.Target;
            }
        }

        private void ApplyBarking(Span<NavigationData> navigations, Vector2 leaderPosition)
        {
            var smoothTime = IsBarking ? GrumpyHeatUp : GrumpyCoolDown;
            grumpiness = Mathf.SmoothDamp(grumpiness, IsBarking ? 1 : 0, ref grumpinessVelocity, smoothTime);

            for (var i = 0; i < navigations.Length; i++)
            {
                ref var navigation = ref navigations[i];
                var fromLeader = navigation.CurrentPosition - leaderPosition;
                var target = navigation.CurrentPosition + fromLeader.normalized * (grumpiness * GrumpyDistance);
                var factor = 1 / fromLeader.magnitude;

                Debug.DrawLine(navigation.CurrentPosition, target, Color.red);

                navigation.Target = new GnomeMovement.Target
                {
                    Position = Vector2.Lerp(navigation.Target.Position, target, factor),
                    Radius = Mathf.Lerp(navigation.Target.Radius, DefaultDestinationRadius, factor),
                };
            }
        }

        private void InitNavigationData(Span<NavigationData> navigations, Vector2 leaderPosition)
        {
            for (var i = 0; i < navigations.Length; i++)
            {
                var position = Followers[i].Position;
                navigations[i] = new NavigationData
                {
                    Index = i,
                    CurrentPosition = position,
                    RankDistance = Vector2.Distance(leaderPosition, position),
                    Target =
                    {
                        Position = position,
                        Radius = DefaultDestinationRadius,
                    },
                };
            }
        }

        private static void AssignRanks(NativeArray<NavigationData> nativeNavigations, Span<NavigationData> navigations)
        {
            nativeNavigations.Sort(new NavigationRankComparer());
            for (var i = 0; i < nativeNavigations.Length; i++)
            {
                navigations[i].Rank = i;
            }
        }

        private static float Cross(Vector3 a, Vector3 b) => a.x * b.y - a.y * b.x;
    }
}