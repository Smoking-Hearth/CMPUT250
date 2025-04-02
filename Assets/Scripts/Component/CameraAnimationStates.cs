using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CameraAnimationStates : MonoBehaviour
{
    private Animator cameraAnimator;

    private void Awake()
    {
        cameraAnimator = GetComponent<Animator>();
    }

    public void ToggleBool(string parameterName)
    {
        cameraAnimator.SetBool(parameterName, !cameraAnimator.GetBool(parameterName));
    }
}
