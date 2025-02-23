using UnityEngine;

public class FireSpreader : MonoBehaviour
{
    [SerializeField] private float interval;
    [SerializeField] private float temperatureSpread;
    [SerializeField] private float radius;
    [SerializeField] private LayerMask fireLayer;
    private float spreadTimer;

    void FixedUpdate()
    {
        if (interval <= 0)
        {
            return;
        }
        if (spreadTimer > 0)
        {
            spreadTimer -= Time.fixedDeltaTime;
        }
        else
        {
            spreadTimer = interval;
        }
    }

    public void SpreadTemperature()
    {
        Collider2D[] fires = Physics2D.OverlapCircleAll(transform.position, radius, fireLayer);
        for (int i = 0; i < fires.Length; i++)
        {
            fires[i].GetComponentInParent<Combustible>().Temperature += temperatureSpread;
        }
    }
}
