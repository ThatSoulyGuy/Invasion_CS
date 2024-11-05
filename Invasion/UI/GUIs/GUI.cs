using System.Collections.Generic;

namespace Invasion.UI.GUIs
{
    public class GUI
    {
        public string Name { get; init; } = string.Empty;

        private bool _isActive = false;

        public bool IsActive 
        { 
            get => _isActive;
            set
            {
                _isActive = value;

                foreach (var item in Elements)
                    item.IsVisible = value;
            }
        }

        private List<UIElement> Elements { get; } = [];

        public GUI AddElement(UIElement element)
        {
            element.IsVisible = IsActive;
            Elements.Add(element);

            return this;
        }

        public UIElement? GetElement(string name)
        {
            return Elements.Find(x => x.Name == name);
        }

        public void RemoveElement(UIElement element)
        {
            element.IsVisible = false;
            Elements.Remove(element);
        }

        public static GUI Create(string name)
        {
            return new()
            {
                Name = name
            };
        }
    }
}
