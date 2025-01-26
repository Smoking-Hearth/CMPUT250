using UnityEngine;

public class WaterRefiller : MonoBehaviour
{
    [SerializeField] private int refillPerTick;
    [SerializeField] private int refillDelayTicks;
    private int refillDelayCounter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int Refill()
    {
        if (refillDelayCounter < refillDelayTicks)
        {
            refillDelayCounter++;
            return 0;
        }

        refillDelayCounter = 0;
        return refillPerTick * refillDelayTicks;
    }
}
