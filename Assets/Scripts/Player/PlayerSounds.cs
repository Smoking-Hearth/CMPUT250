using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip mainShootSound;

    public void PlayMainShoot()
    {
        audioSource.PlayOneShot(mainShootSound);
    }
}
