using UnityEngine;
using UnityEngine.UI;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine.EventSystems;

public class LevelContainer: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    struct CompactState
    {
        public Vector2 containerPos;
        public Vector2 containerSize;

        public CompactState(Vector2 pos, Vector2 size)
        {
            containerPos = pos;
            containerSize = size;
        }
    }
    public enum ContainerState 
    {
        Compact, PerformingHover, Hover, PerformingExpand, Expand, ExitExpand
    }
    private ContainerState containerState = ContainerState.Compact;

    [field: SerializeField] private RectTransform canvasRectTransform;
    [field: SerializeField] private CanvasGroup containerButtons;
    [field: SerializeField] public Button button { get; private set; }
    [field: SerializeField] private RectTransform labelRectTransform;


    [SerializeField] public RawImage previewImage;
    private RectTransform previewRectTransform;
    private RectTransform myRectTransform;

    [field: SerializeField] public SceneIndex levelIndex { get; private set; }
    private RenderTexture preview;
    private bool previewLoaded = false;

    private MotionHandle anim = MotionHandle.None;
    private CompactState compactState;

    void Start()
    {
        previewRectTransform = previewImage.GetComponent<RectTransform>();
        myRectTransform = GetComponent<RectTransform>();
        if (previewRectTransform == null)
        {
            DevLog.Error("LevelContainer should be attached to a UI Object");
        }
        compactState = new CompactState(myRectTransform.anchoredPosition, myRectTransform.sizeDelta);
        button.onClick.AddListener(ContainerClicked);

        gameObject.MyLevelManager().onActivate += Activate;
        gameObject.MyLevelManager().onDeactivate += Deactivate;
    }
    
    void Activate()
    {
        // We have entered/re-entered make sure the contained scene is loaded
        StartCoroutine(GameManager.SceneSystem.Preload(levelIndex));
        previewLoaded = false;
    }
    
    public void Deactivate()
    {
        GameManager.SceneSystem.LevelManagers[(int)levelIndex].LevelCamera.targetTexture = null;
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (containerState != ContainerState.Compact) return;
        anim = LMotion.Create(myRectTransform.sizeDelta, compactState.containerSize * 1.5f, 0.4f)
            .WithEase(Ease.InOutSine)
            .BindToSizeDelta(myRectTransform);
        containerState = ContainerState.PerformingHover;
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if (containerState != ContainerState.Hover && containerState != ContainerState.PerformingHover) return;
        anim = LMotion.Create(myRectTransform.sizeDelta, compactState.containerSize, 0.4f)
            .WithEase(Ease.InOutSine)
            .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
            .BindToSizeDelta(myRectTransform);
        containerState = ContainerState.Compact;
    }

    void ContainerClicked()
    {
        if (containerState == ContainerState.Expand || containerState == ContainerState.PerformingExpand) return;
        containerState = ContainerState.PerformingExpand;
        gameObject.transform.SetAsLastSibling();
        button.interactable = false;

        anim.TryCancel();
        MotionHandle center = LMotion.Create(myRectTransform.anchoredPosition, Vector2.zero, 0.4f)
            .WithEase(Ease.OutSine)
            .BindToAnchoredPosition(myRectTransform);

        MotionHandle grow = LMotion.Create(myRectTransform.sizeDelta, canvasRectTransform.rect.size, 0.4f)
            .WithEase(Ease.OutSine)
            .BindToSizeDelta(myRectTransform);

        MotionHandle moveLableUp = LMotion.Create(labelRectTransform.anchoredPosition, Vector2.up * 100f, 0.4f)
            .WithEase(Ease.OutSine)
            .BindToAnchoredPosition(labelRectTransform);
        
        MotionHandle fadeInPlay = LMotion.Create(0f, 1f, 0.1f)
            .WithEase(Ease.InSine)
            .BindToAlpha(containerButtons);

        anim = LSequence.Create()
            .Insert(0f, center)
            .Insert(0f, grow)
            .Insert(0f, moveLableUp)
            .Insert(0.3f, fadeInPlay)
            .Run();
    }

    public void BackToMenu()
    {
        if (containerState != ContainerState.Expand) return;
        containerState = ContainerState.ExitExpand;
        button.interactable = true;

        MotionHandle fadeOutPlay = LMotion.Create(1f, 0f, 0.1f)
            .WithEase(Ease.OutSine)
            .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
            .BindToAlpha(containerButtons);

        MotionHandle moveLabelCenter = LMotion.Create(labelRectTransform.anchoredPosition, Vector2.zero, 0.4f)
            .WithEase(Ease.OutSine)
            .BindToAnchoredPosition(labelRectTransform);
        
        MotionHandle shrink = LMotion.Create(canvasRectTransform.rect.size, compactState.containerSize, 0.4f)
            .WithEase(Ease.OutSine)
            .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
            .BindToSizeDelta(myRectTransform);

        MotionHandle returnToSpot = LMotion.Create(Vector2.zero, compactState.containerPos, 0.4f)
            .WithEase(Ease.OutSine)
            .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
            .BindToAnchoredPosition(myRectTransform);

        anim = LSequence.Create()
            .Insert(0f, fadeOutPlay)
            .Insert(0.1f, moveLabelCenter)
            .Insert(0.1f, shrink)
            .Insert(0.1f, returnToSpot)
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

            levelManager.LevelCamera.gameObject.SetActive(true);
            levelManager.LevelCamera.enabled = true;

            levelManager.LevelCamera.targetTexture = preview;
            previewImage.texture = preview;

            levelManager.LevelCamera.Render();
            previewLoaded = true;
        }

        if (anim == MotionHandle.None) return;
        if (anim.IsPlaying())
        {
            preview.Release();
            Vector2 size = previewRectTransform.rect.size;
            preview.width = (int)size.x;
            preview.height = (int)size.y;
            preview.Create();
        }
        else
        {
            switch (containerState)
            {
                case ContainerState.PerformingHover:
                    containerState = ContainerState.Hover;
                    break;
                case ContainerState.PerformingExpand:
                    containerState = ContainerState.Expand;
                    break;
                case ContainerState.ExitExpand:
                    containerState = ContainerState.Compact;
                    break;
                default:
                    break;
            }
            anim.TryComplete();
            anim = MotionHandle.None;
        }
    }
}