using UnityEngine;
using UnityEngine.Audio;

static class VolumeSettings
{
    public static readonly string MasterVolumeKey = "MasterVolume";
    public static float MasterVolume 
    {
        set { PlayerPrefs.SetFloat(MasterVolumeKey, value); }
        get { return PlayerPrefs.GetFloat(MasterVolumeKey, 1f); }
    }

    public static readonly string MusicVolumeKey = "MusicVolume";
    public static float MusicVolume
    {
        set { PlayerPrefs.SetFloat(MusicVolumeKey, value); }
        get { return PlayerPrefs.GetFloat(MusicVolumeKey, 1f); }
    }

    public static readonly string SFXVolumeKey = "SFXVolume";
    public static float SFXVolume 
    {
        set { PlayerPrefs.SetFloat(SFXVolumeKey, value); }
        get { return PlayerPrefs.GetFloat(SFXVolumeKey, 1f); }
    }

    public static readonly string AmbientVolumeKey = "AmbientVolume";
    public static float AmbientVolume 
    {
        set { PlayerPrefs.SetFloat(AmbientVolumeKey, value); }
        get { return PlayerPrefs.GetFloat(AmbientVolumeKey, 1f); }
    }

    private static float LinearToDB(float ratio)
    {
        ratio = Mathf.Clamp(ratio, 0f, 1f);
        ratio = 10f * Mathf.Log10(ratio);
        return Mathf.Clamp(ratio, -80f, 0f);
    }

    public static void ApplySettings(AudioMixer mixer)
    {
        mixer.SetFloat("MasterVolume", LinearToDB(MasterVolume));
        mixer.SetFloat("MusicVolume", LinearToDB(MusicVolume));
        mixer.SetFloat("SFXVolume", LinearToDB(SFXVolume));
        mixer.SetFloat("AmbientVolume", LinearToDB(AmbientVolume));
    }
}