using UnityEngine;
using LitMotion;

public class FireSounds : MonoBehaviour
{
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioSource ambientAudio;    //The audio that the enemy plays just for existing;
    [SerializeField] protected float ambientVolume;
    [SerializeField] protected AudioClip hitClip;
    [SerializeField] protected AudioClip extinguishClip;
    static protected float globalHitTimer;
    protected float hitTimer;
    static protected bool playedExtinguish;

    private void OnEnable()
    {
        if (ambientAudio == null)
        {
            return;
        }
        ambientAudio.volume = ambientVolume;
    }
    private void FixedUpdate()
    {
        playedExtinguish = false;
        if (hitTimer > 0)
        {
            hitTimer -= Time.fixedDeltaTime;
        }
    }

    public static void UpdateGlobalHitTimer()
    {
        if (globalHitTimer > 0)
        {
            globalHitTimer -= Time.fixedDeltaTime;
        }
    }

    public void HitSound()
    {
        if (hitTimer <= 0 && globalHitTimer <= 0)
        {
            audioSource.pitch = 1;
            audioSource.PlayOneShot(hitClip);
            globalHitTimer = 0.1f;
            hitTimer = 0.12f;
        }
    }
    public void ExtinguishSound()
    {
        if (!playedExtinguish)
        {
            audioSource.pitch = 1;
            audioSource.PlayOneShot(extinguishClip);
            playedExtinguish = true;
        }
    }

    public void FadeAmbientSounds(float fadeDuration)
    {
        if (ambientAudio == null)
        {
            return;
        }
        LMotion.Create(ambientAudio.volume, 0, fadeDuration)
            .WithEase(Ease.InCubic)
            .Bind(ambientAudio, (x, audio) => audio.volume = x);
    }
    public void EnableAmbientSounds(float fadeDuration)
    {
        if (ambientAudio == null)
        {
            return;
        }
        LMotion.Create(ambientAudio.volume, ambientVolume, fadeDuration)
            .WithEase(Ease.InCubic)
            .Bind(ambientAudio, (x, audio) => audio.volume = x);
    }
}
