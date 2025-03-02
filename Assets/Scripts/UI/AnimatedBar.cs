using LitMotion;
using UnityEngine;
using UnityEngine.UI;

// I hate this so much. I yearn for the traits.
public class AnimatedBar : MonoBehaviour
{
    enum State {
        Stable, 
        IntensityIncreasing,
        IntensityDecreasing,
    }

    private State state = State.Stable;

    [SerializeField] private Image height;
    [SerializeField] private Slider control;

    [Header("Slider")]
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
            current = Mathf.Clamp(value, minValue, maxValue);
        }
    }

    [Header("Animation")]
    [SerializeField] private float duration = 0.7f;
    [SerializeField] private float intensityDecreasePoint = 0.3f;

    // This is where the display shows the health to be at.
    private MotionHandle barPosAnim = MotionHandle.None;
    private MotionHandle intensityAnim = MotionHandle.None;

    private void Awake()
    {
        current = Mathf.Clamp(current, minValue, maxValue);
        barPosition = current;
        state = State.Stable;
        UpdateUniforms();
    }

    private void UpdateUniforms()
    {
        float normalizedPosition = barPosition / (maxValue - minValue);
        height.material.SetFloat("_BarPosition", normalizedPosition);
        height.material.SetFloat("_Intensity", lossIntensity);
    }

    private void SetIntensity(float val)
    {
        lossIntensity = val;
    }

    private void StableUpdate()
    {
        if (Mathf.Abs(barPosition - current) < 0.02) return;
        
        state = State.IntensityIncreasing;
        
        barPosAnim = LMotion.Create(barPosition, current, duration)
            .WithEase(Ease.OutQuad)
            .Bind(val => barPosition = val);

        intensityAnim = LMotion.Create(lossIntensity, 1f, intensityDecreasePoint)
            .Bind(SetIntensity);
    }

    public void Update()
    {
        if (state != State.Stable) UpdateUniforms();

        switch (state)
        {
            case State.Stable:
                StableUpdate();
                break;

            case State.IntensityIncreasing:
                if (intensityAnim.IsPlaying()) return;
                state = State.IntensityDecreasing;
                intensityAnim = LMotion.Create(lossIntensity, 0.2f, duration - intensityDecreasePoint)
                    .Bind(SetIntensity);
                break;

            case State.IntensityDecreasing:
                if (intensityAnim.IsPlaying() || barPosAnim.IsPlaying()) return;
                state = State.Stable;
                break;
        }
    }
}