using DirectXTexNet;
using Invasion.Util;
using SharpGen.Runtime;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Invasion.Render
{
    public class Texture
    {
        public string Name { get; private set; } = string.Empty;
        public DomainedPath DomainedPath { get; set; } = null!;

        public ID3D11ShaderResourceView? ShaderResourceView { get; private set; } = null;

        private ID3D11Texture2D? TextureReference { get; set; } = null!;

        private Texture() { }

        public void Bind(int slot)
        {
            if (ShaderResourceView != null)
                Renderer.Context.PSSetShaderResource(slot, ShaderResourceView);
        }

        private void Generate()
        {
            if (!File.Exists(DomainedPath.FullPath))
                throw new FileNotFoundException($"Texture file not found: {DomainedPath.FullPath}");

            ScratchImage image = TexHelper.Instance.LoadFromDDSFile(DomainedPath.FullPath, DDS_FLAGS.NONE) ?? throw new Exception($"Failed to load texture: {DomainedPath.FullPath}, Error: -32768");
            
            if (image.GetMetadata().Dimension != TEX_DIMENSION.TEXTURE2D)
            {
                image.Dispose();
                throw new Exception($"Unsupported texture dimension. Only 2D textures are supported: {DomainedPath.FullPath}");
            }

            TexMetadata metadata = image.GetMetadata();

            Texture2DDescription textureDesc = new()
            {
                Width = metadata.Width,
                Height = metadata.Height,
                MipLevels = metadata.MipLevels,
                ArraySize = metadata.ArraySize,
                Format = (Format)metadata.Format,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None
            };

            int subresourceCount = (metadata.MipLevels * metadata.ArraySize);
            SubresourceData[] subresourceData = new SubresourceData[subresourceCount];

            for (int i = 0; i < subresourceCount; i++)
            {
                Image imageData = image.GetImage(i);

                if (imageData == null)
                {
                    image.Dispose();
                    throw new Exception($"Failed to retrieve image data for subresource {i}");
                }

                subresourceData[i] = new SubresourceData
                {
                    DataPointer = imageData.Pixels,
                    RowPitch = (int)imageData.RowPitch,
                    SlicePitch = (int)imageData.SlicePitch
                };
            }
           
            TextureReference = Renderer.Device.CreateTexture2D(textureDesc, subresourceData);

            ShaderResourceViewDescription shaderResourceViewDescription = new()
            {
                Format = textureDesc.Format,
                ViewDimension = ShaderResourceViewDimension.Texture2D,
                Texture2D = new()
                {
                    MipLevels = textureDesc.MipLevels,
                    MostDetailedMip = 0
                }
            };

            ShaderResourceView = Renderer.Device.CreateShaderResourceView(TextureReference, shaderResourceViewDescription);

            image.Dispose();
        }

        public void CleanUp()
        {
            TextureReference?.Dispose();
            ShaderResourceView?.Dispose();
        }

        public static Texture Create(string name, DomainedPath domainedPath)
        {
            Texture result = new()
            {
                Name = name,
                DomainedPath = domainedPath
            };

            result.Generate();

            return result;
        }
    }
}