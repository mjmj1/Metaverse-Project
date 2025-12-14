using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

namespace Game
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioMixer audioMixer;   // 믹서 파일 연결
        [SerializeField] private AudioMixerGroup bgmGroup; // BGM 그룹 연결
        [SerializeField] private AudioMixerGroup sfxGroup; // SFX 그룹 연결

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource bgmSource;

        [Header("BGM Clips")]
        [SerializeField] private AudioClip lobbyBgm;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip clickClip;
        [SerializeField] private AudioClip countdownClip;
        [SerializeField] private AudioClip gameStartClip;
        [SerializeField] private AudioClip gameOverClip;

        [Header("Volume Settings")]
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.7f;
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.3f;

        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (!Instance) Instance = this;
            else Destroy(gameObject);

            InitializeAudioSources();
        }

        private void Start()
        {
            // 게임 시작 시 로비 음악 재생
            PlayLobbyBGM();
        }

        private void InitializeAudioSources()
        {
            // BGM 소스 설정
            if (!bgmSource)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.playOnAwake = false;
            }
            bgmSource.loop = true;
            bgmSource.outputAudioMixerGroup = bgmGroup; // 믹서 그룹 연결

            // SFX 소스 설정
            if (!sfxSource)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
            sfxSource.outputAudioMixerGroup = sfxGroup; // 믹서 그룹 연결
        }

        public void PlayLobbyBGM() => PlayBGM(lobbyBgm);

        private void PlayBGM(AudioClip clip)
        {
            if (bgmSource.clip == clip && bgmSource.isPlaying) return; // 이미 재생 중이면 패스

            bgmSource.Stop();
            bgmSource.clip = clip;
            if (clip) bgmSource.Play();
        }

        public void StopBGM() => bgmSource.Stop();

        public void Play3DSfx(AudioSource source, AudioClip clip)
        {
            if (source && clip)
            {
                // 볼륨은 믹서가 제어하므로 PlayOneShot에는 1.0f(최대)를 넘김
                source.PlayOneShot(clip, 1f);
            }
        }

        public void PlayCountdown() => PlaySfx(countdownClip);
        public void PlayGameStart() => PlaySfx(gameStartClip);
        public void PlayGameOver() => PlaySfx(gameOverClip);
        public void PlayClick() => PlaySfx(clickClip);

        private void PlaySfx(AudioClip clip)
        {
            if (clip && sfxSource)
            {
                sfxSource.PlayOneShot(clip, 1f);
            }
        }

        public void SetBGMVolume(float linear)
        {
            var db = linear <= 0.0001f ? -80f : Mathf.Log10(Mathf.Clamp01(linear)) * 20f;
            audioMixer.SetFloat("BGM", db);
        }

        public void SetSfxVolume(float linear)
        {
            var db = linear <= 0.0001f ? -80f : Mathf.Log10(Mathf.Clamp01(linear)) * 20f;
            audioMixer.SetFloat("SFX", db);
        }
    }
}
