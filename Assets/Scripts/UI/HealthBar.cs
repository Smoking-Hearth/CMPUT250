using LitMotion;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private float min = 0f;
    public float Min
    {
        get { return min; }
        set { min = value; }
    }


    private float max = 0f;
    public float Max 
    {
        get { return max; }
        set { max = value; }
    }

    private float current = 0f;
    public float Current 
    {
        get { return current;}
        set 
        {
            current = value;

            anim.TryCancel();
            anim = LMotion.Create(slider.value, current, 0.2f)
            .WithEase(Ease.OutQuad)
                .Bind(val => { slider.value = val; });
        }
    }

    [SerializeField] private Slider slider;

    // This is where the display shows the health to be at.
    private MotionHandle anim = MotionHandle.None;

    public void Update()
    {
        

    }
}