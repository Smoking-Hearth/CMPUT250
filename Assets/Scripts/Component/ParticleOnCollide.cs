using UnityEngine;

public class ParticleOnCollide : MonoBehaviour
{
    [SerializeField] private ParticleSystem enterParticles;
    [SerializeField] private ParticleSystem stayParticles;
    [SerializeField] private AudioSource enterAudio;
    [SerializeField] private AudioSource stayAudio;
    private bool staying;

    private void FixedUpdate()
    {
        if (staying)
        {
            staying = false;
        }
        else if (stayAudio != null)
        {
            stayAudio.Stop();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        enterParticles.transform.position = collision.transform.position;
        enterParticles.Play();

        if (enterAudio != null)
        {
            enterAudio.Play();
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        stayParticles.transform.position = collision.transform.position;
        stayParticles.Play();
        staying = true;

        if (stayAudio == null)
        {
            return;
        }
        if (!stayAudio.isPlaying)
        {
            stayAudio.Play();
        }
    }
}
