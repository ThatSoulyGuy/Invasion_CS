using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Invasion.Page
{
    public partial class Window : Form
    {
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

        public Window()
        {
            InitializeComponent();

            UpdateTimer.Interval = 1000 / 60;
            UpdateTimer.Tick += (sender, e) =>
            {
                foreach (var updateFunction in UpdateFunctions)
                    updateFunction();
            };

            RenderTimer.Interval = 1000 / 60;
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
    }
}
