using DirectXTexNet;
using Invasion.ECS;
using Invasion.Render;
using Invasion.Util;
using SharpGen.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Invasion.World
{
    public class TextureAtlas : Component
    {
        public string Name { get; private set; } = string.Empty;

        public Texture Atlas { get; private set; } = null!;

        public DomainedPath InputDirectory { get; private set; } = null!;
        public DomainedPath OutputDirectory { get; private set; } = null!;

        private Dictionary<string, Vector2[]> SubTextureCoordinates { get; } = [];
        private ScratchImage? AtlasScratchImage { get; set; } = null;

        private const float UV_OFFSET = 0.003f;

        private TextureAtlas() { }

        public Vector2[] GetTextureCoordinates(string textureName)
        {
            if (SubTextureCoordinates.TryGetValue(textureName, out var coords))
                return coords;

            throw new KeyNotFoundException($"Texture '{textureName}' not found in atlas.");
        }

        private void Generate()
        {
            string[] ddsFiles = Directory.GetFiles(InputDirectory.FullPath, "*.dds");

            if (ddsFiles.Length == 0)
                throw new Exception("No .dds files found in the input directory.");

            List<(ScratchImage image, string name)> textures = new();

            foreach (var file in ddsFiles)
            {
                var textureName = Path.GetFileNameWithoutExtension(file);
                ScratchImage loadedImage = TexHelper.Instance.LoadFromDDSFile(file, DDS_FLAGS.NONE);
                textures.Add((loadedImage, textureName));
            }

            int totalArea = textures.Sum(t => t.image.GetMetadata().Width * t.image.GetMetadata().Height);

            int atlasDimension = (int)MathF.Ceiling(MathF.Sqrt(totalArea)) * 2;

            AtlasScratchImage = TexHelper.Instance.Initialize2D(DXGI_FORMAT.R32G32B32A32_FLOAT, atlasDimension, atlasDimension, 1, 1, CP_FLAGS.NONE);

            int currentX = 0;
            int currentY = 0;
            int maxRowHeight = 0;

            foreach (var (image, name) in textures)
            {
                var metadata = image.GetMetadata();
                int textureWidth = (int)metadata.Width;
                int textureHeight = (int)metadata.Height;

                if (currentX + textureWidth > atlasDimension)
                {
                    currentX = 0;
                    currentY += maxRowHeight;
                    maxRowHeight = 0;
                }

                if (currentY + textureHeight > atlasDimension)
                    throw new Exception("Texture atlas is too small to fit all textures.");

                TexHelper.Instance.CopyRectangle(image.GetImage(0, 0, 0), 0, 0, textureWidth, textureHeight, AtlasScratchImage.GetImage(0, 0, 0), TEX_FILTER_FLAGS.DEFAULT, currentX, currentY);

                float uMin = (float)currentX / atlasDimension;
                float vMin = (float)currentY / atlasDimension;
                float uMax = (float)(currentX + textureWidth) / atlasDimension;
                float vMax = (float)(currentY + textureHeight) / atlasDimension;

                float paddingX = UV_OFFSET / atlasDimension;
                float paddingY = UV_OFFSET / atlasDimension;

                uMin += paddingX;
                vMin += paddingY;
                uMax -= paddingX;
                vMax -= paddingY;

                SubTextureCoordinates[name] =
                [
                    new Vector2(uMax, vMax),
                    new Vector2(uMin, vMax),
                    new Vector2(uMin, vMin),
                    new Vector2(uMax, vMin)
                ];

                currentX += textureWidth;
                maxRowHeight = (int)MathF.Max(maxRowHeight, textureHeight);
            }

            SaveAtlas();
        }

        private void SaveAtlas()
        {
            if (AtlasScratchImage == null) 
                throw new Exception("Atlas image is null. Cannot save.");

            string saveName = string.Concat(Name[0].ToString().ToUpper(), Name.AsSpan(1));

            string atlasPath = Path.Combine(OutputDirectory.FullPath, $"{saveName}Atlas.dds");

            AtlasScratchImage.SaveToDDSFile(DDS_FLAGS.NONE, atlasPath);

            Atlas = Texture.Create(Name, new(atlasPath), new()
            {
                Filter = Vortice.Direct3D11.Filter.MinMagMipPoint,
                AddressU = Vortice.Direct3D11.TextureAddressMode.Wrap,
                AddressV = Vortice.Direct3D11.TextureAddressMode.Wrap,
                AddressW = Vortice.Direct3D11.TextureAddressMode.Wrap,
                ComparisonFunc = Vortice.Direct3D11.ComparisonFunction.Never,
                MinLOD = 0,
                MaxLOD = float.MaxValue
            });
        }

        public new void CleanUp()
        {
            AtlasScratchImage?.Dispose();
            Atlas?.CleanUp();
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