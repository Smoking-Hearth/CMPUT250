using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsScreen : MonoBehaviour
{
    [SerializeField] Slider volume;
    [SerializeField] Button back;
    [SerializeField] AudioMixer mixer;

    void OnEnable()
    {
        volume.onValueChanged.AddListener(VolumeChanged);
        back.onClick.AddListener(BackClick);
    }

    void OnDisable()
    {
        volume.onValueChanged.AddListener(VolumeChanged);
        back.onClick.AddListener(BackClick);
    }

    void BackClick()
    {
        gameObject.SetActive(false);
    }

    void VolumeChanged(float value)
    {
        mixer.SetFloat("MasterVolume", value);
    }
}
