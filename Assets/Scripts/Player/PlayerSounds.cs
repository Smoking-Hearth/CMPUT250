using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip mainShootSound;

    [SerializeField] private int footstepInterval;
    [SerializeField] private int footstepCounter;
    [SerializeField] private AudioClip grassFootsteps;

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

    public void ResetFootsteps()
    {
        footstepCounter = footstepInterval / 2;
    }
}
