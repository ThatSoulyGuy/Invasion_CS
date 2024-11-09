using Invasion.ECS;
using Invasion.Math;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Invasion.Render
{
    public struct SkyboxMatrixBuffer
    {
        public Matrix4x4 Projection;
        public Matrix4x4 View;
    }

    public class Skybox : Component
    {
        private ID3D11Buffer? vertexBuffer;
        private ID3D11Buffer? indexBuffer;

        private Shader Shader => GameObject.GetComponent<Shader>();
        private TextureCube Texture => GameObject.GetComponent<TextureCube>();

        private Skybox() { }

        public void Generate(ID3D11Device device)
        {
            Vector3f[] vertices =
            {
                new(-1.0f,  1.0f, -1.0f),
                new(-1.0f, -1.0f, -1.0f),
                new( 1.0f, -1.0f, -1.0f),
                new( 1.0f, -1.0f, -1.0f),
                new( 1.0f,  1.0f, -1.0f),
                new(-1.0f,  1.0f, -1.0f),

                new(-1.0f, -1.0f,  1.0f),
                new(-1.0f,  1.0f,  1.0f),
                new( 1.0f,  1.0f,  1.0f),
                new( 1.0f,  1.0f,  1.0f),
                new( 1.0f, -1.0f,  1.0f),
                new(-1.0f, -1.0f,  1.0f),

                new(-1.0f,  1.0f,  1.0f),
                new(-1.0f,  1.0f, -1.0f),
                new(-1.0f, -1.0f, -1.0f),
                new(-1.0f, -1.0f, -1.0f),
                new(-1.0f, -1.0f,  1.0f),
                new(-1.0f,  1.0f,  1.0f),

                new(1.0f,  1.0f, -1.0f),
                new(1.0f,  1.0f,  1.0f),
                new(1.0f, -1.0f,  1.0f),
                new(1.0f, -1.0f,  1.0f),
                new(1.0f, -1.0f, -1.0f),
                new(1.0f,  1.0f, -1.0f),

                new(-1.0f,  1.0f,  1.0f),
                new(-1.0f,  1.0f, -1.0f),
                new( 1.0f,  1.0f, -1.0f),
                new( 1.0f,  1.0f, -1.0f),
                new( 1.0f,  1.0f,  1.0f),
                new(-1.0f,  1.0f,  1.0f),
                
                new(-1.0f, -1.0f, -1.0f),
                new(-1.0f, -1.0f,  1.0f),
                new( 1.0f, -1.0f,  1.0f),
                new( 1.0f, -1.0f,  1.0f),
                new( 1.0f, -1.0f, -1.0f),
                new(-1.0f, -1.0f, -1.0f),
            };

            uint[] indices = 
            {
                0, 1, 2, 
                2, 3, 0,

                4, 5, 6, 
                6, 7, 4,

                4, 0, 3, 
                3, 7, 4,

                1, 5, 6, 
                6, 2, 1,

                4, 5, 1, 
                1, 0, 4,

                3, 2, 6, 
                6, 7, 3
            };

            vertexBuffer = device.CreateBuffer(vertices, BindFlags.VertexBuffer);
            indexBuffer = device.CreateBuffer(indices, BindFlags.IndexBuffer);
        }

        public override void Render(Camera camera)
        {
            if (Shader == null || vertexBuffer == null || indexBuffer == null)
                return;

            var context = Renderer.Context;

            var depthStencilStateDescription = new DepthStencilDescription
            {
                DepthEnable = true,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthFunc = ComparisonFunction.LessEqual
            };

            var depthStencilState = Renderer.Device.CreateDepthStencilState(depthStencilStateDescription);
            context.OMSetDepthStencilState(depthStencilState);

            RasterizerDescription rasterizerDesc = new()
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
                FrontCounterClockwise = false,
                DepthClipEnable = true
            };

            var rasterizerState = Renderer.Device.CreateRasterizerState(rasterizerDesc);
            context.RSSetState(rasterizerState);

            context.IASetVertexBuffer(0, vertexBuffer, (uint)Marshal.SizeOf<Vector3f>());
            context.IASetIndexBuffer(indexBuffer, Format.R32_UInt, 0);
            context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);

            var viewMatrix = camera.View;
            viewMatrix.Translation = Vector3.Zero;

            Shader.SetConstantBuffer(ShaderStage.Vertex, 0, new SkyboxMatrixBuffer()
            {
                Projection = camera.Projection,
                View = viewMatrix
            });

            Shader.Bind();
            Texture.Bind(0);

            context.DrawIndexed(36, 0, 0);

            context.OMSetDepthStencilState(null);
            depthStencilState.Dispose();

            context.RSSetState(null);
            rasterizerState.Dispose();
        }

        public static Skybox Create()
        {
            return new();
        }
    }
}
