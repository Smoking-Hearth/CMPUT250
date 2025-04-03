using UnityEngine;
using UnityEngine.Audio;
using LitMotion;

public enum MusicState
{
    Normal, Battle
}
public class MusicSystem : MonoBehaviour
{
    [System.Serializable]
    public struct Theme
    {
        public AudioClip fullClip;
        public AudioClip loopClip;
    }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource blendAudioSource;
    [SerializeField] private Theme[] levelThemes;
    private int currentThemeIndex;
    private int currentBlendIndex;
    [SerializeField] private float defaultAudioVolume;
    [SerializeField] private float specialAudioVolume;
    private float currentVolume;
    [SerializeField] private float specialAmplifyDuration;
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private float fadeDurationSeconds;
    [SerializeField] private bool playOnAwake = false;

    private float fadeTimer;
    private float blendTimer;
    private float blendRatio;
    private bool blendDominant;
    private MotionHandle musicAmplifyAnim;
    private bool playing;
    private bool usingLoopClip;

    private void OnEnable()
    {
        currentVolume = defaultAudioVolume;
        if (playOnAwake)
        {
            PlayLevelTheme(0);
        }
    }

    private void Update()
    {
        if (fadeTimer > 0)
        {
            fadeTimer -= Time.deltaTime;
            float musicVolume = Mathf.Clamp(Mathf.Lerp(-50, 1, fadeTimer / fadeDurationSeconds), -50, 1);
            mixer.SetFloat("MusicVolume", musicVolume);

            if (fadeTimer <= 0)
            {
                audioSource.Stop();
                blendAudioSource.Stop();
            }
        }
        else if (playing && !audioSource.isPlaying && !usingLoopClip)
        {
            audioSource.clip = levelThemes[currentThemeIndex].loopClip;
            audioSource.loop = true;
            audioSource.Play();

            blendAudioSource.clip = levelThemes[currentBlendIndex].loopClip;
            blendAudioSource.loop = true;
            blendAudioSource.Play();

            usingLoopClip = true;
        }

        if (blendTimer > 0)
        {
            blendTimer -= Time.deltaTime;
            if (blendDominant)
            {
                blendRatio = Mathf.Lerp(1, 0, blendTimer / fadeDurationSeconds);
            }
            else
            {
                blendRatio = Mathf.Lerp(0, 1, blendTimer / fadeDurationSeconds);
            }
        }

        audioSource.volume = currentVolume * (1 - blendRatio);
        blendAudioSource.volume = currentVolume * blendRatio;
    }
    public void PlayLevelTheme(int index)
    {
        if (!playing)
        {
            playing = true;
            currentThemeIndex = index;
            audioSource.clip = levelThemes[index].fullClip;
            audioSource.loop = false;
            audioSource.Play();
            usingLoopClip = false;
            blendDominant = false;
        }
        else if (blendDominant)
        {
            if (currentBlendIndex == index)
            {
                return;
            }
            currentThemeIndex = index;
            blendTimer = fadeDurationSeconds;
            blendDominant = false;

            if (usingLoopClip)
            {
                audioSource.clip = levelThemes[index].loopClip;
                audioSource.loop = true;
            }
            else
            {
                audioSource.clip = levelThemes[index].fullClip;
                audioSource.loop = false;
            }
            audioSource.Play();
            audioSource.time = blendAudioSource.time;
        }
        else
        {
            if (currentThemeIndex == index)
            {
                return;
            }
            currentBlendIndex = index;
            blendTimer = fadeDurationSeconds;
            blendDominant = true;

            if (usingLoopClip)
            {
                blendAudioSource.clip = levelThemes[index].loopClip;
                blendAudioSource.loop = true;
            }
            else
            {
                blendAudioSource.clip = levelThemes[index].fullClip;
                blendAudioSource.loop = false;
            }
            blendAudioSource.Play();
            blendAudioSource.time = audioSource.time;
        }
        mixer.SetFloat("MusicVolume", 1);
        fadeTimer = 0;
    }

    public void StopMusic()
    {
        playing = false;
        fadeTimer = fadeDurationSeconds;
    }

    public void SpecialVolume()
    {
        musicAmplifyAnim.TryCancel();
        musicAmplifyAnim = LMotion.Create(currentVolume, specialAudioVolume, specialAmplifyDuration)
            .Bind(audioSource, (x, source) => source.volume = x);
    }

    public void DefaultVolume()
    {
        musicAmplifyAnim.TryCancel();
        musicAmplifyAnim = LMotion.Create(currentVolume, defaultAudioVolume, specialAmplifyDuration * 2)
    .Bind(audioSource, (x, source) => source.volume = x);
    }
}
