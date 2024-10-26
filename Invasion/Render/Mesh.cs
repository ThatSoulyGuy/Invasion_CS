using Invasion.ECS;
using Invasion.Math;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Vortice;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

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

        public List<Vertex> Vertices { get; set; } = new();
        public List<uint> Indices { get; set; } = new();

        public Shader Shader => GameObject.GetComponent<Shader>();
        public Texture Texture => GameObject.GetComponent<Texture>();

        private ID3D11Buffer VertexBuffer { get; set; } = null!;
        private ID3D11Buffer IndexBuffer { get; set; } = null!;

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

                int vertexBufferSize = Marshal.SizeOf<Vertex>() * Vertices.Count;

                if (VertexBuffer != null)
                {
                    MappedSubresource vertexDataBox = context.Map(VertexBuffer, 0, MapMode.WriteDiscard, Vortice.Direct3D11.MapFlags.None);

                    unsafe
                    {
                        fixed (Vertex* vertexPtr = Vertices.ToArray())
                        {
                            Buffer.MemoryCopy(vertexPtr, vertexDataBox.DataPointer.ToPointer(), vertexBufferSize, vertexBufferSize);
                        }
                    }

                    context.Unmap(VertexBuffer, 0);
                }
                else
                {
                    BufferDescription vertexBufferDescription = new()
                    {
                        BindFlags = BindFlags.VertexBuffer,
                        CPUAccessFlags = CpuAccessFlags.Write,
                        MiscFlags = ResourceOptionFlags.None,
                        ByteWidth = (uint)vertexBufferSize,
                        StructureByteStride = 0,
                        Usage = ResourceUsage.Dynamic
                    };

                    Vertex[] vertexArray = Vertices.ToArray();

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

                int indexBufferSize = sizeof(uint) * Indices.Count;

                if (IndexBuffer != null)
                {
                    MappedSubresource indexDataBox = context.Map(IndexBuffer, 0, MapMode.WriteDiscard, Vortice.Direct3D11.MapFlags.None);

                    unsafe
                    {
                        fixed (uint* indexPtr = Indices.ToArray())
                        {
                            Buffer.MemoryCopy(indexPtr, indexDataBox.DataPointer.ToPointer(), indexBufferSize, indexBufferSize);
                        }
                    }

                    context.Unmap(IndexBuffer, 0);
                }
                else
                {
                    BufferDescription indexBufferDescription = new()
                    {
                        BindFlags = BindFlags.IndexBuffer,
                        CPUAccessFlags = CpuAccessFlags.Write,
                        MiscFlags = ResourceOptionFlags.None,
                        ByteWidth = (uint)indexBufferSize,
                        StructureByteStride = 0,
                        Usage = ResourceUsage.Dynamic
                    };

                    uint[] indexArray = Indices.ToArray();

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
            }
        }

        public override void Render(Camera camera)
        {
            ID3D11DeviceContext4 context = Renderer.Context;

            context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);

            uint stride = (uint)Marshal.SizeOf<Vertex>();
            uint offset = 0;
            context.IASetVertexBuffers(0, new ID3D11Buffer[] { VertexBuffer }, new uint[] { stride }, new uint[] { offset });

            context.IASetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);

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
            IndexBuffer?.Dispose();
        }

        public static Mesh Create(string name, List<Vertex> vertices, List<uint> indices)
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
