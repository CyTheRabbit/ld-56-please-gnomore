using UnityEngine;

namespace Gnome
{
    public class PerspectiveLayer : MonoBehaviour
    {
        public void LateUpdate()
        {
            var position = transform.position;
            position.z = position.y;
            transform.position = position;
        }
    }
}