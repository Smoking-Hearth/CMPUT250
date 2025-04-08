using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using LitMotion;
using LitMotion.Extensions;

public class FadeScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup screen;
    private MotionHandle anim = MotionHandle.None;
    [SerializeField] private UnityEvent endEvent;
    private bool played;

    private void FixedUpdate()
    {
        if (played && !anim.IsPlaying())
        {
            endEvent.Invoke();
        }
    }

    public void FadeIn(float duration)
    {
        played = true;
        anim = LMotion.Create(screen.alpha, 1, duration)
            .BindToAlpha(screen);
    }
    public void FadeOut(float duration)
    {
        played = true;
        anim = LMotion.Create(screen.alpha, 0, duration)
            .BindToAlpha(screen);
    }
}
