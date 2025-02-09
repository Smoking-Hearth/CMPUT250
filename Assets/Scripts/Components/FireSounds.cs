using UnityEngine;

public class FireSounds : MonoBehaviour
{
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip hitClip;
    [SerializeField] protected AudioClip extinguishClip;
    static protected bool playedHit;
    protected float hitTimer;
    static protected bool playedExtinguish;
    private void FixedUpdate()
    {
        playedHit = false;
        playedExtinguish = false;
        if (hitTimer > 0)
        {
            hitTimer -= Time.fixedDeltaTime;
        }
    }

    public void HitSound()
    {
        if (hitTimer <= 0 && !playedHit)
        {
            audioSource.PlayOneShot(hitClip);
            hitTimer = 0.2f;
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
