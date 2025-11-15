using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

namespace Metaverse
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Mixer & Groups")]
        [SerializeField] private AudioMixer mixer;

        [SerializeField] private AudioMixerGroup bgmGroup;
        [SerializeField] private AudioMixerGroup sfxGroup; // 3D SFX용

        [Header("BGM Clips")]
        [SerializeField] private AudioClip bgm;
        [SerializeField] private AudioSource bgmSource;

        [Header("SFX Clips")]
        [SerializeField] private AudioClip chocolateBreakClip;
        [SerializeField] private AudioClip dartThrowClip;
        [SerializeField] private AudioClip balloonPopClip;

        [Header("3D SFX Pool Settings")]
        [SerializeField] private int poolCapacity = 10;
        [SerializeField] private int poolMaxSize = 20;

        private ObjectPool<AudioSource> sfxPool;

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

            InitBgmSource();
            InitSfxPool();
        }

        private void Start()
        {
            PlayBGM(bgm);
        }

        #region Init

        private void InitBgmSource()
        {
            if (!bgmSource)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
            }

            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            bgmSource.spatialBlend = 0f; // 2D BGM
            bgmSource.outputAudioMixerGroup = bgmGroup;
        }

        private void InitSfxPool()
        {
            sfxPool = new ObjectPool<AudioSource>(
                () =>
                {
                    var go = new GameObject("PooledSFX3D");
                    go.transform.SetParent(transform);
                    var src = go.AddComponent<AudioSource>();

                    src.outputAudioMixerGroup = sfxGroup;
                    src.playOnAwake = false;
                    src.loop = false;
                    src.spatialBlend = 1f;
                    src.minDistance = 1f;
                    src.maxDistance = 8f;
                    src.dopplerLevel = 1f;
                    src.spread = 360f;
                    src.rolloffMode = AudioRolloffMode.Linear;

                    go.SetActive(false);
                    return src;
                },
                src => src.gameObject.SetActive(true),
                src =>
                {
                    src.Stop();
                    src.clip = null;
                    src.gameObject.SetActive(false);
                },
                src => Destroy(src.gameObject),
                false,
                poolCapacity,
                poolMaxSize
            );
        }

        #endregion

        #region Volum

        public void SetBGMVolume(float linear)
        {
            var db = linear <= 0.0001f ? -80f : Mathf.Log10(Mathf.Clamp01(linear)) * 20f;
            mixer.SetFloat("BGM", db);
        }

        public void SetSfxVolume(float linear)
        {
            var db = linear <= 0.0001f ? -80f : Mathf.Log10(Mathf.Clamp01(linear)) * 20f;
            mixer.SetFloat("SFX", db);
        }

        #endregion

        #region BGM API

        public void PlayBGM(AudioClip clip, float volume = 1f, bool loop = true)
        {
            if (!clip) return;

            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            bgmSource.clip = clip;
            bgmSource.outputAudioMixerGroup = bgmGroup;
            bgmSource.volume = volume;
            bgmSource.loop = loop;
            bgmSource.Play();
        }

        public void StopBGM()
        {
            bgmSource.Stop();
        }

        #endregion

        #region SFX API

        /// <summary>
        /// 월드 좌표 기준 3D SFX 재생
        /// </summary>
        public void PlaySfx(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
        {
            if (!clip) return;

            var src = sfxPool.Get();
            src.transform.position = position;
            src.clip = clip;
            src.volume = volume;
            src.pitch = pitch;
            src.Play();
            StartCoroutine(ReleaseWhenDone(src));
        }

        private IEnumerator ReleaseWhenDone(AudioSource src)
        {
            yield return new WaitUntil(() => !src.isPlaying);
            sfxPool.Release(src);
        }

        #endregion
    }
}
