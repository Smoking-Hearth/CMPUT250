using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelContainer: MonoBehaviour
{
    [SerializeField] public RawImage previewImage;
    [field: SerializeField] public SceneIndex levelIndex { get; private set; }
    private Texture2D preview;
    private bool previewLoaded = false;

    void Start()
    {
        gameObject.MyLevelManager().onActivate += Activate;
    }
    
    void Activate()
    {

    }

    void Update()
    {
        if (!gameObject.ShouldUpdate()) return;
        
        if (!previewLoaded && GameManager.SceneSystem.IsLoaded(levelIndex))
        {
            LevelManager levelManager = GameManager.SceneSystem.LevelManagers[(int)levelIndex];
            RenderTexture cameraTarget = levelManager.LevelCamera.targetTexture;
            

            previewLoaded = true;
        }
    }
}