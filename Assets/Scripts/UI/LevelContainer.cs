using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LitMotion;
using LitMotion.Extensions;

public class LevelContainer: MonoBehaviour
{
    [field: SerializeField] private RectTransform canvasRectTransform;
    [field: SerializeField] public Button button { get; private set; }
    [SerializeField] public RawImage previewImage;
    private RectTransform previewRectTransform;
    private RectTransform myRectTransform;

    [field: SerializeField] public SceneIndex levelIndex { get; private set; }
    private RenderTexture preview;
    private bool previewLoaded = false;

    void Start()
    {
        StartCoroutine(GameManager.SceneSystem.Load(levelIndex));
        previewRectTransform = previewImage.GetComponent<RectTransform>();
        myRectTransform = GetComponent<RectTransform>();
        if (previewRectTransform == null)
        {
            DevLog.Error("LevelContainer should be attached to a UI Object");
        }

        button.onClick.AddListener(ContainerClicked);
    }

    void ContainerClicked()
    {
        gameObject.transform.SetAsLastSibling();

        LMotion.Create(myRectTransform.anchoredPosition, Vector2.zero, 1f)
            .WithEase(Ease.OutSine)
            .BindToAnchoredPosition(myRectTransform);
        
        LMotion.Create(myRectTransform.sizeDelta, canvasRectTransform.rect.size, 1f)
            .WithEase(Ease.OutSine)
            .BindToSizeDelta(myRectTransform);
    }

    void Update()
    {
        if (!gameObject.ShouldUpdate()) return;
        
        if (!previewLoaded && GameManager.SceneSystem.IsLoaded(levelIndex))
        {
            LevelManager levelManager = GameManager.SceneSystem.LevelManagers[(int)levelIndex];
            
            Vector2 size = previewRectTransform.rect.size;
            preview = new RenderTexture(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y), 16, RenderTextureFormat.Default);
            levelManager.LevelCamera.targetTexture = preview;
            previewImage.texture = preview;

            levelManager.LevelCamera.Render();
            previewLoaded = true;
        }
    }
}