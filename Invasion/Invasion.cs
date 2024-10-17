using Invasion.Core;
using Invasion.ECS;
using Invasion.Entity.Entities;
using Invasion.Entity.Models;
using Invasion.Math;
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
        public static GameObject Pig { get; private set; } = null!;

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
            TextureAtlasManager.Register(TextureAtlas.Create("entities", new("Texture/Entity", "Invasion"), new("Texture/Atlas", "Invasion")));

            Overworld = GameObject.Create("Overworld");
            Overworld.AddComponent(IWorld.Create("overworld"));

            Player = GameObject.Create("Player");
            Player.Transform.LocalPosition = new(0.0f, 60.0f, 0.0f);

            Player.AddComponent(BoundingBox.Create(new(0.6f, 1.89f, 0.6f)));
            Player.AddComponent(Rigidbody.Create());
            Player.AddComponent(new EntityPlayer());

            Pig = GameObject.Create("Pig");
            Pig.Transform.LocalPosition = new(0.0f, 50.0f, 0.0f);

            Pig.AddComponent(BoundingBox.Create(new(0.95f, 0.95f, 0.95f)));
            Pig.AddComponent(Rigidbody.Create());
            Pig.AddComponent(new ModelPig());
            Pig.AddComponent(new EntityPig());
        }

        public static void Update(object? s, EventArgs a)
        {
            Overworld.GetComponent<IWorld>().LoaderPositions = [Player.Transform.WorldPosition];

            Pig.GetComponent<EntityPig>().PlayerPosition = Player.Transform.WorldPosition;

            InputManager.Update();
            GameObjectManager.Update();
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
#if RELEASE
            if (!File.Exists(Application.CommonAppDataPath + "/" + "Invasion.nomodify"))
            {
                Console.WriteLine("Invasion***");
                Console.WriteLine("Initializing critical systems...");
                Thread.Sleep(1500);
                Console.WriteLine("ERROR! Failed to verify secure protocol...");
                Thread.Sleep(800);
                Console.WriteLine("Retrying secure handshake...");
                Thread.Sleep(1200);
                Console.WriteLine("DEPLOYMENT_STATUS: {ERROR} [Code: 0x89B2]");
                Thread.Sleep(1000);
                Console.WriteLine("!!! ALERT: Unauthorized access detected !!!");
                Thread.Sleep(500);
                Console.WriteLine("0xAB34D, 0x76A9C, 0xEF5C2, 0x9834H9");
                Thread.Sleep(300);
                Console.WriteLine("Attempting countermeasure deployment...");
                Thread.Sleep(700);
                Console.WriteLine("VIRUS_STATUS: {RUNNING}");
                Thread.Sleep(500);
                Console.WriteLine("Injecting payload 0x67234...");
                Thread.Sleep(1500);
                Console.WriteLine("System integrity compromised. Retrying...");
                Thread.Sleep(1000);
                Console.WriteLine("Retried V_DBG_ANNOY_H3 protocol {7} times! FAILURE.");
                Thread.Sleep(2500);
                Console.WriteLine("Shutting down non-essential services...");
                Thread.Sleep(1000);
                Console.WriteLine("ERROR: Shutdown command ignored. Unauthorized access escalating...");
                Thread.Sleep(2000);
                Console.WriteLine("FATAL ERROR: System breach imminent.");
                Thread.Sleep(1500);
                Console.WriteLine("0x483F7, 0x12CD9, 0xBF983, 0x03A17D");
                Thread.Sleep(800);
                Console.WriteLine("Critical failure. Entering lockdown mode...");
                Thread.Sleep(1500);
                Console.WriteLine("THREAD_INTERRUPT {0x4857F}, User interrupted thread, proceeding with initialization\"\"\"");
                Thread.Sleep(3000);

                File.WriteAllText(Application.CommonAppDataPath + "/" + "Invasion.nomodify", "Completed");
            }
#endif

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