using UnityEngine;
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

    private MotionHandle anim = MotionHandle.None;

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

        MotionHandle center = LMotion.Create(myRectTransform.anchoredPosition, Vector2.zero, 1f)
            .WithEase(Ease.OutSine)
            .BindToAnchoredPosition(myRectTransform);

        MotionHandle grow = LMotion.Create(myRectTransform.sizeDelta, canvasRectTransform.rect.size, 1f)
            .WithEase(Ease.OutSine)
            .BindToSizeDelta(myRectTransform);

        anim = LSequence.Create()
            .Join(center)
            .Join(grow)
            .Run();
    }

    void Update()
    {
        if (!gameObject.ShouldUpdate()) return;

        if (!previewLoaded && GameManager.SceneSystem.IsLoaded(levelIndex))
        {
            LevelManager levelManager = GameManager.SceneSystem.LevelManagers[(int)levelIndex];
            
            Vector2 size = previewRectTransform.rect.size;
            preview = new RenderTexture(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y), 16, RenderTextureFormat.Default);
            preview.filterMode = FilterMode.Point;
            preview.autoGenerateMips = false;

            levelManager.LevelCamera.targetTexture = preview;
            previewImage.texture = preview;

            levelManager.LevelCamera.Render();
            previewLoaded = true;
        }

        if (anim != MotionHandle.None && anim.IsPlaying())
        {
            preview.Release();
            Vector2 size = previewRectTransform.rect.size;
            preview.width = (int)size.x;
            preview.height = (int)size.y;
            preview.Create();
        }
    }
}