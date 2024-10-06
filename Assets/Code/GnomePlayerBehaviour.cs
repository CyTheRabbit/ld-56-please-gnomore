using System.Collections;
using UnityEngine;

namespace Gnome
{
    public class GnomePlayerBehaviour : GnomeAgent.IBehaviour
    {
        private readonly GnomeAgent gnome;
        private readonly LeaderController leader;
        private readonly Camera camera;
        private Coroutine inputCoroutine;

        public int Priority => 100;

        public GnomePlayerBehaviour(GnomeAgent gnome, LeaderController leader, Camera camera)
        {
            this.gnome = gnome;
            this.leader = leader;
            this.camera = camera;
        }

        public void Start()
        {
            inputCoroutine = gnome.StartCoroutine(InputLoop());
        }

        public IEnumerator InputLoop()
        {
            while (gnome != null)
            {
                ReadInputs();
                yield return null;
            }
        }

        public void End()
        {
            gnome.StopCoroutine(inputCoroutine);
        }

        private void ReadInputs()
        {
            if (Input.GetMouseButton(0))
            {
                var mousePosition = (Vector2) camera.ScreenPointToRay(Input.mousePosition).GetPoint(distance: 0f);
                gnome.Destination = new GnomeMovement.Target
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
                    gnome.Destination = new GnomeMovement.Target
                    {
                        Position = gnome.Position + Vector2.ClampMagnitude(inputDirection.normalized, 1f) * 2f,
                        Radius = 0.1f,
                    };
                }
                else
                {
                    gnome.Destination = null;
                }
            }

            leader.IsBarking = Input.GetButton("Bark");
        }
    }
}