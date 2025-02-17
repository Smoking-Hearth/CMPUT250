using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelContainer: MonoBehaviour
{
    [field: SerializeField] public Button button { get; private set; }
    [SerializeField] public RawImage previewImage;
    private RectTransform rectTransform;

    [field: SerializeField] public SceneIndex levelIndex { get; private set; }
    private RenderTexture preview;
    private bool previewLoaded = false;

    void Start()
    {
        StartCoroutine(GameManager.SceneSystem.Load(levelIndex));
        rectTransform = previewImage.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            DevLog.Error("LevelContainer should be attached to a UI Object");
        }

        button.onClick.AddListener(ContainerClicked);
    }

    void ContainerClicked()
    {
        StartCoroutine(GameManager.SceneSystem.SetSceneActive(levelIndex));
    }

    void Update()
    {
        if (!gameObject.ShouldUpdate()) return;
        
        if (!previewLoaded && GameManager.SceneSystem.IsLoaded(levelIndex))
        {
            LevelManager levelManager = GameManager.SceneSystem.LevelManagers[(int)levelIndex];
            
            Vector2 size = rectTransform.rect.size;
            preview = new RenderTexture(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y), 16, RenderTextureFormat.Default);
            levelManager.LevelCamera.targetTexture = preview;
            previewImage.texture = preview;

            levelManager.LevelCamera.Render();
            previewLoaded = true;
        }
    }
}