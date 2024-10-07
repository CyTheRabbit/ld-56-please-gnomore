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
        public GameObject WinScreen;
        public GameUI GameUI;
        public GnomeAgent FirstGnome;
        public CameraController CameraController;
        public GameplayConfig Config;
        [Space]
        public AudioMixer AudioMixer;

        public Behaviour[] DisableOnIntro;

        private bool muteNoise = false;
        private int gnomeDustCount = 0;
        private GnomeAgent leader = null;
        private const int GnomeDustToWin = 69;

        public void Awake()
        {
            foreach (var behaviour in DisableOnIntro)
            {
                behaviour.enabled = false;
            }
            GameUI.gameObject.SetActive(false);
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
            GameUI.gameObject.SetActive(true);
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
            GameUI.gameObject.SetActive(false);
            GameOverScreen.SetActive(true);
            GameOverAnimation.Play();
        }

        public void GainGnomeDust()
        {
            gnomeDustCount++;
            GameUI.SetGnomeDustCount(gnomeDustCount);
            if (gnomeDustCount >= GnomeDustToWin)
            {
                WinScreen.SetActive(true);
                leader?.SetBehaviour(null);
            }
        }

        public void OnLeaderChanged(GnomeAgent gnome)
        {
            leader = gnome;
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(gameObject.scene.name);
        }
    }
}