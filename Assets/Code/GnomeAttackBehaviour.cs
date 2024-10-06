using System.Collections;
using UnityEngine;

namespace Gnome
{
    public class GnomeAttackBehaviour : GnomeAgent.IBehaviour
    {
        private readonly GnomeAgent gnome;
        private readonly IPunchable victim;
        private Coroutine attackCoroutine;

        public int Priority => victim.Priority;

        public GnomeAttackBehaviour(GnomeAgent gnome, IPunchable victim)
        {
            this.gnome = gnome;
            this.victim = victim;
        }

        public void Start()
        {
            attackCoroutine = gnome.StartCoroutine(FollowAndPunch());
        }

        private IEnumerator FollowAndPunch()
        {
            const float handLength = 0.25f;
            const float punchCooldown = 0.4f;
            var punchReach = victim.Radius + handLength;

            while (!victim.IsDead)
            {
                if (Vector2.Distance(gnome.Position, victim.Position) > punchReach)
                {
                    var destination = new GnomeMovement.Target
                    {
                        Position = victim.Position,
                        Radius = punchReach,
                    };
                    gnome.Destination = destination;
                }
                else
                {
                    gnome.Punch(victim);
                    yield return new WaitForSeconds(punchCooldown);
                }
                yield return new WaitForFixedUpdate();
            }

            gnome.SetBehaviour(null);
        }

        public void End()
        {
            if (attackCoroutine != null)
            {
                gnome.StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
        }
    }
}