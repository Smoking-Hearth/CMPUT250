using LitMotion;
using UnityEngine;
using UnityEngine.UI;

// I hate this so much. I yearn for the traits.
public class AnimatedBar : MonoBehaviour
{
    enum State {
        Stable, 
        Approach,
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


    private float intensity = 0.03f;
    private float lagEdge = 1f;
    [SerializeField] private float leadEdge = 1f;
    public float Current 
    {
        get { return leadEdge;}
        set 
        {
            leadEdge = Mathf.Clamp(value, minValue, maxValue);
        }
    }

    [Header("Animation")]
    [SerializeField] private float duration = 0.7f;
    [SerializeField] private float intensityDecreasePoint = 0.3f;

    // This is where the display shows the health to be at.
    private MotionHandle lagEdgeAnim = MotionHandle.None;
    private MotionHandle intensityAnim = MotionHandle.None;

    private void Awake()
    {
        leadEdge = Mathf.Clamp(leadEdge, minValue, maxValue);
        lagEdge = leadEdge;
        state = State.Stable;
        UpdateUniforms();
    }

    private void UpdateUniforms()
    {
        float lagEdgeNorm = lagEdge / (maxValue - minValue);
        float leadEdgeNorm = leadEdge / (maxValue - minValue);
        float lo = Mathf.Min(lagEdgeNorm, leadEdgeNorm);
        float hi = Mathf.Max(lagEdgeNorm, leadEdgeNorm);
        height.material.SetFloat("_Lo", lo);
        height.material.SetFloat("_Hi", hi);
        height.material.SetFloat("_Intensity", intensity);
    }

    private void StableUpdate()
    {
        if (Mathf.Abs(lagEdge - leadEdge) < 1e-3) return;

        state = State.Approach;
        
        intensityAnim = LMotion.Create(0.3f, 0.03f, duration * 1.1f)
            .WithEase(Ease.Linear)
            .Bind(val => intensity = val);

        lagEdgeAnim = LMotion.Create(lagEdge, leadEdge, duration)
            .WithEase(Ease.Linear)
            .Bind(val => lagEdge = val);
    }

    public void Update()
    {
        switch (state)
        {
            case State.Stable:
                StableUpdate();
                break;

            case State.Approach:
                UpdateUniforms();
                if (lagEdgeAnim.IsPlaying() || intensityAnim.IsPlaying()) return;
                state = State.Stable;
                break;
        }
    }
}