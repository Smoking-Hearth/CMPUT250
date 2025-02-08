using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Fire : MonoBehaviour
{
    [SerializeField] protected ParticleSystem particles;
    [SerializeField] protected Light2D fireLight;
    [SerializeField] protected FireSounds sounds;
    protected bool activated;

    public bool IsActivated
    {
        get
        {
            return activated;
        }
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
        sounds.HitSound();
    }
    public void ExtinguishSound()
    {
        sounds.ExtinguishSound();
    }
}
