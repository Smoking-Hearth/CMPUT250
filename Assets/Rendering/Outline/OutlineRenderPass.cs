// Most credit and blame to: https://bgolus.medium.com/the-quest-for-very-wide-outlines-ba82ed442cd9

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

class OutlineRenderPass : ScriptableRenderPass 
{
    // render texture IDs
    private static readonly int silhouetteBufferID = Shader.PropertyToID("_SilhouetteBuffer");
    private static readonly int nearestPointID = Shader.PropertyToID("_NearestPoint");
    private static readonly int nearestPointPingPongID = Shader.PropertyToID("_NearestPointPingPong");

    // shader properties
    private static readonly int outlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int outlineWidthID = Shader.PropertyToID("_OutlineThickness");
    private static readonly int stepWidthID = Shader.PropertyToID("_StepWidth");
    private static readonly int silhouetteTexID = Shader.PropertyToID("_SilhouetteTex");

    // pass names
    private const int SHADER_PASS_SILHOUETTE_BUFFER_FILL = 0;
    private const int SHADER_PASS_JFA_INIT = 1; 
    private const int SHADER_PASS_JFA_FLOOD = 2;
    private const int SHADER_PASS_JFA_OUTLINE = 3;

    private readonly OutlineSettings defaultSettings;

    private readonly Material material;
    private readonly List<SpriteRenderer> spriteRenderers;

    public OutlineRenderPass(Material material, OutlineSettings outlineSettings, List<SpriteRenderer> spriteRenderers) 
    {
        this.material = material;
        defaultSettings = outlineSettings;
        this.spriteRenderers = spriteRenderers;
        renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameCtx)
    {
        using (var builder = renderGraph.AddUnsafePass<PassData>("OutlinePass", out var passData)) 
        {
            var resourceData = frameCtx.Get<UniversalResourceData>();
            var cameraData = frameCtx.Get<UniversalCameraData>();

            passData.renderers = spriteRenderers;

            passData.width = cameraData.scaledWidth;
            passData.height = cameraData.scaledHeight;
            passData.material = material;

            passData.thickness = defaultSettings.thickness;
            passData.color = defaultSettings.color;

            builder.SetRenderFunc<PassData>(ExecutePass);
        }
    }

    // This draws a single group of SpriteRenderers with an outline, one "Object"
    static void ExecutePass(PassData data, UnsafeGraphContext ctx)
    {
        
        // We may want to draw a single outline on a group of multiple sprites
        // Also we need the return somewhere so we don't try to render outlines with zero
        // thickness.

        // Here we start building a sillouette of the shape by masking bits in
        // the stencil buffer. (Is this also where the renderers actually get drawn to the screen?)
        ctx.cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
        foreach (var renderer in data.renderers)
        {
            // Apparently meshes can have multiple sub-meshes for Skinned meshes and Mesh
            // Renderers. I'm pretty sure the only Mesh-Renderers in the project are the 
            // parallax layers. So we should mostly be seeing SpriteRenderers, meaning we
            // don't need to worry about sub-meshes here (we should have 1 so render submesh idx 0)
            ctx.cmd.DrawRenderer(renderer, data.material, 0, SHADER_PASS_INTERIOR_STENCIL);
        }

        // Now we actually need to make a texture using the bits from the stencil buffer,
        // we basically want to set the pixels that sample a texture value that is not transparent
        // as white
        RenderTextureDescriptor silhouetteRTD = new(data.width, data.height, GraphicsFormat.R8_UNorm, 0)
        {
            msaaSamples = 1,
            // PERF by reducing memory usage 
            depthBufferBits = 0,
            sRGB = false,
            useMipMap = false,
            autoGenerateMips = false
        };

        //ctx.cmd.GetTemporaryRT(silhouetteBufferID, silhouetteRTD, FilterMode.Point);
        ctx.cmd.SetRenderTarget(silhouetteBufferID);
        ctx.cmd.ClearRenderTarget(false, true, Color.clear);

        foreach (var renderer in data.renderers)
        {
            ctx.cmd.DrawRenderer(renderer, data.material, 0, SHADER_PASS_SILHOUETTE_BUFFER_FILL);
        }

        // Now we want to encode position data into our silhouette for the coloring pass after 
        // our jump floods
        // ctx.cmd.Blit(silhouetteBufferID, nearestPointID, data.material, SHADER_PASS_JFA_INIT);

        var jfaRTD = silhouetteRTD;
        jfaRTD.graphicsFormat = GraphicsFormat.R16G16_SNorm;
        
        int numMips = Mathf.CeilToInt(Mathf.Log(data.thickness + 1, 2f));
        int jfaIters = numMips - 1;

        // Here we do the traditional JFA
        for (int i = jfaIters; i >= 0; --i)
        {
            // Yeah it feels a bit dangerous to sit on the edge or pixels so to the 
            // center we go (+0.5)
            // stepWidth = 2^i
            ctx.cmd.SetGlobalFloat(stepWidthID, (1 << i) + 0.5f);

        // Step 1: Silhouette Fill Pass
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Outline Silhouette Fill", out var passData))
        {
            builder.SetRenderAttachment(silhouetteBuffer, 0);
            builder.AllowPassCulling(false);

            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
            {
                // ctx.cmd.Blit(nearestPointPingPongID, nearestPointID, data.material, SHADER_PASS_JFA_FLOOD);
            });
        }

        // Step 2: JFA Init Pass
        renderGraph.AddBlitPass(
            new RenderGraphUtils.BlitMaterialParameters(
                silhouetteBuffer,
                nearestPointBuffer, 
                material,
                SHADER_PASS_JFA_INIT
            ),
            passName: "Outline JFA Init"
        );

        // This is weird to me. Since when is there a guarantee that we finished in nearestPointID?
        // ctx.cmd.Blit(nearestPointID, BuiltinRenderTextureType.CameraTarget, data.material, SHADER_PASS_JFA_OUTLINE);

        TextureHandle currentSrc = nearestPointBuffer;
        TextureHandle currentDst = pingPongBuffer;

        // Step 3: JFA Flood Passes
        for (int i = jfaIterations; i >= 0; --i)
        {
            int stepWidth = 1 << i;
            
            material.SetFloat(stepWidthID, stepWidth + 0.5f);
            renderGraph.AddBlitPass(
                new RenderGraphUtils.BlitMaterialParameters(
                    currentSrc,
                    currentDst, 
                    material,
                    SHADER_PASS_JFA_FLOOD
                ),
                passName: $"Outline JFA Flood {i}"
            );

            // Swap textures for next iteration
            (currentDst, currentSrc) = (currentSrc, currentDst);
        }

        // Step 4: Final Outline Pass
        material.SetColor(outlineColorID, defaultSettings.color);
        material.SetFloat(outlineWidthID, defaultSettings.thickness);

        using (var builder = renderGraph.AddUnsafePass<PassData>("Color Outline", out var passData))
        {
            // builder.SetRenderAttachment(cameraColorTarget, 0);
            builder.UseTexture(silhouetteBuffer);
            builder.UseTexture(currentSrc);
            builder.UseTexture(cameraColorTarget, AccessFlags.Write);

            builder.SetRenderFunc((PassData passData, UnsafeGraphContext ctx) => {
                ctx.cmd.SetGlobalTexture(silhouetteTexID, silhouetteBuffer);

                CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(ctx.cmd);
                Blitter.BlitCameraTexture(nativeCommandBuffer, currentSrc, cameraColorTarget, material, SHADER_PASS_JFA_OUTLINE);
            });
        }
    }
}
}