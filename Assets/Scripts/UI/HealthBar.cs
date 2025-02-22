using LitMotion;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image height;

    [SerializeField] private float minValue = 0f;
    public float Min
    {
        get { return minValue; }
        set { minValue = value; }
    }


    [SerializeField] private float maxValue = 1f;
    public float Max 
    {
        get { return maxValue; }
        set { maxValue = value; }
    }


    private float barPosition = 1f;

    private float lossIntensity = 0.2f;

    [SerializeField] private float current = 1f;
    public float Current 
    {
        get { return current;}
        set 
        {
            if (anim.IsPlaying()) return;

            current = Mathf.Clamp(value, minValue, maxValue);

            const float duration = 0.4f;
            const float intensityDecreasePoint = duration / 2f;

            MotionHandle changeHeight = LMotion.Create(barPosition, current, duration)
                .WithEase(Ease.OutQuad)
                .Bind(val => barPosition = val);

            MotionHandle increaseIntensity = LMotion.Create(lossIntensity, 1f, 0.2f)
                .Bind(val => lossIntensity = val);

            MotionHandle reduceIntensity = LMotion.Create(lossIntensity, 0.2f, 0.2f)
                .Bind(val => lossIntensity = val);

            anim = LSequence.Create()
                .Join(increaseIntensity)
                .Append(reduceIntensity)
                .Insert(0f, changeHeight)
                .Run();
        }
    }

    // This is where the display shows the health to be at.
    private MotionHandle anim = MotionHandle.None;

    private void Awake()
    {
        current = Mathf.Clamp(current, minValue, maxValue);
        barPosition = current;
    }

    public void Update()
    {
        barPosition = Mathf.Clamp(barPosition, minValue, maxValue);
        float normalizedPosition = barPosition / (maxValue - minValue);

        height.material.SetFloat("_BarPosition", normalizedPosition);
        height.material.SetFloat("_Intensity", lossIntensity);
    }
}