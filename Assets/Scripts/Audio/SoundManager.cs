using System;
using UnityEngine;
using UnityEngine.Audio;

namespace KVA.SoundManager
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private SoundsSO _soundsSO;

        private static SoundManager s_instance = null;
        private AudioSource _audioSource;

        private void Awake()
        {
            if (!s_instance)
            {
                s_instance = this;
                _audioSource = GetComponent<AudioSource>();
            }
        }

        public static void PlaySound(SoundType sound, AudioSource source = null, float volume = 1)
        {
            SoundList soundList = s_instance._soundsSO.sounds[(int)sound];
            AudioClip[] clips = soundList.sounds;
            AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

            if (source)
            {
                source.outputAudioMixerGroup = soundList.mixer;
                source.clip = randomClip;
                source.volume = volume * soundList.volume;
                source.Play();
            }
            else
            {
                s_instance._audioSource.outputAudioMixerGroup = soundList.mixer;
                s_instance._audioSource.PlayOneShot(randomClip, volume * soundList.volume);
            }
        }
    }

    [Serializable]
    public struct SoundList
    {
        [HideInInspector] public string name;
        [Range(0, 1)] public float volume;
        public AudioMixerGroup mixer;
        public AudioClip[] sounds;
    }
}
