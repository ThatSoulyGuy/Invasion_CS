using Invasion.Render;
using Invasion.UI.Elements;
using System.Collections.Generic;

namespace Invasion.UI.GUIs
{
    public static class GUIList
    {
        public static Dictionary<string, GUI> GUIs { get; } = [];

        static GUIList()
        {
            float hotbarBackgroundWidth = 249 * 4;
            float hotbarSelectorWidth = 32 * 4;

            int notchIndex = 0;
            int numberOfNotches = 8;
            float notchWidth = hotbarBackgroundWidth / numberOfNotches;

            float offset = -((hotbarBackgroundWidth / 2) - (notchWidth / 2)) + (notchWidth * notchIndex) - (hotbarSelectorWidth / 2);

            GUIs.Add("hud", GUI.Create("hud")
                .AddElement(new UIImage("hotbarBackground", TextureManager.Get<Texture>("hotbar_background"), new(0, 15), new Math.Vector2f(249, 32) * 4, Alignment.CenterX | Alignment.Bottom))
                .AddElement(new UIImage("hotbarSelector", TextureManager.Get<Texture>("hotbar_selector"), new(0, 15), new Math.Vector2f(32, 32) * 4, Alignment.CenterX | Alignment.Bottom, new(offset, 0))));
        }

        public static void AddGUI(GUI gui)
        {
            GUIs.Add(gui.Name, gui);
        }
    }
}