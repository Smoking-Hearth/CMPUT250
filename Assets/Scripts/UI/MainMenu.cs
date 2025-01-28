using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Button play, settings, credits;

    void OnEnable()
    {
        play.onClick.AddListener(OnPlayClick);
        settings.onClick.AddListener(OnSettingsClick);
        credits.onClick.AddListener(OnCreditsClick);
    }

    void OnDisable()
    {
        play.onClick.RemoveListener(OnPlayClick);
        settings.onClick.RemoveListener(OnSettingsClick);
        credits.onClick.RemoveListener(OnCreditsClick);
    }

    void OnPlayClick()
    {
        // NOTE: This MainMenu object is carried over into the loaded scene.
        SceneLoader.Instance.Transition(SceneLoader.SceneMask.Forest, SceneLoader.TransitionType.Jump);
    }

    void OnSettingsClick()
    {
        SceneLoader.Instance.Transition(SceneLoader.SceneMask.Settings, SceneLoader.TransitionType.Jump);
    }

    void OnCreditsClick()
    {
        Debug.Log("TODO :)");
    }
}
