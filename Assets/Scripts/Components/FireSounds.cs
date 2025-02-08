using UnityEngine;

public class FireSounds : MonoBehaviour
{
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip hitClip;
    [SerializeField] protected AudioClip extinguishClip;
    static protected bool playedHit;
    static protected bool playedExtinguish;
    private void FixedUpdate()
    {
        playedHit = false;
        playedExtinguish = false;
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
        if (!playedExtinguish)
        {
            audioSource.PlayOneShot(extinguishClip);
            playedExtinguish = true;
        }
    }
}
