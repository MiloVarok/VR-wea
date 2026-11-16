using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundEffect
{
    public string id;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0f, 0.2f)]
    public float pitchVariation = 0.05f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Música de Fondo")]
    public AudioClip backgroundMusicClip;
    [Range(0f, 1f)] public float backgroundMusicVolume = 0.30f;
    public bool startMusicMutedForIntro = true;

    [Header("Loop de Susurros")]
    public AudioClip whisperLoopClip;
    [Tooltip("Velocidad con la que sube o baja el volumen de los susurros")]
    public float whisperFadeSpeed = 2f;

    [Header("Efectos Puntuales")]
    public List<SoundEffect> soundEffects = new List<SoundEffect>();

    [Header("Pool Aleatorio de sonidos")]
    public List<AudioClip> randomSpookyClips = new List<AudioClip>();
    [Range(0f, 1f)] public float spookyClipsVolume = 0.25f;

    private AudioSource sfxSource;
    private AudioSource musicSource;
    private AudioSource whisperSource;
    private float targetWhisperVolume = 0f;
    private Coroutine musicFadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;

        // Reproductor de música de fondo
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = backgroundMusicClip;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.volume = startMusicMutedForIntro ? 0f : backgroundMusicVolume;
        if (backgroundMusicClip != null)
            musicSource.Play();

        // Reproductor de susurros
        whisperSource = gameObject.AddComponent<AudioSource>();
        whisperSource.clip = whisperLoopClip;
        whisperSource.loop = true;
        whisperSource.playOnAwake = false;
        whisperSource.spatialBlend = 0f;
        whisperSource.volume = 0f;
        if (whisperLoopClip != null)
            whisperSource.Play();
    }

    private void Update()
    {
        if (whisperSource != null && whisperLoopClip != null)
        {
            whisperSource.volume = Mathf.MoveTowards(
                whisperSource.volume,
                targetWhisperVolume,
                Time.deltaTime * whisperFadeSpeed
            );
        }
    }
    public void FadeInBackgroundMusic(float duration)
    {
        if (musicSource == null || backgroundMusicClip == null) return;

        if (!musicSource.isPlaying)
            musicSource.Play();

        if (musicFadeCoroutine != null)
            StopCoroutine(musicFadeCoroutine);

        musicFadeCoroutine = StartCoroutine(FadeMusicRoutine(backgroundMusicVolume, duration));
    }

    private IEnumerator FadeMusicRoutine(float targetVol, float duration)
    {
        float startVol = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / Mathf.Max(0.01f, duration));
            musicSource.volume = Mathf.Lerp(startVol, targetVol, t);
            yield return null;
        }

        musicSource.volume = targetVol;
    }

    public void SetWhisperVolume(float volume)
    {
        targetWhisperVolume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// Reproduce un sonido por su ID.
    /// </summary>
    public void PlaySFX(string soundId)
    {
        if (string.IsNullOrEmpty(soundId)) return;

        SoundEffect sfx = soundEffects.Find(s =>
            s != null && s.clip != null &&
            string.Equals(s.id.Trim(), soundId.Trim(), System.StringComparison.OrdinalIgnoreCase));

        if (sfx != null)
        {
            sfxSource.pitch = Random.Range(1f - sfx.pitchVariation, 1f + sfx.pitchVariation);
            sfxSource.PlayOneShot(sfx.clip, sfx.volume);
        }
        
    }

    public void PlayRandomSpooky()
    {
        if (randomSpookyClips == null || randomSpookyClips.Count == 0) return;

        AudioClip chosen = randomSpookyClips[Random.Range(0, randomSpookyClips.Count)];
        if (chosen != null)
        {
            sfxSource.pitch = Random.Range(0.95f, 1.05f);
            sfxSource.PlayOneShot(chosen, spookyClipsVolume);
        }
    }

    public void StopAllLoops()
    {
        targetWhisperVolume = 0f;
        if (whisperSource != null) whisperSource.Stop();
        if (musicSource != null) musicSource.Stop();
    }
}