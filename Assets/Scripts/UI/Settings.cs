using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour
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
        StartCoroutine(Unquenchable.SceneManager.SetSceneActive(SceneIndex.MainMenu));
    }

    void VolumeChanged(float value)
    {
        mixer.SetFloat("Master", value);
    }
}
