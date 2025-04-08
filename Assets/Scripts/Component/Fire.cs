using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Fire : MonoBehaviour
{
    public EnemyAttackInfo damageInfo;
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

    public virtual void SetActive(bool set)
    {
        activated = set;
        fireLight.enabled = set;
        if (set)
        {
            particles.Play();
            sounds.EnableAmbientSounds(0.5f);
        }
        else
        {
            particles.Stop();
            sounds.FadeAmbientSounds(0.5f);
        }
    }

    private void FixedUpdate()
    {
        if (sounds.playingAmbient)
        {
            if ((gameObject.MyLevelManager().Player.Position - (Vector2)transform.position).magnitude > Combustible.SIMULATION_DISTANCE)
            {
                sounds.FadeAmbientSounds(0.5f);
            }
        }
        else if (FireSounds.ambientSounds < 10)
        {
            if ((gameObject.MyLevelManager().Player.Position - (Vector2)transform.position).magnitude <= Combustible.SIMULATION_DISTANCE)
            {
                sounds.EnableAmbientSounds(0.5f);
            }
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
