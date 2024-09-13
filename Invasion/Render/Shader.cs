using Invasion.ECS;
using Invasion.Util;
using SharpGen.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vortice;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Invasion.Render
{
    public enum ShaderStage
    {
        Vertex,
        Pixel,
        Compute,
        Geometry,
        Hull,
        Domain
    }

    public class Shader : Component
    {
        public string Name { get; private set; } = string.Empty;
        public DomainedPath DomainedPath { get; set; } = null!;

        public string VertexPath { get; private set; } = string.Empty;
        public string PixelPath { get; private set; } = string.Empty;
        public string ComputePath { get; private set; } = string.Empty;
        public string GeometryPath { get; private set; } = string.Empty;
        public string HullPath { get; private set; } = string.Empty;
        public string DomainPath { get; private set; } = string.Empty;

        public Blob VertexBlob { get; private set; } = null!;

        public ID3D11InputLayout InputLayout { get; private set; } = null!;

        public ID3D11VertexShader VertexShader { get; private set; } = null!;
        public ID3D11PixelShader PixelShader { get; private set; } = null!;
        public ID3D11ComputeShader? ComputeShader { get; private set; } = null;
        public ID3D11GeometryShader? GeometryShader { get; private set; } = null;
        public ID3D11HullShader? HullShader { get; private set; } = null;
        public ID3D11DomainShader? DomainShader { get; private set; } = null;

        private Dictionary<(ShaderStage, int), ID3D11Buffer> ConstantBuffers { get; } = [];
        private Dictionary<(ShaderStage, int), ID3D11SamplerState> SamplerStates { get; } = [];
        private Dictionary<(ShaderStage, int), ID3D11ShaderResourceView> Textures { get; } = [];

        private Shader() { }

        public void Bind()
        {
            Renderer.Context.IASetInputLayout(InputLayout);

            if (VertexShader != null)
                Renderer.Context.VSSetShader(VertexShader);

            if (PixelShader != null)
                Renderer.Context.PSSetShader(PixelShader);

            if (ComputeShader != null)
                Renderer.Context.CSSetShader(ComputeShader);

            if (GeometryShader != null)
                Renderer.Context.GSSetShader(GeometryShader);

            if (HullShader != null)
                Renderer.Context.HSSetShader(HullShader);

            if (DomainShader != null)
                Renderer.Context.DSSetShader(DomainShader);

            BindConstantBuffers();
            BindSamplerStates();
            BindTextures();
        }

        public void SetConstantBuffer<T>(ShaderStage stage, int slot, T data) where T : struct
        {
            unsafe
            {
                if (ConstantBuffers.TryGetValue((stage, slot), out ID3D11Buffer? buffer))
                {
                    MappedSubresource mappedSubresource = Renderer.Context.Map(buffer, 0, MapMode.WriteDiscard, MapFlags.None);
                    Unsafe.Copy(mappedSubresource.DataPointer.ToPointer(), ref data);
                    Renderer.Context.Unmap(buffer, 0);
                }
                else
                {
                    BufferDescription description = new()
                    {
                        BindFlags = BindFlags.ConstantBuffer,
                        CPUAccessFlags = CpuAccessFlags.Write,
                        MiscFlags = ResourceOptionFlags.None,
                        ByteWidth = (int)MathF.Max(16, (Unsafe.SizeOf<T>() + 15) / 16 * 16),
                        StructureByteStride = 0,
                        Usage = ResourceUsage.Dynamic
                    };

                    SubresourceData subresourceData = new()
                    {
                        DataPointer = (IntPtr)Unsafe.AsPointer(ref data),
                        RowPitch = 0,
                        SlicePitch = 0
                    };

                    buffer = Renderer.Device.CreateBuffer(description, subresourceData);
                    ConstantBuffers[(stage, slot)] = buffer;
                }
            }
        }

        public void SetConstantBuffer(ShaderStage stage, int slot, ID3D11Buffer buffer)
        {
            ConstantBuffers[(stage, slot)] = buffer;
        }

        public void SetSamplerState(ShaderStage stage, int slot, ID3D11SamplerState sampler)
        {
            SamplerStates[(stage, slot)] = sampler;
        }

        public void SetTexture(ShaderStage stage, int slot, ID3D11ShaderResourceView texture)
        {
            Textures[(stage, slot)] = texture;
        }

        private void Generate()
        {
            if (File.Exists(VertexPath))
            {
                var vertexByteCode = CompileShader(VertexPath, "Main", "vs_5_0");
                VertexShader = Renderer.Device.CreateVertexShader(vertexByteCode);
            }
            else
                throw new Exception($"Vertex shader not found at path: {VertexPath}");

            if (File.Exists(PixelPath))
            {
                var pixelByteCode = CompileShader(PixelPath, "Main", "ps_5_0");
                PixelShader = Renderer.Device.CreatePixelShader(pixelByteCode);
            }
            else
                throw new Exception($"Pixel shader not found at path: {PixelPath}");

            if (File.Exists(ComputePath))
            {
                var computeByteCode = CompileShader(ComputePath, "Main", "cs_5_0");
                ComputeShader = Renderer.Device.CreateComputeShader(computeByteCode);
            }

            if (File.Exists(GeometryPath))
            {
                var geometryByteCode = CompileShader(GeometryPath, "Main", "gs_5_0");
                GeometryShader = Renderer.Device.CreateGeometryShader(geometryByteCode);
            }

            if (File.Exists(HullPath))
            {
                var hullByteCode = CompileShader(HullPath, "Main", "hs_5_0");
                HullShader = Renderer.Device.CreateHullShader(hullByteCode);
            }

            if (File.Exists(DomainPath))
            {
                var domainByteCode = CompileShader(DomainPath, "Main", "ds_5_0");
                DomainShader = Renderer.Device.CreateDomainShader(domainByteCode);
            }

            InputLayout = Renderer.Device.CreateInputLayout(Vertex.InputElements, VertexBlob);
        }

        private void BindConstantBuffers()
        {
            foreach (var ((stage, slot), buffer) in ConstantBuffers)
            {
                switch (stage)
                {
                    case ShaderStage.Vertex:
                        Renderer.Context.VSSetConstantBuffer(slot, buffer);
                        break;
                    case ShaderStage.Pixel:
                        Renderer.Context.PSSetConstantBuffer(slot, buffer);
                        break;
                    case ShaderStage.Compute:
                        Renderer.Context.CSSetConstantBuffer(slot, buffer);
                        break;
                    case ShaderStage.Geometry:
                        Renderer.Context.GSSetConstantBuffer(slot, buffer);
                        break;
                    case ShaderStage.Hull:
                        Renderer.Context.HSSetConstantBuffer(slot, buffer);
                        break;
                    case ShaderStage.Domain:
                        Renderer.Context.DSSetConstantBuffer(slot, buffer);
                        break;
                }
            }
        }

        private void BindSamplerStates()
        {
            foreach (var ((stage, slot), sampler) in SamplerStates)
            {
                switch (stage)
                {
                    case ShaderStage.Vertex:
                        Renderer.Context.VSSetSampler(slot, sampler);
                        break;
                    case ShaderStage.Pixel:
                        Renderer.Context.PSSetSampler(slot, sampler);
                        break;
                    case ShaderStage.Compute:
                        Renderer.Context.CSSetSampler(slot, sampler);
                        break;
                    case ShaderStage.Geometry:
                        Renderer.Context.GSSetSampler(slot, sampler);
                        break;
                    case ShaderStage.Hull:
                        Renderer.Context.HSSetSampler(slot, sampler);
                        break;
                    case ShaderStage.Domain:
                        Renderer.Context.DSSetSampler(slot, sampler);
                        break;
                }
            }
        }

        private void BindTextures()
        {
            foreach (var ((stage, slot), texture) in Textures)
            {
                switch (stage)
                {
                    case ShaderStage.Vertex:
                        Renderer.Context.VSSetShaderResource(slot, texture);
                        break;
                    case ShaderStage.Pixel:
                        Renderer.Context.PSSetShaderResource(slot, texture);
                        break;
                    case ShaderStage.Compute:
                        Renderer.Context.CSSetShaderResource(slot, texture);
                        break;
                    case ShaderStage.Geometry:
                        Renderer.Context.GSSetShaderResource(slot, texture);
                        break;
                    case ShaderStage.Hull:
                        Renderer.Context.HSSetShaderResource(slot, texture);
                        break;
                    case ShaderStage.Domain:
                        Renderer.Context.DSSetShaderResource(slot, texture);
                        break;
                }
            }
        }

        private byte[] CompileShader(string shaderPath, string entryPoint, string shaderModel)
        {
            if (!File.Exists(shaderPath))
                throw new FileNotFoundException($"Shader file not found: {shaderPath}");

            Result compileResult = Compiler.CompileFromFile(shaderPath, entryPoint, shaderModel, out Blob bytecode, out Blob errorBlob);

            if (compileResult.Failure)
            {
                string errorMsg = errorBlob != null ? errorBlob.AsString() : "Unknown error";
                errorBlob?.Dispose();
                throw new Exception($"Failed to compile {shaderModel} shader: {errorMsg}");
            }

            byte[] shaderByteCode = bytecode.AsSpan().ToArray();
            errorBlob?.Dispose();

            if (shaderModel == "vs_5_0")
                VertexBlob = bytecode;
            else
                bytecode.Dispose();

            return shaderByteCode;
        }

        public new void CleanUp()
        {
            VertexShader?.Dispose();
            PixelShader?.Dispose();
            ComputeShader?.Dispose();
            GeometryShader?.Dispose();
            HullShader?.Dispose();
            DomainShader?.Dispose();

            InputLayout?.Dispose();
            VertexBlob?.Dispose();
        }

        public static Shader Create(string name, DomainedPath domainedPath)
        {
            Shader result = new()
            {
                Name = name,
                DomainedPath = domainedPath,
                VertexPath = domainedPath.FullPath + "Vertex.hlsl",
                PixelPath = domainedPath.FullPath + "Pixel.hlsl",
                ComputePath = domainedPath.FullPath + "Compute.hlsl",
                GeometryPath = domainedPath.FullPath + "Geometry.hlsl",
                HullPath = domainedPath.FullPath + "Hull.hlsl",
                DomainPath = domainedPath.FullPath + "Domain.hlsl"
            };

            result.Generate();

            return result;
        }
    }
}
