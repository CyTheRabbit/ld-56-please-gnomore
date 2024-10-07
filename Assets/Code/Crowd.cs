using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Gnome
{
    public class Crowd
    {
        public struct Order
        {
            public int Index;
            public int Rank;
            public float RankDistance;
            public Vector2 CurrentPosition;
            public GnomeMovement.Target Target;
            public bool Ignore;
        }

        public struct OrdersRankDistanceComparer : IComparer<Order>
        {
            public int Compare(Order x, Order y) =>
                x.RankDistance.CompareTo(y.RankDistance);
        }

        private const float DefaultDestinationRadius = 0.1f;

        public readonly List<GnomeAgent> Members = new();
        public Vector2 Center;
        public int Priority;

        public Crowd(int priority)
        {
            Priority = priority;
        }

        public void Invite(GnomeAgent gnome)
        {
            Members.Add(gnome);
            gnome.Crowd = this;
        }

        public void Exile(GnomeAgent gnome)
        {
            Members.Remove(gnome);
            gnome.Crowd = null;
        }

        public NativeArray<Order> BeginOrders()
        {
            var nativeOrders = new NativeArray<Order>(Members.Count, Allocator.Temp);
            var orders = nativeOrders.AsSpan();

            InitOrders(orders);
            AssignRanks(nativeOrders);

            return nativeOrders;
        }

        public void RunOrders(ReadOnlySpan<Order> orders)
        {
            foreach (var order in orders)
            {
                if (order.Ignore) { continue; }
                var follower = Members[order.Index];
                var isAlreadyThere = Vector2.Distance(order.CurrentPosition, order.Target.Position) <= order.Target.Radius;
                follower.Destination = isAlreadyThere ? null : order.Target;

                Debug.DrawLine(order.CurrentPosition, order.Target.Position, Color.yellow);
            }
        }
        
        private void InitOrders(Span<Order> orders)
        {
            for (var i = 0; i < orders.Length; i++)
            {
                var position = Members[i].Position;
                orders[i] = new Order
                {
                    Index = i,
                    CurrentPosition = position,
                    RankDistance = Vector2.Distance(Center, position),
                    Target =
                    {
                        Position = position,
                        Radius = DefaultDestinationRadius,
                    },
                };
            }
        }

        private static void AssignRanks(NativeArray<Order> nativeOrders)
        {
            nativeOrders.Sort(new OrdersRankDistanceComparer());
            var orders = nativeOrders.AsSpan();
            for (var i = 0; i < orders.Length; i++)
            {
                orders[i].Rank = i;
            }
        }
    }
}