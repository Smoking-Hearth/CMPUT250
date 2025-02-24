using UnityEngine;
using System.Collections.Generic;

public class WaterArea : MonoBehaviour
{
    [SerializeField] private ParticleSystem enterParticles;
    [SerializeField] private ParticleSystem stayParticles;
    [SerializeField] private AudioSource enterAudio;
    [SerializeField] private AudioSource stayAudio;
    [SerializeField] private LayerMask extinguishLayers;
    private List<IExtinguishable> extinguishables = new List<IExtinguishable>();
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

        for (int i = 0; i < extinguishables.Count; i++)
        {
            extinguishables[i].Extinguish(CombustibleKind.A_COMMON, 10);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        enterParticles.transform.position = collision.transform.position;
        enterParticles.Play();

        if (enterAudio != null)
        {
            enterAudio.PlayOneShot(enterAudio.clip);
        }

        IExtinguishable extinguishable = null;
        collision.TryGetComponent(out extinguishable);
        if (extinguishable != null && !extinguishables.Contains(extinguishable))
        {
            extinguishables.Add(extinguishable);
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

    private void OnTriggerExit2D(Collider2D collision)
    {
        IExtinguishable extinguishable = null;
        collision.TryGetComponent(out extinguishable);
        if (extinguishable != null && extinguishables.Contains(extinguishable))
        {
            extinguishables.Remove(extinguishable);
        }
    }
}
