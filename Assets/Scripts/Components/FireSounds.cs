using UnityEngine;

public class FireSounds : MonoBehaviour
{
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip hitClip;
    [SerializeField] protected AudioClip extinguishClip;
    protected bool playedHit;
    private void FixedUpdate()
    {
        playedHit = false;
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
