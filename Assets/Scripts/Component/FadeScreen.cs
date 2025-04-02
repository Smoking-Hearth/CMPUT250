using UnityEngine;
using UnityEngine.UI;
using LitMotion;
using LitMotion.Extensions;

public class FadeScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup screen;
    private MotionHandle anim = MotionHandle.None;

    private void Update()
    {

    }

    public void FadeIn(float duration)
    {
        anim = LMotion.Create(screen.alpha, 1, duration)
            .BindToAlpha(screen);
    }
    public void FadeOut(float duration)
    {
        anim = LMotion.Create(screen.alpha, 0, duration)
            .BindToAlpha(screen);
    }
}
