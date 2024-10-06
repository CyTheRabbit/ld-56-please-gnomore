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

        public AudioSource Source;
        public AudioSource WalkSource;

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

        public void StartWalk()
        {
            WalkSource.Play();
        }

        public void StopWalk()
        {
            WalkSource.Pause();
        }

        private void Play(Sound sound)
        {
            Source.pitch = Random.Range(sound.PitchMin, sound.PitchMax);
            Source.PlayOneShot(sound.Clip, Random.Range(sound.VolumeMin, sound.VolumeMax));
        }
    }
}