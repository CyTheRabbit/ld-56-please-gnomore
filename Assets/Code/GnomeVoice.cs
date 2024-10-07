using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gnome
{
    public class GnomeVoice : MonoBehaviour
    {
        [Serializable]
        public class Sound
        {
            public AudioClip Clip;
            public float PitchMin;
            public float PitchMax;
            public float VolumeMin;
            public float VolumeMax;
        }

        public Sound[] PunchSounds;
        public Sound[] Barks;
        public Sound[] Yelps;
        public Sound[] Wishes;

        public AudioSource Source;
        public AudioSource WalkSource;

        private bool isWalking;
        private bool walksSilently = true;

        public bool IsWalking
        {
            get => isWalking;
            set
            {
                if (isWalking == value) return;
                isWalking = value;
                
                if (WalksSilently) return;
                if (value) StartWalk();
                else StopWalk();
            }
        }

        public bool WalksSilently
        {
            get => walksSilently;
            set
            {
                if (walksSilently == value) return;
                walksSilently = value;
                if (!isWalking) return;
                if (value) StopWalk();
                else StartWalk();
            }
        }

        public void Bark()
        {
            Play(Barks.RandomElement());
        }

        public void Punch()
        {
            Play(PunchSounds.RandomElement());
        }

        public void Perish()
        {
            var yelp = Yelps.RandomElement();
            Play(yelp);

            Source.transform.SetParent(null);
            Destroy(Source.gameObject, yelp.Clip.length);
        }

        private void StartWalk()
        {
            WalkSource.Play();
        }

        private void StopWalk()
        {
            WalkSource.Pause();
        }

        private void Play(Sound sound)
        {
            Source.pitch = Random.Range(sound.PitchMin, sound.PitchMax);
            Source.PlayOneShot(sound.Clip, Random.Range(sound.VolumeMin, sound.VolumeMax));
        }

        public void Wish()
        {
            Play(Wishes.RandomElement());
        }
    }
}