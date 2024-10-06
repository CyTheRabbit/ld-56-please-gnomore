using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Gnome
{
    public class PunchableDecoration : MonoBehaviour, IPunchable
    {
        public struct OpponentData
        {
            public int Index;
            public float Distance;
        }

        public struct OpponentComparerByDistance : IComparer<OpponentData>
        {
            public int Compare(OpponentData x, OpponentData y) =>
                x.Distance.CompareTo(y.Distance);
        }
        
        public int Health;
        public int MaxOpponents;
        public int AttackPriority;
        public CircleCollider2D Body;
        public BounceAnimator BounceAnimator;

        public Vector2 Position => transform.position;
        public int Priority => AttackPriority;
        public float Radius => Body.radius;

        public List<GnomeAgent> Opponents = new();
        public List<GnomeAgent> OpponentsInQueue = new();

        public bool IsDead => this == null || Health <= 0;

        public void FixedUpdate()
        {
            Opponents.RemoveAll(gnome => gnome == null);
            OpponentsInQueue.RemoveAll(gnome => gnome == null);

            if (Opponents.Count < MaxOpponents && OpponentsInQueue.Count > 0)
            {
                EngageNewOpponents();
            }
        }

        private void EngageNewOpponents()
        {
            using var nativeCandidates = new NativeArray<OpponentData>(OpponentsInQueue.Count, Allocator.Temp);
            var candidates = nativeCandidates.AsSpan();
            for (var i = 0; i < OpponentsInQueue.Count; i++)
            {
                candidates[i] = new OpponentData
                {
                    Index = i,
                    Distance = Vector2.Distance(OpponentsInQueue[i].Position, Position),
                };
            }
            nativeCandidates.Sort(new OpponentComparerByDistance());
            var places = Math.Min(MaxOpponents - Opponents.Count, candidates.Length);
            for (var i = 0; i < places; i++)
            {
                var winner = OpponentsInQueue[candidates[i].Index];
                var becameOpponent = Provoke(winner);
                if (!becameOpponent)
                {
                    i--;
                    places--;
                }
            }
            for (var i = Opponents.Count - places; i < Opponents.Count; i++)
            {
                OpponentsInQueue.Remove(Opponents[i]);
            }
        }

        public void TakeHit(Vector2 hitDirection)
        {
            Health--;

            if (Health == 0)
            {
                Destroy(gameObject);
            }

            BounceAnimator.Bump(hitDirection);
        }

        public bool Provoke(GnomeAgent gnome)
        {
            if ((gnome.Behaviour?.Priority ?? 0) >= Priority) return false;
            gnome.SetBehaviour(new GnomeAttackBehaviour(gnome, victim: this));
            Opponents.Add(gnome);
            return true;
        }

        public void AddOpponent(GnomeAgent gnome)
        {
            if (Opponents.Count < MaxOpponents)
            {
                Provoke(gnome);
            }
            else
            {
                OpponentsInQueue.Add(gnome);
            }
        }

        public void RemoveOpponent(GnomeAgent gnome)
        {
            if (Opponents.Remove(gnome))
            {
                gnome.SetBehaviour(null);
            }
            else
            {
                OpponentsInQueue.Remove(gnome);
            }
        }
    }
}