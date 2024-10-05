using UnityEngine;

namespace Gnome
{
    public class AvatarController : MonoBehaviour
    {
        public GnomeMovement Movement;

        public void Update()
        {
            if (Input.GetMouseButton(0))
            {
                var mousePosition = (Vector2) Camera.main.ScreenPointToRay(Input.mousePosition).GetPoint(distance: 0f);
                Movement.Destination = new GnomeMovement.Target
                {
                    Position = mousePosition,
                    Radius = 0.01f,
                };
            }
            else
            {
                var inputDirection = (Vector2.up * Input.GetAxis("Vertical") + Vector2.right * Input.GetAxis("Horizontal"));
                if (inputDirection.magnitude > 0.1f)
                {
                    Movement.Destination = new GnomeMovement.Target
                    {
                        Position = Movement.Position + inputDirection.normalized * 2,
                        Radius = 0.1f,
                    };
                }
                else
                {
                    Movement.Destination = null;
                }
            }
        }
    }
}

