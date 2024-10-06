using System.Collections.Generic;
using UnityEngine;

namespace Gnome
{
    public class EnemyComponent : MonoBehaviour
    {
        public FistsAnimator FistsAnimator;
        public Rigidbody2D Rigidbody;
        public float AttackRange;
        public float SmoothTime;
        public float Speed;

        private readonly List<GnomeAgent> victims = new();
        private float nextAttackTime;
        private Coroutine activity;
        private float acceleration;

        private GnomeAgent activeVictim;

        public void OnEnable()
        {
            FistsAnimator.PunchLanded += LandPunch;
        }

        public void OnDisable()
        {
            FistsAnimator.PunchLanded -= LandPunch;
        }

        private GnomeAgent FindAttackTarget()
        {
            foreach (var victim in victims)
            {
                if (Vector2.Distance(victim.Position, transform.position) < AttackRange) return victim;
            }

            return null;
        }

        public void FixedUpdate()
        {
            var target = GetMoveTarget();
            var velocity = Rigidbody.velocity;
            Vector2.SmoothDamp(Rigidbody.position, target, ref velocity, SmoothTime, maxSpeed: Speed);
            Rigidbody.velocity = velocity;

            if (activeVictim == null && FindAttackTarget() is { } victim)
            {
                activeVictim = victim;
                FistsAnimator.SetTarget(victim.transform);
            }

            FistsAnimator.IsPunching = activeVictim != null;
        }

        public Vector2 GetMoveTarget()
        {
            if (victims.Count == 0) return transform.position;

            var sum = Vector2.zero;
            foreach (var victim in victims)
            {
                sum += victim.Position;
            }
            return sum / victims.Count;
        }

        public void AddVictim(GnomeAgent agent)
        {
            victims.Add(agent);
        }

        public void RemoveVictim(GnomeAgent agent)
        {
            victims.Remove(agent);
        }

        public void LandPunch()
        {
            Destroy(activeVictim.gameObject);
        }
    }
}