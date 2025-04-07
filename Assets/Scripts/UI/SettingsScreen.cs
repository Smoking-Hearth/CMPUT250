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
    }

    void OnDisable()
    {
        back.onClick.AddListener(BackClick);
        masterVolume.onValueChanged.RemoveListener(MasterVolume);
        musicVolume.onValueChanged.RemoveListener(MusicVolume);
        sfxVolume.onValueChanged.RemoveListener(SFXVolume);
        ambientVolume.onValueChanged.RemoveListener(AmbientVolume);
    }

    void BackClick()
    {
        gameObject.SetActive(false);
    }

    private static float LinearToDB(float ratio)
    {
        ratio = Mathf.Clamp(ratio, 0f, 1f);
        ratio = 10f * Mathf.Log10(ratio);
        return Mathf.Clamp(ratio, -80f, 0f);
    }

    public void MasterVolume(float value)
    {
        float volume = LinearToDB(value);
        mixer.SetFloat("MasterVolume", volume);
    }

    public void MusicVolume(float value)
    {
        float volume = LinearToDB(value);
        mixer.SetFloat("MusicVolume", volume);
    }

    public void SFXVolume(float value)
    {
        float volume = LinearToDB(value);
        mixer.SetFloat("SFXVolume", volume);
    }

    public void AmbientVolume(float value)
    {
        float volume = LinearToDB(value);
        mixer.SetFloat("SFXVolume", volume);
    }
}
