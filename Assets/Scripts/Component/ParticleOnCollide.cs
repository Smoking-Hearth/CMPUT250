using UnityEngine;

public class ParticleOnCollide : MonoBehaviour
{
    [SerializeField] private ParticleSystem enterParticles;
    [SerializeField] private ParticleSystem stayParticles;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        enterParticles.transform.position = collision.transform.position;
        enterParticles.Play();
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        stayParticles.transform.position = collision.transform.position;
        stayParticles.Play();
    }
}
