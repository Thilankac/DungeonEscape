using UnityEngine;

/// <summary>
/// Plays short sound effects for key collect, win, and game over.
/// Uses Inspector clips when assigned; otherwise builds simple fallback tones
/// so audio still works with no assets.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips (optional — fallbacks are generated if empty)")]
    [SerializeField] private AudioClip keyCollectClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip gameOverClip;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.85f;

    private AudioSource audioSource;
    private AudioClip fallbackKey;
    private AudioClip fallbackWin;
    private AudioClip fallbackGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        // Keep SFX audible after Time.timeScale = 0 (win / game over).
        audioSource.ignoreListenerPause = true;

        BuildFallbacksIfNeeded();
    }

    public void PlayKeyCollect()
    {
        PlayClip(keyCollectClip != null ? keyCollectClip : fallbackKey);
    }

    public void PlayWin()
    {
        PlayClip(winClip != null ? winClip : fallbackWin);
    }

    public void PlayGameOver()
    {
        PlayClip(gameOverClip != null ? gameOverClip : fallbackGameOver);
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip == null || audioSource == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip, sfxVolume);
    }

    private void BuildFallbacksIfNeeded()
    {
        if (keyCollectClip == null)
        {
            fallbackKey = BuildArpeggio("FallbackKey", new[] { 880f, 1175f, 1568f }, 0.28f);
        }

        if (winClip == null)
        {
            fallbackWin = BuildArpeggio("FallbackWin", new[] { 523f, 659f, 784f, 1046f }, 0.55f);
        }

        if (gameOverClip == null)
        {
            fallbackGameOver = BuildDescendingBuzz("FallbackGameOver", 240f, 0.5f);
        }
    }

    private static AudioClip BuildArpeggio(string name, float[] freqs, float duration)
    {
        const int sampleRate = 22050;
        int sampleCount = Mathf.CeilToInt(duration * sampleRate);
        float[] data = new float[sampleCount];
        float seg = duration / freqs.Length;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            int index = Mathf.Clamp(Mathf.FloorToInt(t / seg), 0, freqs.Length - 1);
            float localT = t - index * seg;
            float env = Mathf.Exp(-3.5f * localT / seg);
            data[i] = Mathf.Sin(2f * Mathf.PI * freqs[index] * t) * env * 0.45f;
        }

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip BuildDescendingBuzz(string name, float startFreq, float duration)
    {
        const int sampleRate = 22050;
        int sampleCount = Mathf.CeilToInt(duration * sampleRate);
        float[] data = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float n = t / duration;
            float freq = Mathf.Lerp(startFreq, startFreq * 0.55f, n);
            float env = Mathf.Exp(-2.2f * n);
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.5f;
        }

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
