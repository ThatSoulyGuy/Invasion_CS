using Invasion.ECS;
using Invasion.Render;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Invasion.UI
{
    [StructLayout(LayoutKind.Sequential)]
    public struct UIMatrixBuffer
    {
        public Matrix4x4 Projection;
        public Matrix4x4 Model;
    }

    public class UIMesh
    {
        public string Name { get; set; } = string.Empty;

        public List<Vertex> Vertices { get; set; } = [];
        public List<uint> Indices { get; set; } = [];

        public Shader Shader { get; set; } = null!;
        public Texture Texture { get; set; } = null!;

        public UITransform Transform { get; set; } = null!;

        private ID3D11Buffer VertexBuffer { get; set; } = null!;
        private ID3D11Buffer IndexBuffer { get; set; } = null!;

        private object Lock { get; set; } = new();

        private UIMesh() { }

        public void Generate()
        {
            lock (Lock)
            {
                if (Vertices.Count == 0 || Indices.Count == 0)
                    return;

                VertexBuffer?.Dispose();
                IndexBuffer?.Dispose();

                ID3D11Device5 device = Renderer.Device;

                BufferDescription vertexBufferDescription = new()
                {
                    BindFlags = BindFlags.VertexBuffer,
                    CPUAccessFlags = CpuAccessFlags.None,
                    MiscFlags = ResourceOptionFlags.None,
                    ByteWidth = (uint)Marshal.SizeOf<Vertex>() * (uint)Vertices.Count,
                    StructureByteStride = 0,
                    Usage = ResourceUsage.Default
                };

                GCHandle vertexHandle = GCHandle.Alloc(Vertices.ToArray(), GCHandleType.Pinned);

                try
                {
                    SubresourceData vertexBufferSubresourceData = new()
                    {
                        DataPointer = vertexHandle.AddrOfPinnedObject(),
                        RowPitch = 0,
                        SlicePitch = 0
                    };

                    VertexBuffer = device.CreateBuffer(vertexBufferDescription, vertexBufferSubresourceData);
                }
                finally
                {
                    vertexHandle.Free();
                }


                BufferDescription indexBufferDescription = new()
                {
                    BindFlags = BindFlags.IndexBuffer,
                    CPUAccessFlags = CpuAccessFlags.None,
                    MiscFlags = ResourceOptionFlags.None,
                    ByteWidth = sizeof(uint) * (uint)Indices.Count,
                    StructureByteStride = 0,
                    Usage = ResourceUsage.Default
                };

                GCHandle indexHandle = GCHandle.Alloc(Indices.ToArray(), GCHandleType.Pinned);

                try
                {
                    SubresourceData indexBufferSubresourceData = new()
                    {
                        DataPointer = indexHandle.AddrOfPinnedObject(),
                        RowPitch = 0,
                        SlicePitch = 0
                    };

                    IndexBuffer = device.CreateBuffer(indexBufferDescription, indexBufferSubresourceData);
                }
                finally
                {
                    indexHandle.Free();
                }
            }
        }

        public void Render(Matrix4x4 projection)
        {
            ID3D11DeviceContext4 context = Renderer.Context;

            context.OMSetDepthStencilState(Renderer.NoDepthStencilState, 0);
            unsafe { Renderer.Context.OMSetBlendState(Renderer.AlphaBlendState, null, 0xFFFFFFFF); }

            context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);

            uint stride = (uint)Marshal.SizeOf<Vertex>();
            uint offset = 0;
            context.IASetVertexBuffers(0, [VertexBuffer], [stride], [offset]);

            context.IASetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            
            Shader.SetConstantBuffer<UIMatrixBuffer>(ShaderStage.Vertex, 0, new()
            {
                Projection = Matrix4x4.Transpose(projection),
                Model = Matrix4x4.Transpose(Transform.GetModelMatrix())
            });

            Shader.Bind();
            Texture.Bind(0);

            context.DrawIndexed((uint)Indices.Count, 0, 0);

            context.OMSetDepthStencilState(null, 0);

            unsafe { context.OMSetBlendState(null, null, 0xFFFFFFFF); }
        }

        public void CleanUp()
        {
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
        }

        public static UIMesh Create(string name, List<Vertex> vertices, List<uint> indices)
        {
            return new()
            {
                Name = name,
                Vertices = vertices,
                Indices = indices
            };
        }
    }
}
