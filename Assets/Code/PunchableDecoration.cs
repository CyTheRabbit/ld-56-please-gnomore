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
        public Transform VisualRoot;
        [Range(0, 1000)]
        public float TiltOnHit;
        [Range(0, 30)]
        public float MoveOnHit;
        public float SmoothTime;

        private Vector2 moveVelocity;
        private float tiltVelocity;

        public Vector2 Position => transform.position;
        public int Priority => AttackPriority;
        public float Radius => Body.radius;

        public List<GnomeAgent> Opponents = new();
        public List<GnomeAgent> OpponentsInQueue = new();

        public bool IsDead => this == null || Health <= 0;

        public void Update()
        {
            var position = VisualRoot.localPosition;
            var rotation = VisualRoot.eulerAngles.z;

            position = Vector2.SmoothDamp(position, Vector2.zero, ref moveVelocity, SmoothTime);
            rotation = Mathf.SmoothDamp(rotation, 0, ref tiltVelocity, SmoothTime);

            VisualRoot.localPosition = position;
            VisualRoot.rotation = Quaternion.Euler(0, 0, rotation);
        }

        public void FixedUpdate()
        {
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

            moveVelocity = hitDirection * MoveOnHit;
            tiltVelocity = hitDirection.x * -TiltOnHit;
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
        }
    }
}