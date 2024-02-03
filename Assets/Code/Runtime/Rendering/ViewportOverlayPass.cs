using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Framework.Runtime.Rendering
{
    public class ViewportOverlayPass : ScriptableRenderPass
    {
        private Settings settings;
        private Material overlayMaterial;
        private Material clearMaterial;
        private Mesh mesh;
        private int rtHandle = Shader.PropertyToID("_ViewportOverlay");

        private List<ShaderTagId> shaderTagIdList;

        public static float ViewportFieldOfView { get; set; }

        public ViewportOverlayPass(Settings settings)
        {
            this.settings = settings;
            this.settings.Validate();

            if (!overlayMaterial)
            {
                overlayMaterial = new Material(Shader.Find("Hidden/ViewportOverlay"));
                overlayMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            if (!clearMaterial)
            {
                clearMaterial = new Material(Shader.Find("Hidden/Clear"));
                clearMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            if (!mesh)
            {
                mesh = new Mesh();
                mesh.hideFlags = HideFlags.HideAndDontSave;

                mesh.vertices = new Vector3[]
                {
                    new(-1.0f, -1.0f, 0.0f),
                    new(3.0f, -1.0f, 0.0f),
                    new(-1.0f, 3.0f, 0.0f),
                };
                mesh.uv = new Vector2[]
                {
                    new(0.0f, 1.0f),
                    new(2.0f, 1.0f),
                    new(0.0f, -1.0f),
                };
                mesh.triangles = new int[]
                {
                    0, 1, 2,
                };
            }

            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;

            shaderTagIdList = new List<ShaderTagId>();
            shaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
            shaderTagIdList.Add(new ShaderTagId("UniversalForward"));
            shaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            var aspect = (float)descriptor.width / descriptor.height;
            descriptor.width = Mathf.RoundToInt(settings.height * aspect);
            descriptor.height = settings.height;
            descriptor.colorFormat = RenderTextureFormat.ARGB32;

            cmd.GetTemporaryRT(rtHandle, descriptor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get("ViewportOverlay");
            cmd.Clear();

            cmd.SetRenderTarget(rtHandle);
            cmd.ClearRenderTarget(true, false, Color.clear);

            cmd.DrawMesh(mesh, Matrix4x4.identity, clearMaterial, 0, 0);

            var camera = renderingData.cameraData.camera;

            if (settings.overrideFov && !renderingData.cameraData.isSceneViewCamera)
            {
                var projectionMatrix = Matrix4x4.Perspective(ViewportFieldOfView, camera.aspect, settings.nearClip, settings.farClip);
                projectionMatrix = GL.GetGPUProjectionMatrix(projectionMatrix, renderingData.cameraData.IsCameraProjectionMatrixFlipped());

                var viewMatrix = renderingData.cameraData.GetViewMatrix();
                RenderingUtils.SetViewAndProjectionMatrices(cmd, viewMatrix, projectionMatrix, false);
            }

            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque, 1 << Layers.Viewport);
            var drawingSettings = CreateDrawingSettings(shaderTagIdList, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);

            cmd.SetRenderTarget(renderingData.cameraData.renderer.cameraColorTargetHandle);
            cmd.SetGlobalTexture("_ViewportOverlay", rtHandle);
            cmd.DrawMesh(mesh, Matrix4x4.identity, overlayMaterial, 0, 0);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) { cmd.ReleaseTemporaryRT(rtHandle); }

        [System.Serializable]
        public struct Settings
        {
            public int height;
            public bool overrideFov;
            public float nearClip;
            public float farClip;

            public void Validate() { height = Mathf.Max(2, height); }
        }
    }
}