using UnityEngine;

public class EnemyHealth : Health
{
    [SerializeField] private float blinkDuration;
    private float blinkTimer;
    [SerializeField] private ParticleSystem hurtParticles;
    [SerializeField] private SpriteRenderer[] blinkRenderers;
    [SerializeField] private int blinkFrequency;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color blinkColor;

    private void FixedUpdate()
    {
        if (blinkTimer > 0)
        {
            blinkTimer -= Time.fixedDeltaTime;
            if (blinkTimer <= 0)
            {
                HurtBlink(1);
            }
            else
            {
                float time = (Mathf.Cos(blinkTimer / blinkDuration * blinkFrequency * Mathf.PI * 2) + 1) * 0.5f;
                HurtBlink(time);
            }
        }
    }

    public void Hurt(CombustibleKind extinguishClass, float amount)
    {
        Current -= amount;
        blinkTimer = blinkDuration;
        hurtParticles.Play();
    }

    private void HurtBlink(float time)
    {
        for (int i = 0; i < blinkRenderers.Length; i++)
        {
            blinkRenderers[i].color = Color.Lerp(blinkColor, normalColor, time);
        }
    }
}
