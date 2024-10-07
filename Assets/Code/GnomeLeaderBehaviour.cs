using System;
using System.Collections;
using UnityEngine;

namespace Gnome
{
    public class GnomeLeaderBehaviour : GnomeAgent.IBehaviour
    {
        private readonly GameScript game;
        private readonly CameraController camera;
        private readonly GameplayConfig config;
        private readonly GnomeAgent leader;
        private readonly Collider2D[] neighbours = new Collider2D[128];
        private readonly Crowd crowd;

        private Coroutine fixedUpdateCoroutine;
        private Coroutine inputCoroutine;

        private float grumpiness;
        private float grumpinessVelocity;
        private bool isBarking;

        public int Priority => 100;

        public GnomeLeaderBehaviour(
            GameScript game,
            CameraController camera,
            GameplayConfig config,
            GnomeAgent leader,
            Crowd crowd)
        {
            this.game = game;
            this.camera = camera;
            this.config = config;
            this.leader = leader;
            this.crowd = crowd;
        }

        public void Start()
        {
            game.OnLeaderChanged(leader);
            camera.Player = leader;
            crowd.Invite(leader);
            leader.WalksSilently = false;
            fixedUpdateCoroutine = leader.StartCoroutine(FixedUpdateLoop());
            inputCoroutine = leader.StartCoroutine(InputLoop());
        }

        public void End()
        {
            game.OnLeaderChanged(null);
            camera.Player = null;
            crowd.Exile(leader);
            leader.WalksSilently = true;

            if (fixedUpdateCoroutine != null)
            {
                leader.StopCoroutine(fixedUpdateCoroutine);
                fixedUpdateCoroutine = null;
            }

            if (inputCoroutine != null)
            {
                leader.StopCoroutine(inputCoroutine);
                inputCoroutine = null;
            }
        }

        public void OnPerish()
        {
            var successor = FindSuccessor(config.JoinRadius);
            if (successor != null)
            {
                leader.SetBehaviour(null);
                successor.SetBehaviour(new GnomeLeaderBehaviour(game, camera, config, successor, crowd));
            }
            else
            {
                game.GameOver();
            }
        }

        private IEnumerator FixedUpdateLoop()
        {
            while (leader != null)
            {
                yield return new WaitForFixedUpdate();
                UpdateCrowd();
            }
        }

        private IEnumerator InputLoop()
        {
            while (leader != null)
            {
                yield return null;
                ReadInputs();
            }
        }

        private void ReadInputs()
        {
            if (Input.GetMouseButton(0))
            {
                var mousePosition = (Vector2) camera.Camera.ScreenPointToRay(Input.mousePosition).GetPoint(distance: 0f);
                leader.Destination = new GnomeMovement.Target
                {
                    Position = mousePosition,
                    Radius = 0.01f,
                };
            }
            else
            {
                var inputDirection = Vector2.up * Input.GetAxis("Vertical") + Vector2.right * Input.GetAxis("Horizontal");
                if (inputDirection.magnitude > 0.1f)
                {
                    leader.Destination = new GnomeMovement.Target
                    {
                        Position = leader.Position + Vector2.ClampMagnitude(inputDirection.normalized, 1f) * 2f,
                        Radius = 0.1f,
                    };
                }
                else
                {
                    leader.Destination = null;
                }
            }

            isBarking = Input.GetButton("Bark");
            if (Input.GetButtonDown("Bark"))
            {
                leader.Bark();
            }
        }

        public void UpdateCrowd()
        {
            var leaderPosition = leader.Position;
            var destination = leader.Destination ?? new GnomeMovement.Target
            {
                Position = leaderPosition,
                Radius = DefaultDestinationRadius,
            };
            var leaderDirection = (destination.Position - leaderPosition).normalized;

            InviteNeighbours(config.JoinRadius);

            leader.Crowd.Center = leaderPosition;
            using (var nativeOrders = leader.Crowd.BeginOrders())
            {
                var orders = nativeOrders.AsSpan();

                ApplyLeaderFollowing(destination, leaderPosition, orders);
                ApplyLeaderAvoidance(orders, leaderPosition, leaderDirection);
                ApplySocialDistance(orders, leaderPosition);
                ApplyBarking(orders, leaderPosition);
                ExcludeLeader(orders);

                leader.Crowd.RunOrders(orders);
            }

            leader.Destination = destination;
        }

