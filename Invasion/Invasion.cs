using Invasion.ECS;
using Invasion.Page;
using Invasion.Render;
using System;
using System.Numerics;
using System.Windows.Forms;

namespace Invasion
{
    public static class InvasionMain
    {
        public static Window? Window { get; private set; }

        public static GameObject Square { get; private set; } = null!;

        public static void Initialize()
        {
            Renderer.Initialize(Window!);

            ShaderManager.Register(Shader.Create("default", new("Shader/Default", "Invasion")));
            TextureManager.Register(Texture.Create("debug", new("Texture/Debug.dds", "Invasion"), new()
            {
                Filter = Vortice.Direct3D11.Filter.MinMagMipPoint,
                AddressU = Vortice.Direct3D11.TextureAddressMode.Wrap,
                AddressV = Vortice.Direct3D11.TextureAddressMode.Wrap,
                AddressW = Vortice.Direct3D11.TextureAddressMode.Wrap,
                ComparisonFunc = Vortice.Direct3D11.ComparisonFunction.Never,
                MinLOD = 0,
                MaxLOD = float.MaxValue
            }));

            Square = GameObject.Create("Square");

            Square.AddComponent(ShaderManager.Get("default"));
            Square.AddComponent(TextureManager.Get("debug"));

            Square.AddComponent(Mesh.Create("Square",
            [
                new(new(-0.5f, -0.5f, 0.0f), Vector3.One, Vector3.UnitZ, new(0.0f, 0.0f)),
                new(new(-0.5f,  0.5f, 0.0f), Vector3.One, Vector3.UnitZ, new(0.0f, 1.0f)),
                new(new( 0.5f,  0.5f, 0.0f), Vector3.One, Vector3.UnitZ, new(1.0f, 1.0f)),
                new(new( 0.5f, -0.5f, 0.0f), Vector3.One, Vector3.UnitZ, new(1.0f, 0.0f)),
            ],
            [
                0, 1, 2,
                0, 2, 3
            ]));

            Square.GetComponent<Mesh>().Generate();
        }

        public static void Update(object? s, EventArgs a)
        {
            GameObjectManager.Update();
        }

        public static void Resize(object? s, EventArgs a)
        {
            Renderer.Resize();
        }

        public static void Render(object? s, EventArgs a)
        {
            Renderer.PreRender();

            Square.Render();
            GameObjectManager.Render();

            Renderer.PostRender();
        }

        public static void CleanUp(object? s, EventArgs a)
        {
            GameObjectManager.CleanUp();
            TextureManager.CleanUp();
            ShaderManager.CleanUp();
            Renderer.CleanUp();
        }

        public static void Main()
        {
            Window = new Window();

            Window.Update += Update;
            Window.Resize += Resize;
            Window.Render += Render;
            Window.CleanUp += CleanUp;

            Initialize();

            Application.EnableVisualStyles();
            Application.Run(Window);
        }
    }
}