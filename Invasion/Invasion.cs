using Invasion.Core;
using Invasion.ECS;
using Invasion.Entity.Entities;
using Invasion.Page;
using Invasion.Render;
using Invasion.World;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Invasion
{
    public static class InvasionMain
    {
        public static Window? Window { get; private set; }

        public static GameObject Overworld { get; private set; } = null!;

        public static GameObject Player { get; private set; } = null!;

        public static void Initialize()
        {
            Renderer.Initialize(Window!);

            InputManager.Initialize(Window!);

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

            TextureAtlasManager.Register(TextureAtlas.Create("blocks", new("Texture/Block", "Invasion"), new("Texture/Atlas", "Invasion")));

            Overworld = GameObject.Create("Overworld");
            Overworld.AddComponent(IWorld.Create("overworld"));

            Player = GameObject.Create("Player");
            Player.Transform.LocalPosition = new(0.0f, 20.0f, 0.0f);
            Player.AddComponent(new EntityPlayer());
        }

        public static void Update(object? s, EventArgs a)
        {
            Overworld.GetComponent<IWorld>().LoaderPositions = [Player.Transform.WorldPosition];

            InputManager.Update();
            GameObjectManager.Update();
            
            Overworld.GetComponent<IWorld>().LoadReadyChunks();
            Overworld.GetComponent<IWorld>().UnloadReadyChunks();
        }

        public static void Resize(object? s, EventArgs a)
        {
            Renderer.Resize();
        }

        public static void Render(object? s, EventArgs a)
        {
            Renderer.PreRender();

            GameObjectManager.Render(Player.GetComponent<EntityPlayer>().RenderCamera.GetComponent<Camera>());

            Renderer.PostRender(); 
        }

        public static void CleanUp(object? s, EventArgs a)
        {
            GameObjectManager.CleanUp();
            TextureManager.CleanUp();
            ShaderManager.CleanUp();
            TextureAtlasManager.Cleanup();
            Renderer.CleanUp();
        }

        [DllImport("user32.dll")]
        private static extern bool SetProcessDpiAwarenessContext(int dpiFlag);

        private const int DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4;
        private const int DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = -3;
        private const int DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = -2;
        private const int DPI_AWARENESS_CONTEXT_UNAWARE = -1;

        public static void Main()
        {
            SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);

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