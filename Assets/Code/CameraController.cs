using System;
using UnityEngine;

namespace Gnome
{
    public class CameraController : MonoBehaviour
    {
        public Transform CameraTransform;
        public Transform AreaTransform;
        [Range(0, 1)]
        public float AreaStrength;
        public float SmoothTime;

        private Vector2 velocity;

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<AreaAnchor>() is { } area)
            {
                AreaTransform = area.transform;
            }
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            if (other.transform == AreaTransform)
            {
                AreaTransform = null;
            }
        }

        public void Update()
        {
            var screenSize = new Vector2(16, 9);
            var screenTile = Vector2Int.RoundToInt(transform.position / screenSize); 
            var target = Vector2.Lerp(transform.position, screenTile * screenSize, AreaStrength);
            CameraTransform.position = Vector2.SmoothDamp(CameraTransform.position, target, ref velocity, SmoothTime);
        }
    }
}