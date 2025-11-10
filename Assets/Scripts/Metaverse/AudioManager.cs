using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("사운드 채널")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;

    [Header("효과음 목록")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip explodeSound;
    [SerializeField] private AudioClip resetSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void PlaySfxAtPosition(string key, Vector3 position)
    {
        var clip = GetClipByKey(key);
        if (clip == null) return;

        // 임시 AudioSource 생성
        GameObject temp = new GameObject("TempAudio");
        temp.transform.position = position;

        var source = temp.AddComponent<AudioSource>();
        source.clip = clip;

        source.spatialBlend = 1.0f;        // ✅ 3D 사운드
        source.minDistance = 1.0f;
        source.maxDistance = 20.0f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.dopplerLevel = 0;

        source.Play();
        Destroy(temp, clip.length + 0.2f); // 클립 끝나면 제거
    }


    private AudioClip GetClipByKey(string key)
    {
        return key switch
        {
            "click" => clickSound,
            "explode" => explodeSound,
            "reset" => resetSound,
            _ => null
        };
    }

    public void PlayBGM(AudioClip bgm)
    {
        if (bgmSource.clip != bgm)
        {
            bgmSource.clip = bgm;
            bgmSource.Play();
        }
    }

    public void StopBGM() => bgmSource.Stop();
}