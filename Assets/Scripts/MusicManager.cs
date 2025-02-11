using UnityEngine;
using UnityEngine.Audio;

public enum MusicState
{
    Normal, Battle
}
public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip battleMusic;
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private float fadeDurationSeconds;
    private float fadeTimer;

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
            }
        }
    }
    public void PlayBattleMusic()
    {
        mixer.SetFloat("MusicVolume", 1);
        fadeTimer = 0;

        audioSource.clip = battleMusic;
        audioSource.Play();
    }

    public void StopMusic()
    {
        fadeTimer = fadeDurationSeconds;
    }
}
