using Invasion.Math;
using Invasion.Render;

namespace Invasion.UI.Elements
{
    public class UIImage : UIElement
    {
        public UIImage(string name, Texture texture, Vector2f position, Vector2f size, Alignment alignment = Alignment.None, Vector2f alignmentOffset = default) : base(name, position, size, alignment)
        {
            AlignmentOffset = alignmentOffset;
            Mesh.Texture = texture;
            Mesh.Generate();
        }
    }
}
