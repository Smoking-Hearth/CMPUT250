using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Fire : MonoBehaviour
{
    [SerializeField] protected ParticleSystem particles;
    [SerializeField] protected Light2D fireLight;
    protected bool activated;
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip hitClip;
    [SerializeField] protected AudioClip extinguishClip;
    protected bool playedHit;

    public bool IsActivated
    {
        get
        {
            return activated;
        }
    }

    protected void FixedUpdate()
    {
        playedHit = false;
    }

    public void Initialize(FireInfo info)
    {
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.color = info.gradient;

        ParticleSystem.TrailModule trails = particles.trails;
        trails.colorOverTrail = info.gradient;

        fireLight.color = info.lightColour;
        fireLight.intensity = info.lightIntensity;
    }

    public void SetActive(bool set)
    {
        activated = set;
        fireLight.enabled = set;
        if (set)
        {
            particles.Play();
        }
        else
        {
            particles.Stop();
        }
    }

    public void SetLifetime(float particleLifetime)
    {
        ParticleSystem.MainModule main = particles.main;
        main.startLifetime = particleLifetime;
    }

    public void HitSound()
    {
        if (!playedHit)
        {
            audioSource.PlayOneShot(hitClip);
            playedHit = true;
        }
    }
    public void ExtinguishSound()
    {
        audioSource.PlayOneShot(extinguishClip);
    }
}
