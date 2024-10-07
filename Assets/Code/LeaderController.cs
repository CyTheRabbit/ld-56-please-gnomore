using UnityEngine;

namespace Gnome
{
    public class LeaderController : MonoBehaviour
    {
        public GameplayConfig Config;
        public GnomeAgent Leader;
        public CameraController Camera;

        public void Start()
        {
            var crowd = new Crowd(priority: 0);
            Leader.SetBehaviour(new GnomeLeaderBehaviour(Camera, Config, Leader, crowd));
        }
    }
}