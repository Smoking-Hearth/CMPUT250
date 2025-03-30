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
        VolumeChanged(volume.value);
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
        if (value == 0)
        {
            mixer.SetFloat("MasterVolume", -80);
            return;
        }

        float volume = Mathf.Lerp(-20, 10, Mathf.Log10(value + 1));
        mixer.SetFloat("MasterVolume", volume);
    }
}
