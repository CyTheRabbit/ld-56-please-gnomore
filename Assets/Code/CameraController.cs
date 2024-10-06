using UnityEngine;

namespace Gnome
{
    public class CameraController : MonoBehaviour
    {
        [Range(0, 1)]
        public float AreaStrength;
        public float SmoothTime;
        public Camera Camera;

        private Vector2 velocity;
        public GnomeAgent Player;

        public void Update()
        {
            if (Player == null) return;
            var screenSize = new Vector2(16, 9);
            var screenTile = Vector2Int.RoundToInt(Player.Position / screenSize); 
            var target = Vector2.Lerp(Player.Position, screenTile * screenSize, AreaStrength);
            transform.position = Vector2.SmoothDamp(transform.position, target, ref velocity, SmoothTime);
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