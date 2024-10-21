using Invasion.Render;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Invasion.UI
{
    public static class UIManager
    {
        private static List<UIElement> UIElements { get; } = [];

        public static void AddElement(UIElement element)
        {
            UIElements.Add(element);
        }

        public static void RemoveElement(UIElement element)
        {
            UIElements.Remove(element);
        }

        public static void Update()
        {
            foreach (var element in UIElements)
            {
                element.Update();
            }
        }

        public static void Render()
        {
            foreach (var element in UIElements)
                element.Mesh.Render(Matrix4x4.CreateOrthographicOffCenter(0, Renderer.Window.Width, 0, Renderer.Window.Height, 0.0f, 1.0f));
        }

        public static void CleanUp()
        {
            foreach (var element in UIElements)
                element.Uninitialize();
        }
    }
}
