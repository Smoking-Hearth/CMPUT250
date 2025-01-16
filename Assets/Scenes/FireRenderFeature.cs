using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

class PassData 
{
    internal TextureHandle heatmap;
}

class FireRenderPass : ScriptableRenderPass 
{

    public PassData m_passData;

    private Material m_fireMaterial;
    public FireRenderPass(Material fireMaterial) 
    {
        m_fireMaterial = fireMaterial;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }


    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        using RenderGraphBuilder builder = renderGraph.AddRenderPass("Pass", out m_passData);
        var renderingData = frameData.Get<UniversalRenderingData>();
        var cameraData = frameData.Get<UniversalCameraData>();

        // I hate this. That's what make it fun.
    }
}

public class FireRenderFeature : ScriptableRendererFeature
{
    private FireRenderPass fireRenderPass;
    private Texture2D heatmap;

    [SerializeField]
    private Shader m_fireShader; 
    private Material m_fireMaterial;

    public override void Create()
    {
        m_fireMaterial = CoreUtils.CreateEngineMaterial(m_fireShader);
        
        fireRenderPass = new FireRenderPass(m_fireMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(fireRenderPass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(m_fireMaterial);
    }

}
