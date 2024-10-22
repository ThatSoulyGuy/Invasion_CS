using Invasion.Math;
using Invasion.Render;
using Invasion.Util;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Runtime.InteropServices;
using Vortice.Direct3D11;
using Vortice.DXGI;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace Invasion.UI.Elements
{
    public class UIText : UIElement
    {
        private string _text = string.Empty;
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    GenerateTexture();
                }
            }
        }

        private float _fontSize = 30.0f;
        public float FontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize != value)
                {
                    _fontSize = value;
                    Font = Family.CreateFont(_fontSize, Style);
                    GenerateTexture();
                }
            }
        }

        private FontStyle _style = FontStyle.Regular;
        public FontStyle Style
        {
            get => _style;
            set
            {
                if (_style != value)
                {
                    _style = value;
                    Font = Family.CreateFont(FontSize, _style);
                    GenerateTexture();
                }
            }
        }

        private static FontCollection FontCollection { get; } = new();

        private FontFamily Family { get; set; }
        private Font Font { get; set; }

        public UIText(string name, DomainedPath fontPath, Vector2f position, Vector2f size) : base(name, position, size)
        {
            if (!FontCollection.TryGet(fontPath.FullPath, out var existingFamily))
                Family = FontCollection.Add(fontPath.FullPath);
            else
                Family = existingFamily;

            Font = Family.CreateFont(FontSize, Style);

            GenerateTexture();
        }

        private void GenerateTexture()
        {
            Mesh.Texture?.CleanUp();

            float scaleFactor = 2.0f;
            int width = (int)System.Math.Ceiling(Size.X * scaleFactor);
            int height = (int)System.Math.Ceiling(Size.Y * scaleFactor);

            using var image = new Image<Rgba32>(width, height);

            image.Mutate(ctx =>
            {
                ctx.Clear(Color.Transparent);

                ctx.DrawText(Text, Font, Color.White, new PointF(0, 0));
            });

            var pixels = new Rgba32[width * height];
            image.CopyPixelDataTo(pixels);

            GCHandle handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);

            try
            {
                var dataPointer = handle.AddrOfPinnedObject();

                var textureDesc = new Texture2DDescription
                {
                    Width = (uint)width,
                    Height = (uint)height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = Format.R8G8B8A8_UNorm,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.ShaderResource,
                    CPUAccessFlags = CpuAccessFlags.None,
                    MiscFlags = ResourceOptionFlags.None
                };

                var subresourceData = new SubresourceData
                {
                    DataPointer = dataPointer,
                    RowPitch = (uint)width * sizeof(uint),
                    SlicePitch = 0
                };

                var device = Renderer.Device;

                var textureResource = device.CreateTexture2D(textureDesc, new[] { subresourceData });

                var shaderResourceView = device.CreateShaderResourceView(textureResource);

                Mesh.Texture = Texture.Create(textureResource, shaderResourceView);

                Mesh.Generate();
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
