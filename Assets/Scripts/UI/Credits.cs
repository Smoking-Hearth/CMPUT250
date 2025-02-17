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
        GameManager.Instance.StartCoroutine(GameManager.SceneSystem.SetSceneActive(SceneIndex.MainMenu));
    }
}