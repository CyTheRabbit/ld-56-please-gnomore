using System.Collections;
using UnityEngine;

namespace Gnome
{
    public class GnomeJoyBehaviour : GnomeAgent.IBehaviour
    {
        private readonly DesirableItem item;
        private readonly GnomeAgent gnome;
        private Coroutine coroutine;

        public int Priority { get; }

        public GnomeJoyBehaviour(GnomeAgent gnome, int priority, DesirableItem item)
        {
            this.gnome = gnome;
            Priority = priority;
            this.item = item;
        }

        public void Start()
        {
            coroutine = gnome.StartCoroutine(JumpOfJoy());

            Object.FindObjectOfType<GameScript>().GainGnomeDust();
        }

        public void End()
        {
            if (coroutine != null)
            {
                gnome.StopCoroutine(coroutine);
                coroutine = null;
            }
        }

        private IEnumerator JumpOfJoy()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();
            var waitForJump = new WaitForSeconds(1/3f);
            while (gnome != null)
            {
                if (Vector2.Distance(gnome.Position,  item.Position) > item.HappyCircle)
                {
                    gnome.Destination = new GnomeMovement.Target
                    {
                        Position = item.Position,
                        Radius = item.HappyCircle,
                    };
                }
                else
                {
                    for (var i = 0; i < 3; i++)
                    {
                        gnome.Jump();
                        yield return waitForJump;
                    }

                    yield return new WaitForSeconds(Random.Range(1, 3));
                }

                yield return waitForFixedUpdate;
            }
        }
    }
}