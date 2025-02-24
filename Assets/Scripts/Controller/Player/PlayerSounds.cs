using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip mainShootSound;

    [SerializeField] private int footstepInterval;
    private int footstepCounter;

    [SerializeField] private AudioClip fireHurtClip;
    [SerializeField] private AudioClip electricityHurtClip;

    private void Start()
    {
        ResetFootsteps();
    }

    public void PlayLandClip(Ground ground)
    {
        if (ground.LandClip != null)
        {
            audioSource.PlayOneShot(ground.LandClip);
        }
        footstepCounter = 0;
    }
    public void PlayFootsteps(Ground ground)
    {
        footstepCounter++;
        if (footstepCounter >= footstepInterval)
        {
            footstepCounter = 0;
            audioSource.PlayOneShot(ground.FootstepClip);
        }
    }

    public void PlayMainShoot()
    {
        audioSource.PlayOneShot(mainShootSound);
    }
    
    public void PlayHurt(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Fire:
                audioSource.PlayOneShot(fireHurtClip);
                break;
            case DamageType.Electricity:
                audioSource.PlayOneShot(electricityHurtClip);
                break;
        }
    }

    public void ResetFootsteps()
    {
        footstepCounter = footstepInterval / 2;
    }
}
