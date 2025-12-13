using UnityEngine;

namespace Game
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip moleHitSound;
        [SerializeField] private AudioClip moleMissSound;
        [SerializeField] private AudioClip hammerSwingSound;
        [SerializeField] private AudioClip countdownSound;
        [SerializeField] private AudioClip gameStartSound;
        [SerializeField] private AudioClip gameOverSound;

        [Header("Music")]
        [SerializeField] private AudioClip backgroundMusic;

        [Header("Volume Settings")]
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.7f;
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.3f;

        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
        }

        private void InitializeAudioSources()
        {
            if (!sfxSource)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.volume = sfxVolume;
            }

            if (!musicSource)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
                musicSource.volume = musicVolume;
            }
        }

        private void Start()
        {
            PlayBackgroundMusic();
        }

        public void PlayBackgroundMusic()
        {
            if (backgroundMusic && musicSource)
            {
                musicSource.clip = backgroundMusic;
                musicSource.Play();
            }
        }

        public void StopBackgroundMusic()
        {
            if (musicSource) musicSource.Stop();
        }

        public void PlayMoleHit()
        {
            PlaySFX(moleHitSound);
        }

        public void PlayMoleMiss()
        {
            PlaySFX(moleMissSound);
        }

        public void PlayHammerSwing()
        {
            PlaySFX(hammerSwingSound);
        }

        public void PlayCountdown()
        {
            PlaySFX(countdownSound);
        }

        public void PlayGameStart()
        {
            PlaySFX(gameStartSound);
        }

        public void PlayGameOver()
        {
            PlaySFX(gameOverSound);
        }

        private void PlaySFX(AudioClip clip)
        {
            if (clip && sfxSource)
            {
                sfxSource.PlayOneShot(clip, sfxVolume);
            }
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (sfxSource) sfxSource.volume = sfxVolume;
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource) musicSource.volume = musicVolume;
        }

        public void MuteSFX(bool mute)
        {
            if (sfxSource) sfxSource.mute = mute;
        }

        public void MuteMusic(bool mute)
        {
            if (musicSource) musicSource.mute = mute;
        }
    }
}
