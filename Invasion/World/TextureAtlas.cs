using DirectXTexNet;
using Invasion.ECS;
using Invasion.Render;
using Invasion.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Invasion.World
{
    public class TextureAtlas : Component
    {
        public string Name { get; private set; } = string.Empty;

        public Texture Atlas { get; private set; } = null!;

        public DomainedPath InputDirectory { get; private set; } = null!;
        public DomainedPath OutputDirectory { get; private set; } = null!;

        private const float Padding = 0.002f;
        private Dictionary<string, Vector2[]> SubTextureCoordinates = [];

        private TextureAtlas() { }

        public Vector2[] GetSubTextureCoordinates(string textureName)
        {
            if (SubTextureCoordinates.TryGetValue(textureName, out Vector2[]? value))
                return value;

            throw new ArgumentException($"Texture '{textureName}' not found in atlas.");
        }

        private void Generate()
        {
            string[] textureFiles = Directory.GetFiles(InputDirectory.FullPath, "*.dds");

            List<(ScratchImage Image, string Name)> textures = [];

            foreach (var textureFile in textureFiles)
            {
                string textureName = Path.GetFileNameWithoutExtension(textureFile);
                ScratchImage image = TexHelper.Instance.LoadFromDDSFile(textureFile, DDS_FLAGS.NONE);
                textures.Add((image, textureName));
            }

            int atlasSize = CalculateAtlasSize(textures);
            ID3D11Texture2D atlasTexture;

            do
            {
                try
                {
                    atlasTexture = CreateTextureAtlas(textures, atlasSize, atlasSize, out SubTextureCoordinates);
                    break;
                }
                catch (Exception)
                {
                    atlasSize *= 2;
                }
            } 
            while (true);

            string upperCaseName = char.ToUpper(Name[0]) + Name.Substring(1);
            string outputFile = Path.Combine(OutputDirectory.FullPath, $"{upperCaseName}Atlas.dds");

            SaveTextureToFile(atlasTexture, outputFile);
        }

        private int CalculateAtlasSize(List<(ScratchImage Image, string Name)> textures)
        {
            int totalArea = 0;

            foreach (var (image, _) in textures)
            {
                TexMetadata metadata = image.GetMetadata();
                totalArea += metadata.Width * metadata.Height;
            }

            return (int)MathF.Ceiling(MathF.Sqrt(totalArea));
        }

        private ID3D11Texture2D CreateTextureAtlas(List<(ScratchImage Image, string Name)> textures, int atlasWidth, int atlasHeight, out Dictionary<string, Vector2[]> coordinates)
        {
            coordinates = [];
            int x = 0, y = 0, rowHeight = 0;

            Texture2DDescription atlasDescription = new()
            {
                Width = atlasWidth,
                Height = atlasHeight,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R8G8B8A8_UNorm,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Dynamic,
                BindFlags = BindFlags.ShaderResource,
                CPUAccessFlags = CpuAccessFlags.Write,
                MiscFlags = ResourceOptionFlags.None
            };

            ID3D11Texture2D atlasTexture = Renderer.Device.CreateTexture2D(atlasDescription);

            foreach (var (image, name) in textures)
            {
                TexMetadata metadata = image.GetMetadata();
                int width = metadata.Width;
                int height = metadata.Height;

                if (x + width > atlasWidth)
                {
                    x = 0;
                    y += rowHeight;
                    rowHeight = 0;
                }

                if (y + height > atlasHeight)
                    throw new Exception("Atlas size is too small to fit all textures.");
                
                CopySubTextureToAtlas(atlasTexture, image, x, y);

                float paddedX = x + Padding * atlasWidth;
                float paddedY = y + Padding * atlasHeight;
                float paddedWidth = width - 2 * Padding * atlasWidth;
                float paddedHeight = height - 2 * Padding * atlasHeight;

                Vector2 topLeft = new(paddedX / atlasWidth, paddedY / atlasHeight);
                Vector2 topRight = new((paddedX + paddedWidth) / atlasWidth, paddedY / atlasHeight);
                Vector2 bottomRight = new((paddedX + paddedWidth) / atlasWidth, (paddedY + paddedHeight) / atlasHeight);
                Vector2 bottomLeft = new(paddedX / atlasWidth, (paddedY + paddedHeight) / atlasHeight);

                coordinates[name] = [topLeft, topRight, bottomRight, bottomLeft];

                x += width;
                rowHeight = (int)MathF.Max(rowHeight, height);
            }

            foreach (var (image, _) in textures)
                image.Dispose();

            return atlasTexture;
        }

        private void CopySubTextureToAtlas(ID3D11Texture2D atlasTexture, ScratchImage subImage, int x, int y)
        {
            Image subImageData = subImage.GetImage(0);

            var context = Renderer.Context;

            MappedSubresource mappedResource = context.Map(atlasTexture, 0, MapMode.WriteDiscard, Vortice.Direct3D11.MapFlags.None);

            for (int row = 0; row < subImageData.Height; row++)
            {
                IntPtr sourceRowPointer = IntPtr.Add(subImageData.Pixels, row * (int)subImageData.RowPitch);
                IntPtr destRowPointer = IntPtr.Add(mappedResource.DataPointer, ((y + row) * mappedResource.RowPitch) + (x * 4));

                byte[] rowData = new byte[subImageData.Width * 4];

                Marshal.Copy(sourceRowPointer, rowData, 0, rowData.Length);
                Marshal.Copy(rowData, 0, destRowPointer, rowData.Length);
            }

            context.Unmap(atlasTexture, 0);
        }

        private void SaveTextureToFile(ID3D11Texture2D texture, string outputFile)
        {
            Texture2DDescription textureDescription = texture.Description;

            textureDescription.Usage = ResourceUsage.Staging;
            textureDescription.CPUAccessFlags = CpuAccessFlags.Read;
            textureDescription.BindFlags = BindFlags.None;
            textureDescription.MiscFlags = ResourceOptionFlags.None;
            textureDescription.SampleDescription.Count = 1;

            ID3D11Texture2D stagingTexture = Renderer.Device.CreateTexture2D(textureDescription);

            Renderer.Context.CopyResource(stagingTexture, texture);

            MappedSubresource mappedResource = Renderer.Context.Map(stagingTexture, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);

            TexMetadata metadata = new(textureDescription.Width, textureDescription.Height, 1, 1, 1, TEX_MISC_FLAG.TEXTURECUBE, TEX_MISC_FLAG2.ALPHA_MODE_MASK, (DXGI_FORMAT)textureDescription.Format, TEX_DIMENSION.TEXTURE2D);

            ScratchImage scratchImage = TexHelper.Instance.Initialize2D(metadata.Format, metadata.Width, metadata.Height, metadata.ArraySize, metadata.MipLevels, CP_FLAGS.NONE);
            Image image = scratchImage.GetImage(0);

            IntPtr dataPointer = mappedResource.DataPointer;

            for (int row = 0; row < image.Height; row++)
            {
                IntPtr destination = IntPtr.Add(image.Pixels, row * (int)image.RowPitch);
                IntPtr source = IntPtr.Add(dataPointer, row * mappedResource.RowPitch);

                byte[] rowData = new byte[image.RowPitch];

                Marshal.Copy(source, rowData, 0, rowData.Length);
                Marshal.Copy(rowData, 0, destination, rowData.Length);
            }

            scratchImage.SaveToDDSFile(DDS_FLAGS.NONE, outputFile);

            Renderer.Context.Unmap(stagingTexture, 0);

            scratchImage.Dispose();
            stagingTexture.Dispose();

            Atlas = Texture.Create(Name, new(outputFile), new()
            {
                Filter = Filter.MinMagMipPoint,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                ComparisonFunc = ComparisonFunction.Never,
                MinLOD = 0,
                MaxLOD = float.MaxValue
            });
        }

        public new void CleanUp()
        {
            Atlas.CleanUp();
        }

        public static TextureAtlas Create(string name, DomainedPath inputDirectory, DomainedPath outputDirectory)
        {
            TextureAtlas result = new()
            {
                Name = name,
                InputDirectory = inputDirectory,
                OutputDirectory = outputDirectory
            };

            result.Generate();

            return result;
        }
    }
}