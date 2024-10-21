using Invasion.Math;
using Invasion.Render;

namespace Invasion.UI.Elements
{
    public class UIImage : UIElement
    {
        public UIImage(string name, Texture texture, Vector2f position, Vector2f size) : base(name, position, size)
        {
            Mesh.Texture = texture;

            Mesh.Generate();
        }
    }
}
