using Invasion.ECS;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

using MapFlags = Vortice.Direct3D11.MapFlags;

namespace Invasion.Render
{
    public class LineMesh : Component
    {
        public string Name { get; set; } = string.Empty;

        public List<Vertex> Vertices { get; set; } = new();
        public List<uint> Indices { get; set; } = new();

        public bool IgnorePosition { get; set; } = false;

        public Shader Shader => GameObject.GetComponent<Shader>();

        private ID3D11Buffer? VertexBuffer { get; set; } = null!;
        private ID3D11Buffer? IndexBuffer { get; set; } = null!;

        private int VertexBufferSize { get; set; } = 0;
        private int IndexBufferSize { get; set; } = 0;

        private readonly object Lock = new();

        private LineMesh() { }

        public void Generate()
        {
            lock (Lock)
            {
                if (Vertices.Count == 0 || Indices.Count == 0)
                    return;

                ID3D11DeviceContext4 context = Renderer.Context;
                ID3D11Device4 device = Renderer.Device;

                int newVertexBufferSize = Marshal.SizeOf<Vertex>() * Vertices.Count;
                int newIndexBufferSize = sizeof(uint) * Indices.Count;

                Vertex[] vertexArray = Vertices.ToArray();
                uint[] indexArray = Indices.ToArray();

                if (VertexBuffer != null)
                {
                    VertexBuffer.Dispose();
                }
                CreateVertexBuffer(device, vertexArray, newVertexBufferSize);

                if (IndexBuffer != null)
                {
                    IndexBuffer.Dispose();
                }
                CreateIndexBuffer(device, indexArray, newIndexBufferSize);

                VertexBufferSize = newVertexBufferSize;
                IndexBufferSize = newIndexBufferSize;
            }
        }

        private void CreateVertexBuffer(ID3D11Device4 device, Vertex[] vertexArray, int bufferSize)
        {
            BufferDescription vertexBufferDescription = new()
            {
                BindFlags = BindFlags.VertexBuffer,
                CPUAccessFlags = CpuAccessFlags.Write,
                MiscFlags = ResourceOptionFlags.None,
                ByteWidth = (uint)bufferSize,
                StructureByteStride = (uint)Marshal.SizeOf<Vertex>(),
                Usage = ResourceUsage.Dynamic
            };

            unsafe
            {
                fixed (Vertex* vertexPtr = vertexArray)
                {
                    SubresourceData vertexBufferSubresourceData = new()
                    {
                        DataPointer = (IntPtr)vertexPtr,
                        RowPitch = 0,
                        SlicePitch = 0
                    };

                    VertexBuffer = device.CreateBuffer(vertexBufferDescription, vertexBufferSubresourceData);
                }
            }
        }

        private void CreateIndexBuffer(ID3D11Device4 device, uint[] indexArray, int bufferSize)
        {
            BufferDescription indexBufferDescription = new()
            {
                BindFlags = BindFlags.IndexBuffer,
                CPUAccessFlags = CpuAccessFlags.Write,
                MiscFlags = ResourceOptionFlags.None,
                ByteWidth = (uint)bufferSize,
                StructureByteStride = sizeof(uint),
                Usage = ResourceUsage.Dynamic
            };

            unsafe
            {
                fixed (uint* indexPtr = indexArray)
                {
                    SubresourceData indexBufferSubresourceData = new()
                    {
                        DataPointer = (IntPtr)indexPtr,
                        RowPitch = 0,
                        SlicePitch = 0
                    };

                    IndexBuffer = device.CreateBuffer(indexBufferDescription, indexBufferSubresourceData);
                }
            }
        }

        private unsafe void UpdateBuffer<T>(ID3D11DeviceContext4 context, ID3D11Buffer buffer, T[] dataArray, int dataSize) where T : unmanaged
        {
            fixed (T* dataPtr = dataArray)
            {
                MappedSubresource mappedResource = context.Map(buffer, 0, MapMode.WriteDiscard, MapFlags.None);
                Buffer.MemoryCopy(dataPtr, mappedResource.DataPointer.ToPointer(), dataSize, dataSize);
                context.Unmap(buffer, 0);
            }
        }

        public override void Render(Camera camera)
        {
            ID3D11DeviceContext4 context = Renderer.Context;

            context.IASetPrimitiveTopology(PrimitiveTopology.LineList);

            uint stride = (uint)Marshal.SizeOf<Vertex>();
            uint offset = 0;

            context.IASetVertexBuffers(0, [VertexBuffer!], [stride], [offset]);

            context.IASetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);

            Shader.SetConstantBuffer<DefaultMatrixBuffer>(ShaderStage.Vertex, 0, new()
            {
                Projection = Matrix4x4.Transpose(camera.Projection),
                View = Matrix4x4.Transpose(camera.View),
                Model = IgnorePosition ? Matrix4x4.Identity : Matrix4x4.Transpose(GameObject.Transform.GetModelMatrix())
            });

            Shader.Bind();

            context.DrawIndexed((uint)Indices.Count, 0, 0);
        }

        public override void CleanUp()
        {
            VertexBuffer?.Dispose();
            VertexBuffer = null;
            IndexBuffer?.Dispose();
            IndexBuffer = null;
        }

        public static LineMesh Create(string name, List<Vertex> vertices, List<uint> indices, bool ignorePosition = false)
        {
            return new LineMesh
            {
                Name = name,
                Vertices = new(vertices),
                Indices = new(indices),
                IgnorePosition = ignorePosition
            };
        }
    }
}
