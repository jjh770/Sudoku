using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("오디오 소스")]
    public AudioSource sfxSource;

    [Header("사운드")]
    public AudioClip buttonClick;
    public AudioClip eraseSound;
    public AudioClip writeSound;

    [Header("볼륨 설정 (0~1)")]
    [Range(0f, 1f)]
    public float buttonClickVolume = 1f;
    [Range(0f, 1f)]
    public float eraseVolume = 1f;
    [Range(0f, 1f)]
    public float writeVolume = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayButtonClick()
    {
        PlaySound(buttonClick, buttonClickVolume);
    }

    public void PlayErase()
    {
        PlaySound(eraseSound, eraseVolume);
    }
    public void PlayWrite()
    {
        PlaySound(writeSound, writeVolume);
    }

    void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    public void SetVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }
    }
}
