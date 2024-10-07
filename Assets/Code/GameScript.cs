using System.Collections;
using UnityEngine;

namespace Gnome
{
    public class GameScript : MonoBehaviour
    {
        public Animation IntroAnimation;
        public GnomeAgent FirstGnome;
        public CameraController CameraController;
        public GameplayConfig Config;

        public Behaviour[] DisableOnIntro;

        public void Awake()
        {
            foreach (var behaviour in DisableOnIntro)
            {
                behaviour.enabled = false;
            }
        }

        public IEnumerator Start()
        {
            var introClip = IntroAnimation.clip;
            var endTime = Time.time + introClip.length;
            IntroAnimation.Play();
            yield return new WaitUntil(() => Time.time >= endTime || Input.anyKeyDown);

            IntroAnimation[introClip.name].normalizedTime = 1f;
            foreach (var behaviour in DisableOnIntro)
            {
                behaviour.enabled = true;
            }

            var playerCrowd = new Crowd(priority: 0);
            FirstGnome.SetBehaviour(new GnomeLeaderBehaviour(CameraController, Config, FirstGnome, playerCrowd));
        }
    }
}