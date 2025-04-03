using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip mainShootSound;

    [SerializeField] private AudioClip fireHurtClip;
    [SerializeField] private AudioClip electricityHurtClip;
    [SerializeField] private AudioClip jumpClip;

    public Ground currentGround;


    public void PlayLandClip(Ground ground)
    {
        if (ground.LandClip != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(ground.LandClip);
        }
    }
    public void PlayFootsteps()
    {
        if (currentGround != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(currentGround.FootstepClip);
        }
    }

    public void PlayMainShoot()
    {
        audioSource.pitch = 1;
        audioSource.PlayOneShot(mainShootSound);
    }
    
    public void PlayHurt(DamageType damageType)
    {
        audioSource.pitch = 1;
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

    public void PlayJump()
    {
        audioSource.pitch = 1;
        audioSource.PlayOneShot(jumpClip);
    }
}
