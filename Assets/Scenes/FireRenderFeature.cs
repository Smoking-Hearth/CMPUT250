using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

class PassData 
{
    internal Texture2D cpu_heatmap;
    internal TextureHandle screen_texture;
    internal TextureHandle heatmap_texture;
    internal Material shader;
}

class FireRenderPass : ScriptableRenderPass 
{

    private PassData m_passData;
    private Material m_fireMaterial;
    public Texture2D m_heatmap;

    public FireRenderPass(Material fireMaterial) 
    {
        m_fireMaterial = fireMaterial;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }


    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameCtx)
    {
        using (RenderGraphBuilder builder = renderGraph.AddRenderPass("Pass", out m_passData)) 
        {
            var frameData = frameCtx.Get<UniversalResourceData>();
            m_passData.screen_texture = builder.ReadTexture(frameData.activeColorTexture);
            m_passData.heatmap_texture = builder.CreateTransientTexture(new TextureDesc(m_heatmap.width, m_heatmap.height));
            builder.SetRenderFunc<PassData>(RenderPass);
        }
    }

    static void RenderPass(PassData data, RenderGraphContext ctx)
    {
        // Copy the heatmap to VRAM (IDK if this is blocking)
        // PERF: Allow this to be skipped when texture has not been modified.
        ctx.cmd.Blit(data.cpu_heatmap, data.heatmap_texture);

        // Bind uniforms
        ctx.cmd.SetGlobalTexture("_ScreenTexture", data.screen_texture);
        ctx.cmd.SetGlobalTexture("_HeatmapTexture", data.heatmap_texture);

        // Render
        ctx.cmd.


    }
}

public class FireRenderFeature : ScriptableRendererFeature
{
    private FireRenderPass fireRenderPass;
    private Texture2D heatmap;

    [SerializeField]
    private Shader m_fireShader; 
    private Material m_fireMaterial;

    // We want this to persist between frame so that we don't need to reupload
    // each cycle.
    private TextureHandle m_heatmapTextureHandle;

    public override void Create()
    {
        m_fireMaterial = CoreUtils.CreateEngineMaterial(m_fireShader);
        // m_heatmapTextureHandle = RenderingUtils.ReAllocateHandleIfNeeded();
        m_heatmapTextureHandle = RTHandles.Alloc()

        fireRenderPass = new FireRenderPass(m_fireMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(fireRenderPass);
    }

    protected override void Dispose(bool disposing)
    {
        m_heatmapTextureHandle.Release();
        CoreUtils.Destroy(m_fireMaterial);
    }

}
