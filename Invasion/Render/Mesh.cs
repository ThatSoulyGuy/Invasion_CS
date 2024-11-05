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
    [StructLayout(LayoutKind.Sequential)]
    public struct DefaultMatrixBuffer
    {
        public Matrix4x4 Projection;
        public Matrix4x4 View;
        public Matrix4x4 Model;
    }

    public class Mesh : Component
    {
        public string Name { get; set; } = string.Empty;

        public ConcurrentBag<Vertex> Vertices { get; set; } = new();
        public ConcurrentBag<uint> Indices { get; set; } = new();

        public Shader Shader => GameObject.GetComponent<Shader>();
        public Texture Texture => GameObject.GetComponent<Texture>();

        private ID3D11Buffer? VertexBuffer { get; set; } = null!;
        private ID3D11Buffer? IndexBuffer { get; set; } = null!;

        private int VertexBufferSize { get; set; } = 0;
        private int IndexBufferSize { get; set; } = 0;

        private object Lock { get; set; } = new();

        private Mesh() { }

        public void Generate()
        {
            lock (Lock)
            {
                if (Vertices.Count == 0 || Indices.Count == 0)
                    return;

                ID3D11DeviceContext4 context = Renderer.Context;
                ID3D11Device4 device = Renderer.Device;

                int newVertexBufferSize = Marshal.SizeOf(typeof(Vertex)) * Vertices.Count;
                int newIndexBufferSize = sizeof(uint) * Indices.Count;

                Vertex[] vertexArray = Vertices.ToArray();
                uint[] indexArray = Indices.ToArray();

                if (VertexBuffer != null)
                {
                    if (newVertexBufferSize > VertexBufferSize)
                    {
                        VertexBuffer.Dispose();

                        CreateVertexBuffer(device, vertexArray, newVertexBufferSize);
                    }
                    else
                        UpdateBuffer(context, VertexBuffer, vertexArray, newVertexBufferSize);
                }
                else
                    CreateVertexBuffer(device, vertexArray, newVertexBufferSize);

                if (IndexBuffer != null)
                {
                    if (newIndexBufferSize > IndexBufferSize)
                    {
                        IndexBuffer.Dispose();

                        CreateIndexBuffer(device, indexArray, newIndexBufferSize);
                    }
                    else
                        UpdateBuffer(context, IndexBuffer, indexArray, newIndexBufferSize);
                }
                else
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
                StructureByteStride = 0,
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
                StructureByteStride = 0,
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

            context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);

            uint stride = (uint)Marshal.SizeOf<Vertex>();
            uint offset = 0;
            context.IASetVertexBuffers(0, [VertexBuffer!], [stride], [offset]);

            context.IASetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);

            if (GameObject.Transform == null)
                return;

            Shader.SetConstantBuffer<DefaultMatrixBuffer>(ShaderStage.Vertex, 0, new()
            {
                Projection = Matrix4x4.Transpose(camera.Projection),
                View = Matrix4x4.Transpose(camera.View),
                Model = Matrix4x4.Transpose(GameObject.Transform.GetModelMatrix())
            });

            Shader.Bind();
            Texture.Bind(0);

            context.DrawIndexed((uint)Indices.Count, 0, 0);
        }

        public override void CleanUp()
        {
            VertexBuffer?.Dispose();
            VertexBuffer = null;
            IndexBuffer?.Dispose();
            IndexBuffer = null;
        }

        public static Mesh Create(string name, List<Vertex> vertices, List<uint> indices)
        {
            return new()
            {
                Name = name,
                Vertices = new(vertices),
                Indices = new(indices)
            };
        }
    }
}