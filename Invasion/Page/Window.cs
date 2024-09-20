using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Invasion.Page
{
    public partial class Window : Form
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DEVMODE
        {
            private const int CCHDEVICENAME = 32;
            private const int CCHFORMNAME = 32;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string dmDeviceName;
            public ushort dmSpecVersion;
            public ushort dmDriverVersion;
            public ushort dmSize;
            public ushort dmDriverExtra;
            public uint dmFields;

            public int dmPositionX;
            public int dmPositionY;
            public uint dmDisplayOrientation;
            public uint dmDisplayFixedOutput;

            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;
            public ushort dmLogPixels;
            public uint dmBitsPerPel;
            public uint dmPelsWidth;
            public uint dmPelsHeight;
            public uint dmDisplayFlags;
            public uint dmDisplayFrequency;

            public uint dmICMMethod;
            public uint dmICMIntent;
            public uint dmMediaType;
            public uint dmDitherType;
            public uint dmReserved1;
            public uint dmReserved2;

            public uint dmPanningWidth;
            public uint dmPanningHeight;
        }

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        public new event EventHandler Update
        {
            add => UpdateFunctions.Add(() => value(this, EventArgs.Empty));
            remove => UpdateFunctions.Remove(() => value(this, EventArgs.Empty));
        }

        public new event EventHandler Resize
        {
            add => ResizeFunctions.Add(() => value(this, EventArgs.Empty));
            remove => ResizeFunctions.Remove(() => value(this, EventArgs.Empty));
        }

        public event EventHandler Render
        {
            add => RenderFunctions.Add(() => value(this, EventArgs.Empty));
            remove => RenderFunctions.Remove(() => value(this, EventArgs.Empty));
        }

        public event EventHandler CleanUp
        {
            add => CleanUpFunctions.Add(() => value(this, EventArgs.Empty));
            remove => CleanUpFunctions.Remove(() => value(this, EventArgs.Empty));
        }

        private List<Action> UpdateFunctions { get; } = [];
        private List<Action> ResizeFunctions { get; } = [];
        private List<Action> RenderFunctions { get; } = [];
        private List<Action> CleanUpFunctions { get; } = [];

        private Timer UpdateTimer { get; } = new();
        private Timer RenderTimer { get; } = new();

        public int RefreshRate { get; } = 60;

        public Window()
        {
            InitializeComponent();

            RefreshRate = GetPrimaryMonitorRefreshRate();

            int frameInterval = (int)(1000.0 / RefreshRate);

            UpdateTimer.Interval = frameInterval;
            UpdateTimer.Tick += (sender, e) =>
            {
                foreach (var updateFunction in UpdateFunctions)
                    updateFunction();
            };

            RenderTimer.Interval = frameInterval;
            RenderTimer.Tick += (sender, e) =>
            {
                foreach (var renderFunction in RenderFunctions)
                    renderFunction();
            };

            Update += (sender, e) => Invalidate();
            Render += (sender, e) => Refresh();

            UpdateTimer.Start();
            RenderTimer.Start();

            FormClosed += (sender, e) =>
            {
                foreach (var cleanUpFunction in CleanUpFunctions)
                    cleanUpFunction();
            };

            SizeChanged += (sender, e) =>
            {
                foreach (var resizeFunction in ResizeFunctions)
                    resizeFunction();
            };
        }

        private int GetPrimaryMonitorRefreshRate()
        {
            DEVMODE devMode = new()
            {
                dmSize = (ushort)Marshal.SizeOf(typeof(DEVMODE))
            };

            if (EnumDisplaySettings(null!, -1, ref devMode))
                return (int)devMode.dmDisplayFrequency;
            
            return 60;
        }
    }
}