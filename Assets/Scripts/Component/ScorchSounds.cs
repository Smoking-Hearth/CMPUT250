using UnityEngine;

public class ScorchSounds : FireSounds
{
    private bool step;
    [SerializeField] private AudioClip[] stepClips;
    [SerializeField] private AudioClip windupClip;
    [SerializeField] private AudioClip jumpClip;

    public void PlayStep()
    {
        if (step)
        {
            audioSource.PlayOneShot(stepClips[0]);
        }
        else
        {
            audioSource.PlayOneShot(stepClips[1]);
        }

        step = !step;
    }
    
    public void PlayWindup()
    {
        audioSource.PlayOneShot(windupClip);
    }

    public void PlayJump()
    {
        audioSource.PlayOneShot(jumpClip);
    }
}
