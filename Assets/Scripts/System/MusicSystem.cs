using UnityEngine;
using UnityEngine.Audio;
using LitMotion;

public enum MusicState
{
    Normal, Battle
}
public class MusicSystem : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip levelTheme;
    [SerializeField] private float defaultAudioVolume;
    [SerializeField] private float specialAudioVolume;
    [SerializeField] private float specialAmplifyDuration;
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private float cutOffPointSeconds;
    [SerializeField] private float fadeDurationSeconds;
    private float fadeTimer;
    private bool restart;
    private MotionHandle musicAmplifyAnim;

    private void Update()
    {
        if (fadeTimer > 0)
        {
            fadeTimer -= Time.deltaTime;
            float musicVolume = Mathf.Clamp(Mathf.Lerp(-50, 1, fadeTimer / fadeDurationSeconds), -50, 1);
            mixer.SetFloat("MusicVolume", musicVolume);

            if (fadeTimer <= 0)
            {
                if (restart)
                {
                    PlayLevelTheme();
                    restart = false;
                }
                else
                {
                    audioSource.Stop();
                }
            }
        }
        else if (audioSource.isPlaying && audioSource.time >= cutOffPointSeconds)
        {
            fadeTimer = fadeDurationSeconds;
            restart = true;
        }
    }
    public void PlayLevelTheme()
    {
        mixer.SetFloat("MusicVolume", 1);
        fadeTimer = 0;

        audioSource.clip = levelTheme;
        audioSource.Play();
    }

    public void StopMusic()
    {
        fadeTimer = fadeDurationSeconds;
    }

    public void SetCutoff(float seconds)
    {
        cutOffPointSeconds = seconds;
    }

    public void SpecialVolume()
    {
        musicAmplifyAnim.TryCancel();
        musicAmplifyAnim = LMotion.Create(audioSource.volume, specialAudioVolume, specialAmplifyDuration)
            .Bind(audioSource, (x, source) => source.volume = x);
    }

    public void DefaultVolume()
    {
        musicAmplifyAnim.TryCancel();
        musicAmplifyAnim = LMotion.Create(audioSource.volume, defaultAudioVolume, specialAmplifyDuration * 2)
    .Bind(audioSource, (x, source) => source.volume = x);
    }
}
