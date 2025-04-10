using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsScreen : MonoBehaviour
{
    [SerializeField] Button back;
    [SerializeField] AudioMixer mixer;
    [SerializeField] Slider masterVolume;
    [SerializeField] Slider musicVolume;
    [SerializeField] Slider sfxVolume;
    [SerializeField] Slider ambientVolume;

    void OnEnable()
    {
        back.onClick.AddListener(BackClick);
        masterVolume.onValueChanged.AddListener(MasterVolume);
        musicVolume.onValueChanged.AddListener(MusicVolume);
        sfxVolume.onValueChanged.AddListener(SFXVolume);
        ambientVolume.onValueChanged.AddListener(AmbientVolume);

        masterVolume.value = VolumeSettings.MasterVolume;
        musicVolume.value = VolumeSettings.MusicVolume;
        sfxVolume.value = VolumeSettings.SFXVolume;
        ambientVolume.value = VolumeSettings.AmbientVolume;
    }

    void OnDisable()
    {
        back.onClick.AddListener(BackClick);
        masterVolume.onValueChanged.RemoveListener(MasterVolume);
        musicVolume.onValueChanged.RemoveListener(MusicVolume);
        sfxVolume.onValueChanged.RemoveListener(SFXVolume);
        ambientVolume.onValueChanged.RemoveListener(AmbientVolume);
        VolumeSettings.ApplySettings(mixer);
    }

    void BackClick()
    {
        gameObject.SetActive(false);
    }


    public void MasterVolume(float value)
    {
        VolumeSettings.MasterVolume = value;
        VolumeSettings.ApplySettings(mixer);
    }

    public void MusicVolume(float value)
    {
        VolumeSettings.MusicVolume = value;
        VolumeSettings.ApplySettings(mixer);
    }

    public void SFXVolume(float value)
    {
        VolumeSettings.SFXVolume = value;
        VolumeSettings.ApplySettings(mixer);
    }

    public void AmbientVolume(float value)
    {
        VolumeSettings.AmbientVolume = value;
        VolumeSettings.ApplySettings(mixer);
    }
}
