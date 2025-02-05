using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] Slider volume;

    void OnEnable()
    {
        volume.onValueChanged.AddListener(VolumeChanged);
    }

    void OnDisable()
    {
        volume.onValueChanged.AddListener(VolumeChanged);
    }

    void VolumeChanged(float value)
    {
        // TODO store value and apply it to a mixer.
    }
}
