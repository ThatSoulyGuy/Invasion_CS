﻿using DirectXTexNet;
using Invasion.ECS;
using Invasion.Util;
using System;
using System.IO;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Invasion.Render
{
    public class TextureCube : Component
    {
        public string Name { get; private set; } = string.Empty;
        public DomainedPath DomainedPath { get; set; } = null!;

        public SamplerDescription SamplerDescription { get; private set; }

        public ID3D11ShaderResourceView? ShaderResourceView { get; private set; } = null;
        public ID3D11SamplerState? SamplerState { get; private set; } = null;

        private ID3D11Texture2D? TextureReference { get; set; } = null!;

        private TextureCube() { }

        public void Bind(uint slot)
        {
            if (ShaderResourceView != null)
                Renderer.Context.PSSetShaderResource(slot, ShaderResourceView);

            if (SamplerState != null)
                Renderer.Context.PSSetSampler(slot, SamplerState);
        }

        private void Generate()
        {
            if (!File.Exists(DomainedPath.FullPath))
                throw new FileNotFoundException($"Texture file not found: {DomainedPath.FullPath}");

            ScratchImage image = TexHelper.Instance.LoadFromDDSFile(DomainedPath.FullPath, DDS_FLAGS.NONE)
                ?? throw new Exception($"Failed to load texture: {DomainedPath.FullPath}, Error: -32768");

            TexMetadata metadata = image.GetMetadata();

            if (!metadata.IsCubemap())
            {
                image.Dispose();
                throw new Exception($"The texture is not a cube map: {DomainedPath.FullPath}");
            }

            if (metadata.Dimension != TEX_DIMENSION.TEXTURE2D)
            {
                image.Dispose();
                throw new Exception($"Unsupported texture dimension for cube maps: {DomainedPath.FullPath}");
            }

            Texture2DDescription textureDescription = new()
            {
                Width = (uint)metadata.Width,
                Height = (uint)metadata.Height,
                MipLevels = (uint)metadata.MipLevels,
                ArraySize = (uint)metadata.ArraySize,
                Format = (Format)metadata.Format,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.TextureCube
            };

            int subresourceCount = (int)(metadata.MipLevels * metadata.ArraySize);
            SubresourceData[] subresourceData = new SubresourceData[subresourceCount];

            for (int i = 0; i < subresourceCount; i++)
            {
                Image imageData = image.GetImage(i);

                if (imageData == null)
                {
                    image.Dispose();
                    throw new Exception($"Failed to retrieve image data for subresource {i}");
                }

                subresourceData[i] = new()
                {
                    DataPointer = imageData.Pixels,
                    RowPitch = (uint)imageData.RowPitch,
                    SlicePitch = (uint)imageData.SlicePitch
                };
            }

            TextureReference = Renderer.Device.CreateTexture2D(textureDescription, subresourceData);

            ShaderResourceViewDescription shaderResourceViewDescription = new()
            {
                Format = textureDescription.Format,
                ViewDimension = ShaderResourceViewDimension.TextureCube,
                TextureCube = new TextureCubeShaderResourceView()
                {
                    MipLevels = textureDescription.MipLevels,
                    MostDetailedMip = 0
                }
            };

            ShaderResourceView = Renderer.Device.CreateShaderResourceView(TextureReference, shaderResourceViewDescription);

            image.Dispose();

            CreateSamplerState();
        }

        private void CreateSamplerState()
        {
            SamplerState = Renderer.Device.CreateSamplerState(SamplerDescription);
        }

        public new void CleanUp()
        {
            TextureReference?.Dispose();
            ShaderResourceView?.Dispose();
            SamplerState?.Dispose();
        }

        public static TextureCube Create(string name, DomainedPath domainedPath, SamplerDescription samplerDescription)
        {
            TextureCube result = new()
            {
                Name = name,
                DomainedPath = domainedPath,
                SamplerDescription = samplerDescription
            };

            result.Generate();

            return result;
        }

        public static TextureCube Create(ID3D11Texture2D texture, ID3D11ShaderResourceView shaderResourceView)
        {
            TextureCube result = new()
            {
                TextureReference = texture,
                ShaderResourceView = shaderResourceView
            };

            result.CreateSamplerState();

            return result;
        }
    }
}