using System;
using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [HideInInspector] public MeshRenderer meshRenderer = null;

    [field: SerializeField] public Texture Texture { get; private set;}
    [field: SerializeField] public bool IsTiled {get; private set; }

    [Header("Animation")]
    [field: SerializeField, Range(1, 8)] public int Rows { get; private set; } = 1;
    [field: SerializeField, Range(1, 8)] public int Columns { get; private set; } = 1;
    // This is the size of a single frame in UV coords
    private float width = 1f; 
    private float height = 1f;

    [field: SerializeField, Range(1, 256)] public int Frames { get; private set; } = 1;
    // WARN: After Awake this store seconds/frame to avoid the divide each frame.
    [field: SerializeField] public float FramesPerSecond { get; private set; } = 10f;
    private int frame = 0;
    private float elapsed = 0f; 

    private Vector2 initPosition;
    public Vector2 InitPosition 
    { 
        get { return initPosition; }
    }

    void Awake()
    {
        initPosition = transform.localPosition;

        Rows = Math.Max(Rows, 1);
        Columns = Math.Max(Columns, 1);
        Frames = Math.Max(Frames, 1);

        if (Frames > Rows * Columns)
        {
            DevLog.Warn("An animated parallax background is configured incorrectly, not enough frames provided. Truncating");
            Frames = Rows * Columns;
        }

        if (FramesPerSecond > 0.1)
        {
            FramesPerSecond = 1 / FramesPerSecond;
        }
        else 
        {
            DevLog.Warn("Config asks to display too few frames, cancelling animation.");
            Frames = 1;
        }

        width = 1f / Columns;
        height = 1f / Rows;
    }

    public Vector2 Attach(GameObject tiledLayerAnchor, MeshRenderer tiledLayerPrefab, int layerIndex)
    {
        meshRenderer = Instantiate(tiledLayerPrefab, parent: tiledLayerAnchor.transform);
        meshRenderer.transform.localPosition = new Vector3(0f, 0f, 100f + 5f * layerIndex);
        meshRenderer.material.SetTexture("_MainTex", Texture);
        SetFrameUniforms();
        return meshRenderer.bounds.extents;
    }

    public void SetFrameUniforms()
    {
        float left = (frame % Columns) * width;
        float top = (frame / Rows) * height;

        // (left, width, top, height)
        Vector4 uvRect = new(left, width, top, height);
        meshRenderer.material.SetVector("_UVRect", uvRect);
    }

    void Update()
    {
        // Shuttling data from RAM to VRAM is not fast. 
        if (Frames == 1 || meshRenderer == null) return;

        elapsed += Time.deltaTime;
        if (elapsed >= FramesPerSecond) 
        {
            frame += 1;
            frame %= Frames;
            elapsed = 0f;
            SetFrameUniforms();
        }
    }
}