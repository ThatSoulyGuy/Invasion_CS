using Invasion.Core;
using Invasion.ECS;
using Invasion.Entity.Entities;
using Invasion.Page;
using Invasion.Render;
using Invasion.Thread;
using Invasion.UI;
using Invasion.World;
using Invasion.World.SpawnManagers;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Invasion
{
    public static class InvasionMain
    {
        public static Window? Window { get; private set; }

        public static GameObject Overworld { get; private set; } = null!;

        public static GameObject SkyboxObject { get; private set; } = null!;
        public static GameObject Player { get; private set; } = null!;

        public static void Initialize()
        {
            Renderer.Initialize(Window!);

            InputManager.Initialize(Window!);

            ShaderManager.Register(Shader.Create("default", new("Shader/Default", "Invasion")));
            ShaderManager.Register(Shader.Create("ui", new("Shader/UI", "Invasion")));
            ShaderManager.Register(Shader.Create("line", new("Shader/Line", "Invasion")));
            ShaderManager.Register(Shader.Create("skybox", new("Shader/Skybox", "Invasion")));

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

            TextureManager.Register(Texture.Create("laser_gun", new("Texture/Item/LaserGun.dds", "Invasion"), new()
            {
                Filter = Vortice.Direct3D11.Filter.MinMagMipPoint,
                AddressU = Vortice.Direct3D11.TextureAddressMode.Wrap,
                AddressV = Vortice.Direct3D11.TextureAddressMode.Wrap,
                AddressW = Vortice.Direct3D11.TextureAddressMode.Wrap,
                ComparisonFunc = Vortice.Direct3D11.ComparisonFunction.Never,
                MinLOD = 0,
                MaxLOD = float.MaxValue
            }));

            TextureManager.Register(Texture.Create("hotbar_background", new("Texture/UI/LargeSlotBackground.dds", "Invasion"), new()
            {
                Filter = Vortice.Direct3D11.Filter.MinMagMipPoint,
                AddressU = Vortice.Direct3D11.TextureAddressMode.Wrap,
                AddressV = Vortice.Direct3D11.TextureAddressMode.Wrap,
                AddressW = Vortice.Direct3D11.TextureAddressMode.Wrap,
                ComparisonFunc = Vortice.Direct3D11.ComparisonFunction.Never,
                MinLOD = 0,
                MaxLOD = float.MaxValue
            }));

            TextureManager.Register(Texture.Create("hotbar_selector", new("Texture/UI/SlotSelector.dds", "Invasion"), new()
            {
                Filter = Vortice.Direct3D11.Filter.MinMagMipPoint,
                AddressU = Vortice.Direct3D11.TextureAddressMode.Wrap,
                AddressV = Vortice.Direct3D11.TextureAddressMode.Wrap,
                AddressW = Vortice.Direct3D11.TextureAddressMode.Wrap,
                ComparisonFunc = Vortice.Direct3D11.ComparisonFunction.Never,
                MinLOD = 0,
                MaxLOD = float.MaxValue
            }));

            TextureManager.Register(Texture.Create("crosshair", new("Texture/UI/Crosshair.dds", "Invasion"), new()
            {
                Filter = Vortice.Direct3D11.Filter.MinMagMipPoint,
                AddressU = Vortice.Direct3D11.TextureAddressMode.Wrap,
                AddressV = Vortice.Direct3D11.TextureAddressMode.Wrap,
                AddressW = Vortice.Direct3D11.TextureAddressMode.Wrap,
                ComparisonFunc = Vortice.Direct3D11.ComparisonFunction.Never,
                MinLOD = 0,
                MaxLOD = float.MaxValue
            }));

            TextureManager.Register(TextureCube.Create("stars", new("Texture/Skybox/cubemap.dds", "Invasion"), new()
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
            TextureAtlasManager.Register(TextureAtlas.Create("entities", new("Texture/Entity", "Invasion"), new("Texture/Atlas", "Invasion")));

            SkyboxObject = GameObject.Create("Skybox");

            SkyboxObject.AddComponent(ShaderManager.Get("skybox"));
            SkyboxObject.AddComponent(TextureManager.Get<TextureCube>("stars"));
            SkyboxObject.AddComponent(Skybox.Create());

            SkyboxObject.GetComponent<Skybox>().Generate(Renderer.Device);

            SkyboxObject.Active = false;

            Overworld = GameObject.Create("Overworld");
            Overworld.AddComponent(IWorld.Create("overworld"));

            Overworld.GetComponent<IWorld>().AddSpawnManager(new SpawnManagerGoober());

            Player = Overworld.GetComponent<IWorld>().SpawnEntity<EntityPlayer>(new(0.0f, 60.0f, 0.0f));
        }

        public static void Update(object? s, EventArgs a)
        {
            Overworld.GetComponent<IWorld>().LoaderPositions = [Player.Transform.WorldPosition];

            UIManager.Update();
            GameObjectManager.Update();
            InputManager.Update();
        }

        public static void Resize(object? s, EventArgs a)
        {
            Renderer.Resize();
        }

        public static void Render(object? s, EventArgs a)
        {
            Renderer.PreRender();

            GameObjectManager.Render(Player.GetComponent<EntityPlayer>().RenderCamera.GetComponent<Camera>());
            SkyboxObject.GetComponent<Skybox>().Render(Player.GetComponent<EntityPlayer>().RenderCamera.GetComponent<Camera>());
            UIManager.Render();

            Renderer.PostRender(); 
        }

        public static void CleanUp(object? s, EventArgs a)
        {
            Time.CleanUp();
            GameObject.CleanUpThreadPool();
            UIManager.CleanUp();
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