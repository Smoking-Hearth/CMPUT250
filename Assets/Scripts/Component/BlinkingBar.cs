using UnityEngine;
using UnityEngine.UI;

public class BlinkingBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Image fill;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color[] blinkingColors = { Color.white };
    [SerializeField] private float blinkIntervalSeconds;
    private float blinkTimer;
    private int blinkIndex;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (slider.value < slider.maxValue)
        {
            fill.color = normalColor;
            return;
        }
        if (blinkTimer > 0)
        {
            blinkTimer -= Time.fixedDeltaTime;
        }
        else
        {
            blinkTimer = blinkIntervalSeconds;
            blinkIndex = (blinkIndex + 1) % blinkingColors.Length;
            fill.color = blinkingColors[blinkIndex];
        }
    }
}
