using UnityEngine;

public class EnemySounds : FireSounds
{
    [SerializeField] protected AudioClip stepClip;
    [SerializeField] protected AudioClip impactClip;

    public void StepSound()
    {
        audioSource.pitch = Random.Range(0.8f, 1.2f);
        audioSource.PlayOneShot(stepClip);
    }

    public void ImpactSound()
    {
        audioSource.pitch = Random.Range(0.8f, 1.2f);
        audioSource.PlayOneShot(impactClip);
    }
}
