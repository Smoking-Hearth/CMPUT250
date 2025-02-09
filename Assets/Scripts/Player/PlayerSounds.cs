using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip mainShootSound;

    [SerializeField] private int footstepInterval;
    private int footstepCounter;
    [SerializeField] private AudioClip grassFootsteps;

    [SerializeField] private AudioClip fireHurtClip;
    [SerializeField] private AudioClip electricityHurtClip;

    private void Start()
    {
        ResetFootsteps();
    }
    public void PlayMainShoot()
    {
        audioSource.PlayOneShot(mainShootSound);
    }

    public void PlayGrassFootsteps()
    {
        footstepCounter++;

        if (footstepCounter >= footstepInterval)
        {
            footstepCounter = 0;
            audioSource.PlayOneShot(grassFootsteps);
        }
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