        private void ExcludeLeader(Span<Crowd.Order> orders)
        {
            var leaderIndex = crowd.Members.IndexOf(leader);
            for (var i = 0; i < orders.Length; i++)
            {
                ref var order = ref orders[i];
                if (order.Index == leaderIndex)
                {
                    order.Ignore = true;
                }
            }
        }

        private void InviteNeighbours(float radius)
        {
            var neighboursCount = Physics2D.OverlapCircleNonAlloc(leader.Position, radius, neighbours);
            for (var i = 0; i < neighboursCount; i++)
            {
                if (neighbours[i].GetComponent<GnomeAgent>() is { } gnome && gnome != leader)
                {
                    TryInvite(gnome);
                }
            }
        }

        private GnomeAgent FindSuccessor(float radius)
        {
            var neighboursCount = Physics2D.OverlapCircleNonAlloc(leader.Position, radius, neighbours);
            for (var i = 0; i < neighboursCount; i++)
            {
                if (neighbours[i].GetComponent<GnomeAgent>() is { } gnome && gnome != leader)
                {
                    return gnome;
                }
            }

            return null;
        }

        private void TryInvite(GnomeAgent gnome)
        {
            if (gnome == leader) return;
            if (gnome.Crowd == leader.Crowd) return;
            if ((gnome.Behaviour?.Priority ?? -100) >= config.FollowPriority) return;
            gnome.SetBehaviour(new GnomeFollowBehaviour(gnome, leader.Crowd));
        }

        private void ApplySocialDistance(Span<Crowd.Order> orders, Vector2 leaderPosition)
        {
            for (var i = 0; i < orders.Length; i++)
            {
                ref var order = ref orders[i];
                var fromLeader = order.CurrentPosition - leaderPosition;
                var target = order.CurrentPosition + fromLeader.normalized * config.SocialDistance;
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
                var avoid = leaderPosition + avoidSide * config.AvoidanceDistance * Vector2.Perpendicular(leaderDirection);

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
            var leaderTarget = Vector2.Lerp(leaderPosition, leaderDestination.Position, config.Overtaking);
            for (var i = 0; i < orders.Length; i++)
            {
                ref var order = ref orders[i];

                Debug.DrawLine(order.CurrentPosition, leaderTarget, Color.blue);

                order.Target = new GnomeMovement.Target
                {
                    Position = leaderTarget,
                    Radius = config.FollowDistanceFromLeader + order.Rank * config.CrowdSpread,
                };
            }
        }

        private void ApplyBarking(Span<Crowd.Order> orders, Vector2 leaderPosition)
        {
            var smoothTime = isBarking ? config.GrumpyHeatUp : config.GrumpyCoolDown;
            grumpiness = Mathf.SmoothDamp(grumpiness, isBarking ? 1 : 0, ref grumpinessVelocity, smoothTime);

            for (var i = 0; i < orders.Length; i++)
            {
                ref var order = ref orders[i];
                var fromLeader = order.CurrentPosition - leaderPosition;
                var target = order.CurrentPosition + fromLeader.normalized * (grumpiness * config.GrumpyDistance);
                var factor = 1 / fromLeader.magnitude;

                Debug.DrawLine(order.CurrentPosition, target, Color.red);

                order.Target = new GnomeMovement.Target
                {
                    Position = Vector2.Lerp(order.Target.Position, target, factor),
                    Radius = Mathf.Lerp(order.Target.Radius, DefaultDestinationRadius, factor),
                };
            }
        }

        private const float DefaultDestinationRadius = 0.1f;

        private static float Cross(Vector3 a, Vector3 b) => a.x * b.y - a.y * b.x;
    }
}