using SharpGen.Runtime;
using System;
using System.Windows.Forms;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Invasion.Render
{
    public static class Renderer
    {
        public static ID3D11Device5 Device { get; private set; } = null!;
        public static ID3D11DeviceContext4 Context { get; private set; } = null!;
        public static IDXGISwapChain4 SwapChain { get; private set; } = null!;
        public static ID3D11RenderTargetView1 RenderTargetView { get; private set; } = null!;
        public static ID3D11DepthStencilView DepthStencilView { get; private set; } = null!;
        public static ID3D11DepthStencilState NoDepthStencilState { get; private set; } = null!;
        public static ID3D11BlendState AlphaBlendState { get; private set; } = null!;

        public static IDXGIFactory6 DxgiFactory { get; private set; } = null!;
        public static IDXGIAdapter4 Adapter { get; private set; } = null!;

        public static Form Window { get; private set; } = null!;

        public static void Initialize(Form form)
        {
            Window = form;

            Result factoryResult = DXGI.CreateDXGIFactory1(out IDXGIFactory6? dxgiFactory);

            if (factoryResult.Failure)
                throw new Exception($"Failed to create DXGI Factory: {factoryResult.Code}");

            DxgiFactory = dxgiFactory!;

            Adapter = GetBestAdapter(DxgiFactory);

            SwapChainDescription1 swapChainDescription = new()
            {
                BufferCount = 2,
                Width = (uint)Window.ClientSize.Width,
                Height = (uint)Window.ClientSize.Height,
                Format = Format.R8G8B8A8_UNorm,
                BufferUsage = Usage.RenderTargetOutput,
                SwapEffect = SwapEffect.FlipSequential,
                SampleDescription = new SampleDescription(1, 0),
                Scaling = Scaling.None,
                Stereo = false,
            };

            Result result = D3D11.D3D11CreateDevice(Adapter, DriverType.Unknown, DeviceCreationFlags.BgraSupport, [FeatureLevel.Level_11_0], out ID3D11Device? deviceOut);

            if (result.Failure)
                throw new Exception($"Failed to create D3D11 device: {result.Code}");

            Device = deviceOut!.QueryInterface<ID3D11Device5>();
            Context = Device.ImmediateContext3.QueryInterface<ID3D11DeviceContext4>();

            IDXGISwapChain1 swapChainOut = DxgiFactory.CreateSwapChainForHwnd(Device.QueryInterface<IDXGIDevice>(), Window.Handle, swapChainDescription);

            SwapChain = swapChainOut!.QueryInterface<IDXGISwapChain4>();

            CreateRenderTarget();
            CreateDepthStencilBuffer();

            DepthStencilDescription depthStencilDescription = new()
            {
                DepthEnable = false,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthFunc = ComparisonFunction.Always,
                StencilEnable = false,
            };

            NoDepthStencilState = Device.CreateDepthStencilState(depthStencilDescription);

            BlendDescription blendDesc = new()
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false,
            };

            var renderTargetBlendDescription = new RenderTargetBlendDescription
            {
                BlendEnable = true,
                SourceBlend = Blend.One,
                DestinationBlend = Blend.InverseSourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceBlendAlpha = Blend.One,
                DestinationBlendAlpha = Blend.Zero,
                BlendOperationAlpha = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteEnable.All
            };

            blendDesc.RenderTarget.e0 = renderTargetBlendDescription;
            blendDesc.RenderTarget.e1 = renderTargetBlendDescription;
            blendDesc.RenderTarget.e2 = renderTargetBlendDescription;
            blendDesc.RenderTarget.e3 = renderTargetBlendDescription;
            blendDesc.RenderTarget.e4 = renderTargetBlendDescription;
            blendDesc.RenderTarget.e5 = renderTargetBlendDescription;
            blendDesc.RenderTarget.e6 = renderTargetBlendDescription;
            blendDesc.RenderTarget.e7 = renderTargetBlendDescription;

            AlphaBlendState = Device.CreateBlendState(blendDesc);

            Context.OMSetRenderTargets(RenderTargetView);
            Context.RSSetViewports([new Viewport(0, 0, Window.ClientSize.Width, Window.ClientSize.Height, 0.0f, 1.0f)]);
        }

        public static void Resize()
        {
            RenderTargetView?.Dispose();
            DepthStencilView?.Dispose();

            SwapChain.ResizeBuffers(0, (uint)Window.ClientSize.Width, (uint)Window.ClientSize.Height, Format.Unknown, SwapChainFlags.None);

            CreateRenderTarget();
            CreateDepthStencilBuffer();

            Context.RSSetViewports([new Viewport(0, 0, Window.ClientSize.Width, Window.ClientSize.Height, 0.0f, 1.0f)]);
        }

        public static void PreRender()
        {
            Context.OMSetRenderTargets(RenderTargetView, DepthStencilView);
            Context.ClearRenderTargetView(RenderTargetView, new(0.0f, 0.45f, 0.75f, 1.0f));
            Context.ClearDepthStencilView(DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
        }

        public static void PostRender()
        {
            SwapChain.Present(1, PresentFlags.None);
        }

        private static void CreateDepthStencilBuffer()
        {
            Texture2DDescription depthStencilDescription = new()
            {
                Width = (uint)Window.ClientSize.Width,
                Height = (uint)Window.ClientSize.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.D24_UNorm_S8_UInt,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None
            };

            ID3D11Texture2D depthStencilBuffer = Device.CreateTexture2D(depthStencilDescription);

            DepthStencilView = Device.CreateDepthStencilView(depthStencilBuffer);
            depthStencilBuffer.Dispose();

            Context.OMSetRenderTargets(RenderTargetView, DepthStencilView);
        }

        private static void CreateRenderTarget()
        {
            using (var backBuffer = SwapChain.GetBuffer<ID3D11Texture2D1>(0))
                RenderTargetView = Device.CreateRenderTargetView1(backBuffer);

            Context.OMSetRenderTargets(RenderTargetView);
        }

        private static IDXGIAdapter4 GetBestAdapter(IDXGIFactory6 factory)
        {
            IDXGIAdapter4 bestAdapter = null!;
            for (uint i = 0; factory.EnumAdapterByGpuPreference(i, GpuPreference.HighPerformance, out IDXGIAdapter4? adapter).Success; i++)
            {
                try
                {
                    var desc = adapter!.Description3;

                    if (desc.DedicatedVideoMemory > 0 || desc.SharedSystemMemory > 0)
                    {
                        bestAdapter = adapter;
                        break;
                    }
                }
                catch (ArithmeticException)
                {
                    bestAdapter = adapter!;
                }
            }

            if (bestAdapter == null)
                throw new Exception("No suitable adapter found");

            return bestAdapter;
        }
        
        public static void CleanUp()
        {
            Device?.Dispose();
            Context?.Dispose();
            SwapChain?.Dispose();
            RenderTargetView?.Dispose();
            DepthStencilView?.Dispose();
            DxgiFactory?.Dispose();
            Adapter?.Dispose();
        }
    }
}