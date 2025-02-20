using UnityEngine;
using UnityEngine.UI;

public class WaterRefiller : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private int waterPerRefill;
    [SerializeField] private int refillDelayTicks;
    [SerializeField] private bool available;
    [SerializeField] private int interactResetRate;
    private int refillDelayCounter;
    private bool interacting;
    public delegate void OnWaterRefill(int amount);
    public static event OnWaterRefill onWaterRefill;

    private void Start()
    {
        progressBar.maxValue = refillDelayTicks;
        progressBar.value = 0;
        progressBar.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (refillDelayCounter > 0)
        {
            if (!interacting)
            {
                refillDelayCounter -= interactResetRate;

                if (refillDelayCounter <= 0)
                {
                    refillDelayCounter = 0;
                    progressBar.gameObject.SetActive(false);
                }
            }

            if (progressBar.gameObject.activeSelf)
            {
                progressBar.value = refillDelayCounter;
            }
        }
    }

    public void StartRefill()
    {
        interacting = true;
    }
    public void Refill()
    {
        if (!interacting)
        {
            return;
        }

        if (!progressBar.gameObject.activeSelf)
        {
            progressBar.gameObject.SetActive(true);
        }

        if (refillDelayCounter < refillDelayTicks)
        {
            refillDelayCounter++;
            return;
        }

        refillDelayCounter = 0;
        
        if (onWaterRefill != null)
        {
            onWaterRefill(waterPerRefill);
        }
    }
    public void StopRefill()
    {
        interacting = false;
    }
}
