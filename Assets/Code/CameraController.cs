using System;
using UnityEditor;
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.black;

            // Get the scene view camera
            var sceneCamera = Camera.current;
            if (sceneCamera == null)
                return;

            var screenSize = new Vector2(16, 9);

            var cameraPos = sceneCamera.transform.position;

            var minX = Mathf.FloorToInt((cameraPos.x - sceneCamera.orthographicSize * sceneCamera.aspect) / screenSize.x - 1);
            var maxX = Mathf.CeilToInt((cameraPos.x + sceneCamera.orthographicSize * sceneCamera.aspect) / screenSize.x + 1);

            var minY = Mathf.FloorToInt((cameraPos.y - sceneCamera.orthographicSize) / screenSize.y - 1);
            var maxY = Mathf.CeilToInt((cameraPos.y + sceneCamera.orthographicSize) / screenSize.y + 1);

            var rooms = new RectInt(minX, minY, maxX - minX, maxY - minY);
            var halfIndexOffset = new Vector2(0.5f, 0.5f);
            foreach (var roomIndex in rooms.allPositionsWithin)
            {
                var roomRect = new Rect((roomIndex - halfIndexOffset) * screenSize, screenSize);
                Gizmos.DrawWireCube(roomRect.center, roomRect.size);
            }
        }
    }
}