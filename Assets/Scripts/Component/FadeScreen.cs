using UnityEngine;
using UnityEngine.UI;
using LitMotion;

public class FadeScreen : MonoBehaviour
{
    [SerializeField] private Image screen;
    [SerializeField] private Color screenColor;
    private MotionHandle anim = MotionHandle.None;
    [SerializeField] private bool activate;
    private float currentDuration;

    private void Update()
    {
        if (activate)
        {
            FadeIn(currentDuration);
        }
        else
        {
            FadeOut(currentDuration);
        }
    }

    public void FadeIn(float duration)
    {
        activate = true;
        currentDuration = duration;
        anim = LMotion.Create(screen.color, screenColor, duration)
            .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
            .Bind(screen.color, (fade, x) => fade = x);
    }
    public void FadeOut(float duration)
    {
        activate = false;
        currentDuration = duration;
        anim = LMotion.Create(screen.color, Color.clear, duration)
            .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
            .Bind(screen.color, (fade, x) => fade = x);
    }
}
