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
            leader = FirstGnome;
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

        public void OnLeaderPerished()
        {
            var routine = FindNewLeader(leader.Crowd, leader.Position);
            leader.SetBehaviour(null);
            leader = null;
            StartCoroutine(routine);
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(gameObject.scene.name);
        }

        private IEnumerator FindNewLeader(Crowd crowd, Vector2 position)
        {
            var neighbours = new Collider2D[128];

            const int attemptsCount = 15;
            for (var i = 0; i < attemptsCount; i++)
            {
                yield return null;
                var successor = FindSuccessor(Config.JoinRadius, position, neighbours);
                if (successor != null)
                {
                    successor.SetBehaviour(new GnomeLeaderBehaviour(game: this, CameraController, Config, successor, crowd));
                    leader = successor;
                    yield break;
                }
            }

            GameOver();
        }

        private GnomeAgent FindSuccessor(float radius, Vector2 position, Collider2D[] neighbours)
        {

            var neighboursCount = Physics2D.OverlapCircleNonAlloc(position, radius, neighbours);
            for (var i = 0; i < neighboursCount; i++)
            {
                if (neighbours[i].GetComponent<GnomeAgent>() is { } gnome
                    && gnome != leader
                    && gnome.isActiveAndEnabled)
                {
                    return gnome;
                }
            }

            return null;
        }
    }
}