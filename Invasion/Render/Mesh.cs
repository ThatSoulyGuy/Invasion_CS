﻿using Invasion.ECS;
using Invasion.Math;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Invasion.Render
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DefaultMatrixBuffer
    {
        public Matrix4x4 Model;
    }

    public class Mesh : Component
    {
        public string Name { get; set; } = string.Empty;

        public List<Vertex> Vertices { get; private set; } = [];
        public List<uint> Indices { get; private set; } = [];

        public Shader Shader => GameObject.GetComponent<Shader>();
        public Texture Texture => GameObject.GetComponent<Texture>();

        private ID3D11Buffer VertexBuffer { get; set; } = null!;
        private ID3D11Buffer IndexBuffer { get; set; } = null!;

        private Mesh() { }

        public void Generate()
        {
            ID3D11Device5 device = Renderer.Device;

            BufferDescription vertexBufferDescription = new()
            {
                BindFlags = BindFlags.VertexBuffer,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None,
                ByteWidth = Marshal.SizeOf<Vertex>() * Vertices.Count,
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
                ByteWidth = sizeof(uint) * Indices.Count,
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

        public override void Render()
        {
            ID3D11DeviceContext4 context = Renderer.Context;

            context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);

            int stride = Marshal.SizeOf<Vertex>();
            int offset = 0;
            context.IASetVertexBuffers(0, [VertexBuffer], [stride], [offset]);

            context.IASetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            
            Shader.SetConstantBuffer<DefaultMatrixBuffer>(ShaderStage.Vertex, 0, new()
            {
                Model = Matrix4x4.Transpose(GameObject.Transform.GetModelMatrix())
            });

            Shader.Bind();
            Texture.Bind(0);

            context.DrawIndexed(Indices.Count, 0, 0);
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
