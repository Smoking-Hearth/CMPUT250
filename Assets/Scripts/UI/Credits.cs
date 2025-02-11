using UnityEngine;
using UnityEngine.UI;

public class Credits : MonoBehaviour
{
    [SerializeField] Button back;

    void OnEnable()
    {
        back.onClick.AddListener(OnBackClick);
    }

    void OnDisable()
    {
        back.onClick.RemoveListener(OnBackClick);
    }

    void OnBackClick()
    {
        StartCoroutine(Unquenchable.SceneManagerWrapper.SetSceneActive(SceneIndex.MainMenu));
    }
}