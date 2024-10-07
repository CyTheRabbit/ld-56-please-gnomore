using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace Gnome
{
    public class GameScript : MonoBehaviour
    {
        public Animation IntroAnimation;
        public Animation GameOverAnimation;
        public GameObject GameOverScreen;
        public GnomeAgent FirstGnome;
        public CameraController CameraController;
        public GameplayConfig Config;
        [Space]
        public AudioMixer AudioMixer;

        public Behaviour[] DisableOnIntro;

        private bool muteNoise = false;

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
            FirstGnome.SetBehaviour(new GnomeLeaderBehaviour(game: this, CameraController, Config, FirstGnome, playerCrowd));
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                muteNoise = !muteNoise;
                AudioMixer.SetFloat("NoiseVolume", muteNoise ? -80 : 0);
            }
        }

        public void GameOver()
        {
            GameOverScreen.SetActive(true);
            GameOverAnimation.Play();
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(gameObject.scene.name);
        }
    }
}